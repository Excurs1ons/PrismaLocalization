using FluentAssertions;
using Xunit;
using Newtonsoft.Json;

namespace PrismaLocalization.Tests;

public class LocalizationWorkflowTests
{
    private readonly LocalizationWorkflow _workflow;

    public LocalizationWorkflowTests()
    {
        _workflow = new LocalizationWorkflow
        {
            Namespace = "TestNamespace",
            DefaultCategory = LocalizationCategory.General
        };
    }

    [Fact]
    public void AddSource_ShouldCreateEntry()
    {
        // Act
        var entry = _workflow.AddSource("Hello World");

        // Assert
        entry.Should().NotBeNull();
        entry.Source.Should().Be("Hello World");
        entry.Key.Should().Be("HelloWorld");
        entry.Namespace.Should().Be("TestNamespace");
    }

    [Fact]
    public void AddSource_WithCustomKey_ShouldUseCustomKey()
    {
        // Act
        var entry = _workflow.AddSourceWithKey("Hello World", "CustomKey");

        // Assert
        entry.Key.Should().Be("CustomKey");
    }

    [Fact]
    public void AddSource_ShouldGeneratePascalCaseKey()
    {
        // Arrange & Act
        var entry1 = _workflow.AddSource("hello world");
        var entry2 = _workflow.AddSource("test_key_name");
        var entry3 = _workflow.AddSource("test-key-name");

        // Assert
        entry1.Key.Should().Be("HelloWorld");
        entry2.Key.Should().Be("TestKeyName");
        entry3.Key.Should().Be("TestKeyName");
    }

    [Fact]
    public void AddTranslation_ShouldAddTranslationToEntry()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var entryId = "TestNamespace:HelloWorld";

        // Act
        var success = _workflow.AddTranslation(entryId, "zh-CN", "你好世界");

        // Assert
        success.Should().BeTrue();
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Id == entryId);
        entry.Should().NotBeNull();
        entry!.Translations["zh-CN"].Should().Be("你好世界");
    }

    [Fact]
    public void AddTranslationByKey_ShouldAddTranslation()
    {
        // Arrange
        _workflow.AddSource("Hello World");

        // Act
        var success = _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");

        // Assert
        success.Should().BeTrue();
    }

    [Fact]
    public void MarkAsTranslated_ShouldMoveEntryToTranslated()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var entryId = "TestNamespace:HelloWorld";
        _workflow.AddTranslation(entryId, "zh-CN", "你好世界");

        // Act
        var moved = _workflow.MarkAsTranslated(entryId);

        // Assert
        moved.Should().BeTrue();
        _workflow.GetPendingEntries().Should().NotContain(e => e.Id == entryId);
        _workflow.GetTranslatedEntries().Should().Contain(e => e.Id == entryId);
    }

    [Fact]
    public void GetPendingEntries_ShouldReturnUntranslatedEntries()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddSource("Goodbye");

        // Act
        var pending = _workflow.GetPendingEntries().ToList();

        // Assert
        pending.Should().HaveCount(2);
    }

    [Fact]
    public void GetTranslatedEntries_ShouldReturnTranslatedEntries()
    {
        // Arrange
        var entryId = "TestNamespace:HelloWorld";
        _workflow.AddSource("Hello World");
        _workflow.AddTranslation(entryId, "zh-CN", "你好世界");
        _workflow.MarkAsTranslated(entryId);

        // Act
        var translated = _workflow.GetTranslatedEntries().ToList();

        // Assert
        translated.Should().HaveCount(1);
        translated[0].Id.Should().Be(entryId);
    }

    [Fact]
    public void GetAllEntries_ShouldReturnAllEntries()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddSource("Goodbye");
        var entryId = "TestNamespace:HelloWorld";
        _workflow.AddTranslation(entryId, "zh-CN", "你好世界");
        _workflow.MarkAsTranslated(entryId);

        // Act
        var all = _workflow.GetAllEntries().ToList();

        // Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    public void GetStatistics_ShouldReturnCorrectCounts()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddSource("Goodbye");
        _workflow.AddSource("Thank You");
        _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");
        _workflow.AddTranslationByKey("Goodbye", "zh-CN", "再见");
        _workflow.MarkAsTranslated("TestNamespace:HelloWorld");
        _workflow.MarkAsTranslated("TestNamespace:Goodbye");

        // Act
        var (total, translated, pending) = _workflow.GetStatistics();

        // Assert
        total.Should().Be(3);
        translated.Should().Be(2);
        pending.Should().Be(1);
    }

    [Fact]
    public void CreateKey_ShouldCreateLocalizationKey()
    {
        // Arrange & Act
        var key = _workflow.CreateKey("Hello World");

        // Assert
        key.Namespace.Should().Be("TestNamespace");
        key.Key.Should().Be("HelloWorld");
        key.DefaultValue.Should().Be("Hello World");
    }

    [Fact]
    public void CreateKey_WithCustomKey_ShouldUseCustomKey()
    {
        // Arrange & Act
        var key = _workflow.CreateKey("Hello World", "CustomKey");

        // Assert
        key.Key.Should().Be("CustomKey");
    }

    [Fact]
    public void CreateKey_WithCategory_ShouldUseCategory()
    {
        // Arrange & Act
        var key = _workflow.CreateKey("Hello World", category: LocalizationCategory.UI);

        // Assert
        key.Category.Should().Be(LocalizationCategory.UI);
    }

    [Fact]
    public void AddNounWithPlural_ShouldCreateEntryWithVariants()
    {
        // Act
        var entry = _workflow.AddNounWithPlural("Item", "Items");

        // Assert
        entry.Source.Should().Be("Item");
        entry.Category.Should().Be("Noun");
        entry.SupportedVariants.Should().Contain("Singular");
        entry.SupportedVariants.Should().Contain("Plural");
        entry.VariantTranslations[":_sg"].Should().Be("Item");
        entry.VariantTranslations[":_pl"].Should().Be("Items");
    }

    [Fact]
    public void AddNounWithPlural_WithCustomKey_ShouldUseCustomKey()
    {
        // Act
        var entry = _workflow.AddNounWithPlural("Item", "Items", "ItemKey");

        // Assert
        entry.Key.Should().Be("ItemKey");
    }

    [Fact]
    public void AddPronounWithDeclensions_ShouldCreateEntryWithVariants()
    {
        // Act
        var entry = _workflow.AddPronounWithDeclensions("I", "me");

        // Assert
        entry.Source.Should().Be("I");
        entry.Category.Should().Be("Pronoun");
        entry.SupportedVariants.Should().Contain("Nominative");
        entry.SupportedVariants.Should().Contain("Accusative");
        entry.VariantTranslations[":_nom"].Should().Be("I");
        entry.VariantTranslations[":_acc"].Should().Be("me");
    }

    [Fact]
    public void AddPronounWithDeclensions_WithAllCases_ShouldIncludeAllVariants()
    {
        // Act
        var entry = _workflow.AddPronounWithDeclensions("I", "me", "mine", "my");

        // Assert
        entry.SupportedVariants.Should().Contain("Genitive");
        entry.SupportedVariants.Should().Contain("PossessiveDeterminer");
        entry.VariantTranslations[":_gen"].Should().Be("mine");
        entry.VariantTranslations[":_posdet"].Should().Be("my");
    }

    [Fact]
    public void SetPluralTranslation_ShouldAddVariantTranslations()
    {
        // Arrange
        _workflow.AddNounWithPlural("Item", "Items", "Item");

        // Act
        var success = _workflow.SetPluralTranslation("Item", "zh-CN", "物品", "物品们");

        // Assert
        success.Should().BeTrue();
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Key == "Item");
        entry.Should().NotBeNull();
        entry!.Translations["zh-CN"].Should().Be("物品");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.Singular).Should().Be("物品");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.Plural).Should().Be("物品们");
    }

    [Fact]
    public void SetPronounDeclensions_ShouldAddVariantTranslations()
    {
        // Arrange
        _workflow.AddPronounWithDeclensions("I", "me", "mine", "my", "I");

        // Act
        var success = _workflow.SetPronounDeclensions("I", "zh-CN", "我", "我", "我的", "我的");

        // Assert
        success.Should().BeTrue();
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Key == "I");
        entry.Should().NotBeNull();
        entry!.GetVariantTranslation("zh-CN", LocalizationVariant.Nominative).Should().Be("我");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.Accusative).Should().Be("我");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.Genitive).Should().Be("我的");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.PossessiveDeterminer).Should().Be("我的");
    }

    [Fact]
    public void ExportToLocalizationJson_ShouldCreateValidJson()
    {
        // Arrange
        _workflow.AddSource("Hello World", "Test context");
        _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");

        // Act
        var json = _workflow.ExportToLocalizationJson();

        // Assert
        json.Should().NotBeEmpty();
        var data = JsonConvert.DeserializeObject<LocalizationData>(json);
        data.Should().NotBeNull();
        data!.Entries.Should().HaveCount(1);
        data.Entries[0].Key.Should().Be("HelloWorld");
        data.Entries[0].Context.Should().Be("Test context");
    }

    [Fact]
    public void ExportToTranslationTable_ShouldCreateValidTable()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");
        var cultures = new[] { "zh-CN", "en-US" };

        // Act
        var table = _workflow.ExportToTranslationTable(cultures);

        // Assert
        table.Should().Contain("Key");
        table.Should().Contain("Source");
        table.Should().Contain("zh-CN");
        table.Should().Contain("en-US");
        table.Should().Contain("Hello World");
    }

    [Fact]
    public void ImportFromJson_ShouldImportTranslations()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var jsonData = @"{
            ""culture"": ""zh-CN"",
            ""entries"": [
                {
                    ""key"": ""HelloWorld"",
                    ""namespace"": ""TestNamespace"",
                    ""category"": ""General"",
                    ""text"": ""你好世界""
                }
            ]
        }";

        // Act
        _workflow.ImportFromJson(jsonData, "zh-CN");

        // Assert
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Key == "HelloWorld");
        entry.Should().NotBeNull();
        entry!.Translations.Should().ContainKey("zh-CN");
        entry.Translations["zh-CN"].Should().Be("你好世界");
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddSource("Goodbye");

        // Act
        _workflow.Clear();

        // Assert
        _workflow.GetAllEntries().Should().BeEmpty();
    }

    [Fact]
    public void Namespace_ShouldBeUsedInEntryId()
    {
        // Arrange
        _workflow.Namespace = "CustomNamespace";
        var entry = _workflow.AddSource("Test");

        // Act
        var entryId = $"{_workflow.Namespace}:Test";

        // Assert
        entry.Id.Should().Be(entryId);
    }

    [Fact]
    public void DefaultCategory_ShouldBeUsedInNewEntries()
    {
        // Arrange
        _workflow.DefaultCategory = LocalizationCategory.UI;
        var entry = _workflow.AddSource("Test");

        // Assert
        entry.Category.Should().Be("UI");
    }

    [Fact]
    public void WorkflowEntry_ToLocalizationKey_ShouldCreateValidKey()
    {
        // Arrange
        var entry = new LocalizationWorkflow.WorkflowEntry
        {
            Namespace = "Ns",
            Key = "MyKey",
            Category = "UI",
            Source = "Test Text"
        };

        // Act
        var key = entry.ToLocalizationKey();

        // Assert
        key.Namespace.Should().Be("Ns");
        key.Key.Should().Be("MyKey");
        key.Category.Should().Be(LocalizationCategory.UI);
        key.DefaultValue.Should().Be("Test Text");
    }

    [Fact]
    public void WorkflowEntry_ToLocalizationKey_WithVariant_ShouldIncludeVariant()
    {
        // Arrange
        var entry = new LocalizationWorkflow.WorkflowEntry
        {
            Namespace = "Ns",
            Key = "MyKey",
            Category = "Noun",
            Source = "Item"
        };

        // Act
        var key = entry.ToLocalizationKey(LocalizationVariant.Plural);

        // Assert
        key.Variant.Should().Be(LocalizationVariant.Plural);
    }

    [Fact]
    public void WorkflowEntry_GetVariantTranslation_ShouldReturnCorrectVariant()
    {
        // Arrange
        var entry = new LocalizationWorkflow.WorkflowEntry
        {
            Namespace = "Ns",
            Key = "Item",
            Category = "Noun",
            Source = "Item",
            Translations = new Dictionary<string, string> { ["en-US"] = "Item" },
            VariantTranslations = new Dictionary<string, string>
            {
                ["en-US:_sg"] = "Item",
                ["en-US:_pl"] = "Items"
            }
        };

        // Act
        var singular = entry.GetVariantTranslation("en-US", LocalizationVariant.Singular);
        var plural = entry.GetVariantTranslation("en-US", LocalizationVariant.Plural);

        // Assert
        singular.Should().Be("Item");
        plural.Should().Be("Items");
    }

    [Fact]
    public void WorkflowEntry_SetVariantTranslation_ShouldAddVariant()
    {
        // Arrange
        var entry = new LocalizationWorkflow.WorkflowEntry
        {
            Namespace = "Ns",
            Key = "Item",
            Category = "Noun",
            Source = "Item"
        };

        // Act
        entry.SetVariantTranslation("zh-CN", LocalizationVariant.Plural, "物品们");

        // Assert
        entry.SupportedVariants.Should().Contain("Plural");
        entry.GetVariantTranslation("zh-CN", LocalizationVariant.Plural).Should().Be("物品们");
    }
}
