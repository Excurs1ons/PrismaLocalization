using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class LocalizationManagerTests : IDisposable
{
    private readonly LocalizationManager _manager;

    public LocalizationManagerTests()
    {
        _manager = new LocalizationManager();
    }

    public void Dispose()
    {
        _manager.Dispose();
    }

    [Fact]
    public void Instance_ShouldReturnSingleton()
    {
        // Arrange & Act
        var instance1 = LocalizationManager.Instance;
        var instance2 = LocalizationManager.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void CurrentCulture_ShouldDefaultToCurrentCulture()
    {
        // Arrange & Act
        var culture = _manager.CurrentCulture;

        // Assert
        culture.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CurrentCulture_Set_ShouldChangeCulture()
    {
        // Arrange
        var newCulture = "zh-CN";

        // Act
        _manager.CurrentCulture = newCulture;

        // Assert
        _manager.CurrentCulture.Should().Be(newCulture);
    }

    [Fact]
    public void AddProvider_ShouldAddProvider()
    {
        // Arrange
        var provider = new TestLocalizationProvider();

        // Act
        _manager.AddProvider(provider);

        // Assert
        _manager.GetAvailableCultures().Should().Contain("en-US");
    }

    [Fact]
    public void RemoveProvider_ShouldRemoveProvider()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);

        // Act
        var removed = _manager.RemoveProvider(provider);

        // Assert
        removed.Should().BeTrue();
    }

    [Fact]
    public void GetText_WithSimpleKey_ShouldReturnTemplate()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);
        var key = new LocalizationKey("HelloWorld", defaultValue: "Hello World");

        // Act
        var text = _manager.GetText(key);

        // Assert
        text.Should().Be("Hello World");
    }

    [Fact]
    public void GetText_WithNamedArguments_ShouldFormat()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);
        var key = new LocalizationKey("Greeting", defaultValue: "Hello, {name}!");
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var text = _manager.GetText(key, args);

        // Assert
        text.Should().Be("Hello, World!");
    }

    [Fact]
    public void GetText_WithIndexedArguments_ShouldFormat()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);
        var key = new LocalizationKey("Sum", defaultValue: "{0} + {1} = {2}");

        // Act
        var text = _manager.GetText(key, 2, 3, 5);

        // Assert
        text.Should().Be("2 + 3 = 5");
    }

    [Fact]
    public void GetText_WithCulture_ShouldUseCorrectCulture()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);
        var key = new LocalizationKey("Hello");

        // Act
        var enText = _manager.GetText(key, "en-US");
        var zhText = _manager.GetText(key, "zh-CN");

        // Assert
        enText.Should().Be("Hello");
        zhText.Should().Be("你好");
    }

    [Fact]
    public void GetText_WithVariantKey_ShouldUseVariant()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);
        var key = new LocalizationKey("Item", LocalizationVariant: LocalizationVariant.Plural);

        // Act
        var text = _manager.GetText(key);

        // Assert
        text.Should().Be("Items");
    }

    [Fact]
    public void HasTemplate_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);

        // Act
        var hasTemplate = _manager.HasTemplate(new LocalizationKey("Hello"));

        // Assert
        hasTemplate.Should().BeTrue();
    }

    [Fact]
    public void HasTemplate_WithNonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);

        // Act
        var hasTemplate = _manager.HasTemplate(new LocalizationKey("NonExistent"));

        // Assert
        hasTemplate.Should().BeFalse();
    }

    [Fact]
    public void GetAvailableCultures_ShouldReturnAllCultures()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);

        // Act
        var cultures = _manager.GetAvailableCultures();

        // Assert
        cultures.Should().Contain("en-US");
        cultures.Should().Contain("zh-CN");
    }

    [Fact]
    public void Reload_ShouldReloadAllProviders()
    {
        // Arrange
        var provider = new TestLocalizationProvider();
        _manager.AddProvider(provider);

        // Act
        _manager.Reload();

        // Assert - should not throw
        _manager.HasTemplate(new LocalizationKey("Hello")).Should().BeTrue();
    }

    [Fact]
    public void GetText_WithNoProviders_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = new LocalizationKey("Test", defaultValue: "Default");

        // Act
        var text = _manager.GetText(key);

        // Assert
        text.Should().Be("Default");
    }

    [Fact]
    public void GetText_WithNoProvidersAndNoDefault_ShouldReturnKey()
    {
        // Arrange
        var key = new LocalizationKey("TestKey");

        // Act
        var text = _manager.GetText(key);

        // Assert
        text.Should().Be("TestKey");
    }

    [Fact]
    public void GetText_WithMultipleProviders_ShouldTryFirstMatch()
    {
        // Arrange
        var provider1 = new TestLocalizationProvider();
        var provider2 = new TestLocalizationProvider();
        _manager.AddProvider(provider1);
        _manager.AddProvider(provider2);
        var key = new LocalizationKey("Hello");

        // Act
        var text = _manager.GetText(key);

        // Assert
        text.Should().Be("Hello"); // From first provider
    }

    // Test implementation of ILocalizationProvider
    private class TestLocalizationProvider : ILocalizationProvider
    {
        private readonly Dictionary<string, string> _translations = new()
        {
            ["Hello"] = "Hello",
            ["你好"] = "你好",
            ["Item"] = "Item",
            ["Item_pl"] = "Items",
            ["Item_sg"] = "Item"
        };

        public string? GetTemplate(LocalizationKey key, string? culture = null)
        {
            culture ??= "en-US";

            // Try variant key first
            if (key.Variant != LocalizationVariant.None)
            {
                var variantKey = $"{key.FullKey}{key.Variant.ToSuffix()}";
                if (_translations.TryGetValue(variantKey, out var variantTemplate))
                    return variantTemplate;
            }

            // Try full key
            if (_translations.TryGetValue(key.FullKey, out var template))
                return template;

            // Try key without namespace
            if (_translations.TryGetValue(key.Key, out var keyTemplate))
                return keyTemplate;

            return key.DefaultValue ?? key.FullKey;
        }

        public bool HasTemplate(LocalizationKey key, string? culture = null)
        {
            return GetTemplate(key, culture) != null;
        }

        public IEnumerable<string> GetAvailableCultures()
        {
            return new[] { "en-US", "zh-CN" };
        }

        public void Reload()
        {
            // Test implementation - no-op
        }
    }
}
