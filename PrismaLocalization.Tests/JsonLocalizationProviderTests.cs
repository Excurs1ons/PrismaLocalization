using FluentAssertions;
using Xunit;
using Newtonsoft.Json;

namespace PrismaLocalization.Tests;

public class JsonLocalizationProviderTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly JsonLocalizationProvider _provider;

    public JsonLocalizationProviderTests()
    {
        _testDirectory = "/tmp/prisma_localization_test";
        Directory.CreateDirectory(_testDirectory);
        _provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeProvider()
    {
        // Assert
        _provider.Should().NotBeNull();
        _provider.GetAvailableCultures().Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableCultures_WithNoFiles_ShouldReturnEmpty()
    {
        // Act
        var cultures = _provider.GetAvailableCultures();

        // Assert
        cultures.Should().BeEmpty();
    }

    [Fact]
    public void GetTemplate_WithSimpleJson_ShouldReturnTemplate()
    {
        // Arrange
        var jsonData = @"{
            ""HelloWorld"": ""Hello, World!"",
            ""Goodbye"": ""Goodbye!""
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var template = provider.GetTemplate(new LocalizationKey("HelloWorld"));

        // Assert
        template.Should().Be("Hello, World!");
    }

    [Fact]
    public void GetTemplate_WithNestedJson_ShouldFlattenKeys()
    {
        // Arrange
        var jsonData = @"{
            ""Menu"": {
                ""Start"": ""Start Game"",
                ""Quit"": ""Quit""
            }
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var template = provider.GetTemplate(new LocalizationKey("Menu.Start"));

        // Assert
        template.Should().Be("Start Game");
    }

    [Fact]
    public void GetTemplate_WithVariantKey_ShouldTryVariantFirst()
    {
        // Arrange
        var jsonData = @"{
            ""Item"": ""Item"",
            ""Item_pl"": ""Items""
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var singularTemplate = provider.GetTemplate(new LocalizationKey("Item", LocalizationVariant: LocalizationVariant.Singular));
        var pluralTemplate = provider.GetTemplate(new LocalizationKey("Item", LocalizationVariant: LocalizationVariant.Plural));

        // Assert
        singularTemplate.Should().Be("Item");
        pluralTemplate.Should().Be("Items");
    }

    [Fact]
    public void GetTemplate_WithNamespace_ShouldMatchFullKey()
    {
        // Arrange
        var jsonData = @"{
            ""UI"": {
                ""Start"": ""Start""
            },
            ""Start"": ""Generic Start""
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var template = provider.GetTemplate(new LocalizationKey("UI", "Start"));

        // Assert
        template.Should().Be("Start");
    }

    [Fact]
    public void GetTemplate_WithNonExistentKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = new LocalizationKey("NonExistent", defaultValue: "Default Value");

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var template = provider.GetTemplate(key);

        // Assert
        template.Should().Be("Default Value");
    }

    [Fact]
    public void GetTemplate_WithNonExistentKeyAndNoDefault_ShouldReturnKey()
    {
        // Arrange
        var key = new LocalizationKey("NonExistent");

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var template = provider.GetTemplate(key);

        // Assert
        template.Should().Be("NonExistent");
    }

    [Fact]
    public void GetTemplate_WithCultureFallback_ShouldTryParentCulture()
    {
        // Arrange
        var parentData = @"{ ""Hello"": ""Hello"" }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en.json"), parentData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act - Try to get en-US when only en exists
        var template = provider.GetTemplate(new LocalizationKey("Hello"), "en-US");

        // Assert
        template.Should().Be("Hello");
    }

    [Fact]
    public void HasTemplate_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var jsonData = @"{ ""TestKey"": ""Test Value"" }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var hasTemplate = provider.HasTemplate(new LocalizationKey("TestKey"));

        // Assert
        hasTemplate.Should().BeTrue();
    }

    [Fact]
    public void HasTemplate_WithNonExistentKey_ShouldReturnFalse()
    {
        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var hasTemplate = provider.HasTemplate(new LocalizationKey("NonExistent"));

        // Assert
        hasTemplate.Should().BeFalse();
    }

    [Fact]
    public void SetTranslation_ShouldAddTranslation()
    {
        // Arrange
        var key = new LocalizationKey("NewKey");

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        provider.SetTranslation(key, "en-US", "New Value");

        // Assert
        var template = provider.GetTemplate(key, "en-US");
        template.Should().Be("New Value");
    }

    [Fact]
    public void SetTranslation_WithVariant_ShouldSetVariantKey()
    {
        // Arrange
        var key = new LocalizationKey("Item", LocalizationVariant: LocalizationVariant.Plural);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        provider.SetTranslation(key, "en-US", "Items");

        // Assert
        var template = provider.GetTemplate(key, "en-US");
        template.Should().Be("Items");
    }

    [Fact]
    public void Reload_ShouldReloadFiles()
    {
        // Arrange
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Add file after creation
        var jsonData = @"{ ""NewKey"": ""New Value"" }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Act
        provider.Reload();

        // Assert
        provider.HasTemplate(new LocalizationKey("NewKey")).Should().BeTrue();
    }

    [Fact]
    public void GetKeys_ShouldReturnAllKeysForCulture()
    {
        // Arrange
        var jsonData = @"{
            ""Key1"": ""Value1"",
            ""Key2"": ""Value2""
        }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), jsonData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var keys = provider.GetKeys("en-US");

        // Assert
        keys.Should().Contain("Key1");
        keys.Should().Contain("Key2");
    }

    [Fact]
    public void Constructor_WithDataDictionary_ShouldLoadData()
    {
        // Arrange
        var data = new Dictionary<string, string>
        {
            ["zh-CN"] = @"{ ""Hello"": ""你好"" }",
            ["en-US"] = @"{ ""Hello"": ""Hello"" }"
        };

        // Act
        var provider = new JsonLocalizationProvider(data);

        // Assert
        provider.GetTemplate(new LocalizationKey("Hello"), "zh-CN").Should().Be("你好");
        provider.GetTemplate(new LocalizationKey("Hello"), "en-US").Should().Be("Hello");
    }

    [Fact]
    public void GetTemplate_WithMultipleCultures_ShouldReturnCorrectCulture()
    {
        // Arrange
        var enData = @"{ ""Hello"": ""Hello"" }";
        var zhData = @"{ ""Hello"": ""你好"" }";
        File.WriteAllText(Path.Combine(_testDirectory, "loc.en-US.json"), enData);
        File.WriteAllText(Path.Combine(_testDirectory, "loc.zh-CN.json"), zhData);

        // Recreate provider to load files
        var provider = new JsonLocalizationProvider(_testDirectory, "loc.{culture}.json");

        // Act
        var enTemplate = provider.GetTemplate(new LocalizationKey("Hello"), "en-US");
        var zhTemplate = provider.GetTemplate(new LocalizationKey("Hello"), "zh-CN");

        // Assert
        enTemplate.Should().Be("Hello");
        zhTemplate.Should().Be("你好");
    }

    [Theory]
    [InlineData("loc.{culture}.json", "loc.en-US.json", "en-US")]
    [InlineData("localization.{culture}.json", "localization.zh-CN.json", "zh-CN")]
    [InlineData("{culture}.json", "ja-JP.json", "ja-JP")]
    public void Constructor_WithDifferentFilePatterns_ShouldLoadFiles(
        string pattern, string fileName, string expectedCulture)
    {
        // Arrange
        var jsonData = @"{ ""Test"": ""Value"" }";
        File.WriteAllText(Path.Combine(_testDirectory, fileName), jsonData);

        // Act
        var provider = new JsonLocalizationProvider(_testDirectory, pattern);
        var cultures = provider.GetAvailableCultures();

        // Assert
        cultures.Should().Contain(expectedCulture);
    }
}
