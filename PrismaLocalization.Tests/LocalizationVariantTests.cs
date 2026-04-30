using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class LocalizationVariantTests
{
    [Fact]
    public void ToSuffix_Singular_ShouldReturnSg()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Singular.ToSuffix();

        // Assert
        suffix.Should().Be("_sg");
    }

    [Fact]
    public void ToSuffix_Plural_ShouldReturnPl()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Plural.ToSuffix();

        // Assert
        suffix.Should().Be("_pl");
    }

    [Fact]
    public void ToSuffix_Nominative_ShouldReturnNom()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Nominative.ToSuffix();

        // Assert
        suffix.Should().Be("_nom");
    }

    [Fact]
    public void ToSuffix_Accusative_ShouldReturnAcc()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Accusative.ToSuffix();

        // Assert
        suffix.Should().Be("_acc");
    }

    [Fact]
    public void ToSuffix_Genitive_ShouldReturnGen()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Genitive.ToSuffix();

        // Assert
        suffix.Should().Be("_gen");
    }

    [Fact]
    public void ToSuffix_Dative_ShouldReturnDat()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Dative.ToSuffix();

        // Assert
        suffix.Should().Be("_dat");
    }

    [Fact]
    public void ToSuffix_PossessiveDeterminer_ShouldReturnPosdet()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.PossessiveDeterminer.ToSuffix();

        // Assert
        suffix.Should().Be("_posdet");
    }

    [Fact]
    public void ToSuffix_PossessivePronoun_ShouldReturnPospro()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.PossessivePronoun.ToSuffix();

        // Assert
        suffix.Should().Be("_pospro");
    }

    [Fact]
    public void ToSuffix_Masculine_ShouldReturnM()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Masculine.ToSuffix();

        // Assert
        suffix.Should().Be("_m");
    }

    [Fact]
    public void ToSuffix_Feminine_ShouldReturnF()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Feminine.ToSuffix();

        // Assert
        suffix.Should().Be("_f");
    }

    [Fact]
    public void ToSuffix_Neuter_ShouldReturnN()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Neuter.ToSuffix();

        // Assert
        suffix.Should().Be("_n");
    }

    [Fact]
    public void ToSuffix_FirstPerson_ShouldReturnP1()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.FirstPerson.ToSuffix();

        // Assert
        suffix.Should().Be("_p1");
    }

    [Fact]
    public void ToSuffix_SecondPerson_ShouldReturnP2()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.SecondPerson.ToSuffix();

        // Assert
        suffix.Should().Be("_p2");
    }

    [Fact]
    public void ToSuffix_ThirdPerson_ShouldReturnP3()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.ThirdPerson.ToSuffix();

        // Assert
        suffix.Should().Be("_p3");
    }

    [Fact]
    public void ToSuffix_Present_ShouldReturnPres()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Present.ToSuffix();

        // Assert
        suffix.Should().Be("_pres");
    }

    [Fact]
    public void ToSuffix_Past_ShouldReturnPast()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Past.ToSuffix();

        // Assert
        suffix.Should().Be("_past");
    }

    [Fact]
    public void ToSuffix_Future_ShouldReturnFut()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Future.ToSuffix();

        // Assert
        suffix.Should().Be("_fut");
    }

    [Fact]
    public void ToSuffix_Conditional_ShouldReturnCond()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.Conditional.ToSuffix();

        // Assert
        suffix.Should().Be("_cond");
    }

    [Fact]
    public void ToSuffix_None_ShouldReturnEmpty()
    {
        // Arrange & Act
        var suffix = LocalizationVariant.None.ToSuffix();

        // Assert
        suffix.Should().Be("");
    }

    [Fact]
    public void ToSuffix_CombinedVariants_ShouldReturnCombinedSuffix()
    {
        // Arrange
        var variant = LocalizationVariant.Singular | LocalizationVariant.Nominative | LocalizationVariant.FirstPerson;

        // Act
        var suffix = variant.ToSuffix();

        // Assert
        suffix.Should().Be("_sg_nom_p1");
    }

    [Fact]
    public void HasAny_ShouldReturnTrueWhenFlagIsSet()
    {
        // Arrange
        var variant = LocalizationVariant.Singular | LocalizationVariant.Plural;

        // Act & Assert
        variant.HasAny(LocalizationVariant.Singular).Should().BeTrue();
        variant.HasAny(LocalizationVariant.Plural).Should().BeTrue();
    }

    [Fact]
    public void HasAny_ShouldReturnFalseWhenFlagIsNotSet()
    {
        // Arrange
        var variant = LocalizationVariant.Singular | LocalizationVariant.Nominative;

        // Act & Assert
        variant.HasAny(LocalizationVariant.Plural).Should().BeFalse();
        variant.HasAny(LocalizationVariant.Accusative).Should().BeFalse();
    }

    [Fact]
    public void Describe_ShouldReturnHumanReadableDescription()
    {
        // Arrange
        var variant = LocalizationVariant.Singular | LocalizationVariant.Nominative | LocalizationVariant.Masculine;

        // Act
        var description = variant.Describe();

        // Assert
        description.Should().Contain("Singular");
        description.Should().Contain("Nominative");
        description.Should().Contain("Masculine");
    }

    [Theory]
    [InlineData(LocalizationVariant.DefaultNoun, "Singular", "Nominative")]
    [InlineData(LocalizationVariant.DefaultPronoun, "1stPerson", "Singular", "Nominative")]
    [InlineData(LocalizationVariant.Subject, "Nominative")]
    [InlineData(LocalizationVariant.Object, "Accusative")]
    public void PredefinedVariants_ShouldHaveExpectedComponents(LocalizationVariant variant, params string[] expectedComponents)
    {
        // Act
        var description = variant.Describe();

        // Assert
        foreach (var component in expectedComponents)
        {
            description.Should().Contain(component);
        }
    }
}
