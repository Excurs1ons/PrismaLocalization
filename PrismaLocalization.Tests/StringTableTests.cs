using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class StringTableTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithTableId()
    {
        // Arrange & Act
        var table = new StringTable("GameUI");

        // Assert
        table.TableId.Should().Be("GameUI");
        table.Namespace.Should().Be("GameUI");
    }

    [Fact]
    public void Constructor_WithCustomNamespace_ShouldUseCustomNamespace()
    {
        // Arrange & Act
        var table = new StringTable("GameUI", "MyNamespace");

        // Assert
        table.TableId.Should().Be("GameUI");
        table.Namespace.Should().Be("MyNamespace");
    }

    [Fact]
    public void SetEntry_ShouldAddEntry()
    {
        // Arrange
        var table = new StringTable("GameUI");
        var key = new LocalizationKey("MyNamespace", "MyKey");
        var localizedString = new LocalizedString(key);

        // Act
        table.SetEntry("MyKey", localizedString);

        // Assert
        table.HasEntry("MyKey").Should().BeTrue();
    }

    [Fact]
    public void SetEntry_WithLocalizationKey_ShouldAddEntry()
    {
        // Arrange
        var table = new StringTable("GameUI");
        var key = new LocalizationKey("GameUI", "StartButton");

        // Act
        table.SetEntry("StartButton", key);

        // Assert
        table.HasEntry("StartButton").Should().BeTrue();
    }

    [Fact]
    public void GetEntry_ShouldReturnEntry()
    {
        // Arrange
        var table = new StringTable("GameUI");
        var key = new LocalizationKey("GameUI", "TestKey", defaultValue: "Test Value");
        table.SetEntry("TestKey", new LocalizedString(key));

        // Act
        var entry = table.GetEntry("TestKey");

        // Assert
        entry.Should().NotBeNull();
        entry!.Key.Should().Be(key);
    }

    [Fact]
    public void GetEntry_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var table = new StringTable("GameUI");

        // Act
        var entry = table.GetEntry("NonExistent");

        // Assert
        entry.Should().BeNull();
    }

    [Fact]
    public void HasEntry_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("TestKey", new LocalizedString("TestKey"));

        // Act
        var hasEntry = table.HasEntry("TestKey");

        // Assert
        hasEntry.Should().BeTrue();
    }

    [Fact]
    public void HasEntry_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var table = new StringTable("GameUI");

        // Act
        var hasEntry = table.HasEntry("NonExistent");

        // Assert
        hasEntry.Should().BeFalse();
    }

    [Fact]
    public void RemoveEntry_ShouldRemoveEntry()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("TestKey", new LocalizedString("TestKey"));

        // Act
        var removed = table.RemoveEntry("TestKey");

        // Assert
        removed.Should().BeTrue();
        table.HasEntry("TestKey").Should().BeFalse();
    }

    [Fact]
    public void RemoveEntry_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var table = new StringTable("GameUI");

        // Act
        var removed = table.RemoveEntry("NonExistent");

        // Assert
        removed.Should().BeFalse();
    }

    [Fact]
    public void GetAllKeys_ShouldReturnAllKeys()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("Key1", new LocalizedString("Key1"));
        table.SetEntry("Key2", new LocalizedString("Key2"));
        table.SetEntry("Key3", new LocalizedString("Key3"));

        // Act
        var keys = table.GetAllKeys();

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().BeInAscendingOrder();
        keys.Should().Contain("Key1");
        keys.Should().Contain("Key2");
        keys.Should().Contain("Key3");
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("Key1", new LocalizedString("Key1"));
        table.SetEntry("Key2", new LocalizedString("Key2"));

        // Act
        table.Clear();

        // Assert
        table.Count.Should().Be(0);
        table.GetAllKeys().Should().BeEmpty();
    }

    [Fact]
    public void Count_ShouldReturnNumberOfEntries()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("Key1", new LocalizedString("Key1"));
        table.SetEntry("Key2", new LocalizedString("Key2"));

        // Act
        var count = table.Count;

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void ToJson_ShouldCreateValidJson()
    {
        // Arrange
        var table = new StringTable("GameUI");
        var key = new LocalizationKey("GameUI", "TestKey", defaultValue: "Test Value");
        table.SetEntry("TestKey", new LocalizedString(key));

        // Act
        var json = table.ToJson();

        // Assert
        json.Should().NotBeEmpty();
        json.Should().Contain("GameUI");
        json.Should().Contain("TestKey");
        json.Should().Contain("Test Value");
    }

    [Fact]
    public void ToJsonFile_ShouldCreateFile()
    {
        // Arrange
        var table = new StringTable("GameUI");
        table.SetEntry("TestKey", new LocalizedString("TestKey"));
        var filePath = "/tmp/test_table.json";

        // Act
        table.ToJsonFile(filePath);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var content = File.ReadAllText(filePath);
        content.Should().Contain("TestKey");

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void FromJson_ShouldLoadTable()
    {
        // Arrange
        var json = @"{
            ""culture"": ""GameUI"",
            ""entries"": [
                {
                    ""key"": ""StartButton"",
                    ""namespace"": ""GameUI"",
                    ""category"": ""Menu"",
                    ""text"": ""Start Game""
                }
            ]
        }";

        // Act
        var table = StringTable.FromJson(json, "GameUI");

        // Assert
        table.TableId.Should().Be("GameUI");
        table.HasEntry("StartButton").Should().BeTrue();
    }

    [Fact]
    public void FromJson_WithMultipleEntries_ShouldLoadAll()
    {
        // Arrange
        var json = @"{
            ""culture"": ""GameUI"",
            ""entries"": [
                {
                    ""key"": ""StartButton"",
                    ""namespace"": ""Menu"",
                    ""category"": ""Menu"",
                    ""text"": ""Start Game""
                },
                {
                    ""key"": ""QuitButton"",
                    ""namespace"": ""Menu"",
                    ""category"": ""Menu"",
                    ""text"": ""Quit""
                }
            ]
        }";

        // Act
        var table = StringTable.FromJson(json, "Menu");

        // Assert
        table.Count.Should().Be(2);
        table.GetAllKeys().Should().Contain("StartButton");
        table.GetAllKeys().Should().Contain("QuitButton");
    }
}

public class StringTableManagerTests
{
    [Fact]
    public void Instance_ShouldReturnSingleton()
    {
        // Arrange & Act
        var instance1 = StringTableManager.Instance;
        var instance2 = StringTableManager.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void RegisterTable_ShouldAddTable()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        var table = new StringTable("GameUI");

        // Act
        manager.RegisterTable(table);

        // Assert
        manager.HasTable("GameUI").Should().BeTrue();
    }

    [Fact]
    public void UnregisterTable_ShouldRemoveTable()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        var table = new StringTable("GameUI");
        manager.RegisterTable(table);

        // Act
        var unregistered = manager.UnregisterTable("GameUI");

        // Assert
        unregistered.Should().BeTrue();
        manager.HasTable("GameUI").Should().BeFalse();
    }

    [Fact]
    public void GetString_ShouldReturnLocalizedString()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        var table = new StringTable("GameUI");
        var key = new LocalizationKey("GameUI", "TestKey", defaultValue: "Test Value");
        table.SetEntry("TestKey", new LocalizedString(key));
        manager.RegisterTable(table);

        // Act
        var result = manager.GetString("GameUI", "TestKey");

        // Assert
        result.Should().NotBeNull();
        result!.ToString().Should().Be("Test Value");
    }

    [Fact]
    public void GetString_WithNonExistentTable_ShouldReturnNull()
    {
        // Arrange
        var manager = StringTableManager.Instance;

        // Act
        var result = manager.GetString("NonExistent", "Key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetString_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        var table = new StringTable("GameUI");
        manager.RegisterTable(table);

        // Act
        var result = manager.GetString("GameUI", "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTableIds_ShouldReturnAllTableIds()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        manager.RegisterTable(new StringTable("Table1"));
        manager.RegisterTable(new StringTable("Table2"));
        manager.RegisterTable(new StringTable("Table3"));

        // Act
        var ids = manager.GetTableIds();

        // Assert
        ids.Should().HaveCount(3);
        ids.Should().BeInAscendingOrder();
    }

    [Fact]
    public void HasTable_WithExistingTable_ShouldReturnTrue()
    {
        // Arrange
        var manager = StringTableManager.Instance;
        manager.RegisterTable(new StringTable("GameUI"));

        // Act
        var hasTable = manager.HasTable("GameUI");

        // Assert
        hasTable.Should().BeTrue();
    }

    [Fact]
    public void HasTable_WithNonExistentTable_ShouldReturnFalse()
    {
        // Arrange
        var manager = StringTableManager.Instance;

        // Act
        var hasTable = manager.HasTable("NonExistent");

        // Assert
        hasTable.Should().BeFalse();
    }
}
