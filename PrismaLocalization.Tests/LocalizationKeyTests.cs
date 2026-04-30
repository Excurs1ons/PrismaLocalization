using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class LocalizationKeyTests
{
    [Fact]
    public void Constructor_ShouldCreateKeyWithAllParameters()
    {
        // Arrange & Act
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationCategory.UI, LocalizationVariant.Singular, "Default Text");

        // Assert
        key.Namespace.Should().Be("MyNamespace");
        key.Key.Should().Be("MyKey");
        key.Category.Should().Be(LocalizationCategory.UI);
        key.Variant.Should().Be(LocalizationVariant.Singular);
        key.DefaultValue.Should().Be("Default Text");
    }

    [Fact]
    public void Constructor_WithOnlyKey_ShouldCreateKeyWithDefaults()
    {
        // Arrange & Act
        var key = new LocalizationKey("MyKey");

        // Assert
        key.Namespace.Should().Be("");
        key.Key.Should().Be("MyKey");
        key.Category.Should().Be(LocalizationCategory.General);
        key.Variant.Should().Be(LocalizationVariant.None);
        key.DefaultValue.Should().Be(null);
    }

    [Fact]
    public void FullKey_WithoutNamespace_ShouldReturnKeyOnly()
    {
        // Arrange
        var key = new LocalizationKey("MyKey");

        // Act
        var fullKey = key.FullKey;

        // Assert
        fullKey.Should().Be("MyKey");
    }

    [Fact]
    public void FullKey_WithNamespace_ShouldReturnNamespaceAndKey()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey");

        // Act
        var fullKey = key.FullKey;

        // Assert
        fullKey.Should().Be("MyNamespace:MyKey");
    }

    [Fact]
    public void VariantKey_WithoutVariant_ShouldReturnFullKey()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey");

        // Act
        var variantKey = key.VariantKey;

        // Assert
        variantKey.Should().Be("MyNamespace:MyKey");
    }

    [Fact]
    public void VariantKey_WithSingularVariant_ShouldReturnKeyWithSuffix()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationVariant: LocalizationVariant.Singular);

        // Act
        var variantKey = key.VariantKey;

        // Assert
        variantKey.Should().Be("MyNamespace:MyKey_sg");
    }

    [Fact]
    public void VariantKey_WithPluralVariant_ShouldReturnKeyWithSuffix()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationVariant: LocalizationVariant.Plural);

        // Act
        var variantKey = key.VariantKey;

        // Assert
        variantKey.Should().Be("MyNamespace:MyKey_pl");
    }

    [Fact]
    public void VariantKey_WithNominativeVariant_ShouldReturnKeyWithSuffix()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationVariant: LocalizationVariant.Nominative);

        // Act
        var variantKey = key.VariantKey;

        // Assert
        variantKey.Should().Be("MyNamespace:MyKey_nom");
    }

    [Fact]
    public void VariantKey_WithAccusativeVariant_ShouldReturnKeyWithSuffix()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationVariant: LocalizationVariant.Accusative);

        // Act
        var variantKey = key.VariantKey;

        // Assert
        variantKey.Should().Be("MyNamespace:MyKey_acc");
    }

    [Fact]
    public void WithVariant_ShouldCreateNewKeyWithDifferentVariant()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey", LocalizationCategory.Noun);

        // Act
        var pluralKey = key.WithVariant(LocalizationVariant.Plural);

        // Assert
        pluralKey.Namespace.Should().Be("MyNamespace");
        pluralKey.Key.Should().Be("MyKey");
        pluralKey.Category.Should().Be(LocalizationCategory.Noun);
        pluralKey.Variant.Should().Be(LocalizationVariant.Plural);
        // 原始键不应改变
        key.Variant.Should().Be(LocalizationVariant.None);
    }

    [Fact]
    public void WithCategory_ShouldCreateNewKeyWithDifferentCategory()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey");

        // Act
        var uiKey = key.WithCategory(LocalizationCategory.UI);

        // Assert
        uiKey.Namespace.Should().Be("MyNamespace");
        uiKey.Key.Should().Be("MyKey");
        uiKey.Category.Should().Be(LocalizationCategory.UI);
        // 原始键不应改变
        key.Category.Should().Be(LocalizationCategory.General);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldCreateKey()
    {
        // Arrange
        string keyString = "MyKey";

        // Act
        LocalizationKey key = keyString;

        // Assert
        key.Key.Should().Be("MyKey");
        key.Namespace.Should().Be("");
    }

    [Fact]
    public void Parse_WithColon_ShouldSplitIntoNamespaceAndKey()
    {
        // Arrange & Act
        var key = LocalizationKey.Parse("MyNamespace:MyKey");

        // Assert
        key.Namespace.Should().Be("MyNamespace");
        key.Key.Should().Be("MyKey");
    }

    [Fact]
    public void Parse_WithoutColon_ShouldCreateKeyWithoutNamespace()
    {
        // Arrange & Act
        var key = LocalizationKey.Parse("MyKey");

        // Assert
        key.Namespace.Should().Be("");
        key.Key.Should().Be("MyKey");
    }

    [Fact]
    public void ToString_ShouldReturnFullKey()
    {
        // Arrange
        var key = new LocalizationKey("MyNamespace", "MyKey");

        // Act
        var result = key.ToString();

        // Assert
        result.Should().Be("MyNamespace:MyKey");
    }

    [Theory]
    [InlineData(LocalizationCategory.General, "General")]
    [InlineData(LocalizationCategory.Noun, "Noun")]
    [InlineData(LocalizationCategory.Verb, "Verb")]
    [InlineData(LocalizationCategory.Pronoun, "Pronoun")]
    [InlineData(LocalizationCategory.UI, "UI")]
    [InlineData(LocalizationCategory.Error, "Error")]
    [InlineData(LocalizationCategory.Dialog, "Dialog")]
    public void Category_ShouldStoreCorrectValue(LocalizationCategory category, string expectedName)
    {
        // Arrange
        var key = new LocalizationKey("Ns", "Key", category);

        // Act & Assert
        key.Category.Should().Be(category);
        key.Category.ToString().Should().Be(expectedName);
    }

    [Theory]
    [InlineData(LocalizationVariant.Singular, "Singular")]
    [InlineData(LocalizationVariant.Plural, "Plural")]
    [InlineData(LocalizationVariant.Nominative, "Nominative")]
    [InlineData(LocalizationVariant.Accusative, "Accusative")]
    [InlineData(LocalizationVariant.Genitive, "Genitive")]
    [InlineData(LocalizationVariant.PossessiveDeterminer, "PossessiveDeterminer")]
    public void Variant_ShouldStoreCorrectValue(LocalizationVariant variant, string expectedName)
    {
        // Arrange
        var key = new LocalizationKey("Ns", "Key", LocalizationVariant: variant);

        // Act & Assert
        key.Variant.Should().Be(variant);
        key.Variant.ToString().Should().Be(expectedName);
    }

    [Fact]
    public void VariantFlags_CanBeCombined()
    {
        // Arrange & Act
        var variant = LocalizationVariant.Singular | LocalizationVariant.Nominative;

        // Assert
        variant.HasAny(LocalizationVariant.Singular).Should().BeTrue();
        variant.HasAny(LocalizationVariant.Nominative).Should().BeTrue();
        variant.HasAny(LocalizationVariant.Plural).Should().BeFalse();
    }

    [Fact]
    public void MultipleVariants_ShouldGenerateCorrectSuffix()
    {
        // Arrange
        var variant = LocalizationVariant.Singular | LocalizationVariant.Nominative;

        // Act
        var suffix = variant.ToSuffix();

        // Assert
        suffix.Should().Be("_sg_nom");
    }
}
