using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class TextFormatTests
{
    [Fact]
    public void Format_WithNamedArguments_ShouldReplace()
    {
        // Arrange
        var format = "Hello, {name}!";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void Format_WithMultipleNamedArguments_ShouldReplaceAll()
    {
        // Arrange
        var format = "{greeting}, {name}! Today is {day}.";
        var args = new Dictionary<string, object?>
        {
            ["greeting"] = "Hello",
            ["name"] = "Alice",
            ["day"] = "Monday"
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Be("Hello, Alice! Today is Monday.");
    }

    [Fact]
    public void Format_WithIndexedArguments_ShouldReplace()
    {
        // Arrange & Act
        var result = TextFormat.Format("Value: {0}, Value2: {1}", 123, "test");

        // Assert
        result.Should().Be("Value: 123, Value2: test");
    }

    [Fact]
    public void Format_WithEmptyArgs_ShouldReturnOriginal()
    {
        // Arrange & Act
        var result = TextFormat.Format("Hello, {name}!");

        // Assert
        result.Should().Be("Hello, {name}!");
    }

    [Fact]
    public void Format_WithNullArgs_ShouldHandleCorrectly()
    {
        // Arrange
        var args = new Dictionary<string, object?>
        {
            ["name"] = null
        };

        // Act
        var result = TextFormat.Format("Hello, {name}!", args);

        // Assert
        result.Should().Be("Hello, !");
    }

    [Fact]
    public void Plural_WithOne_ShouldReturnOneForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["one"] = "# cat",
            ["other"] = "# cats"
        };

        // Act
        var result = TextFormat.Plural(1, forms);

        // Assert
        result.Should().Be("1 cat");
    }

    [Fact]
    public void Plural_WithZero_ShouldReturnZeroForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["zero"] = "No cats",
            ["one"] = "# cat",
            ["other"] = "# cats"
        };

        // Act
        var result = TextFormat.Plural(0, forms);

        // Assert
        result.Should().Be("No cats");
    }

    [Fact]
    public void Plural_WithMultiple_ShouldReturnOtherForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["one"] = "# cat",
            ["other"] = "# cats"
        };

        // Act
        var result = TextFormat.Plural(5, forms);

        // Assert
        result.Should().Be("5 cats");
    }

    [Fact]
    public void Plural_WithTwo_ShouldReturnTwoForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["one"] = "# item",
            ["two"] = "# items",
            ["other"] = "# items"
        };

        // Act
        var result = TextFormat.Plural(2, forms);

        // Assert
        result.Should().Be("2 items");
    }

    [Fact]
    public void Plural_WithoutOneForm_ShouldFallbackToOther()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["other"] = "# items"
        };

        // Act
        var result = TextFormat.Plural(1, forms);

        // Assert
        result.Should().Be("1 items");
    }

    [Theory]
    [InlineData(1, "st")]
    [InlineData(2, "nd")]
    [InlineData(3, "rd")]
    [InlineData(4, "th")]
    [InlineData(11, "th")]
    [InlineData(12, "th")]
    [InlineData(13, "th")]
    [InlineData(21, "st")]
    [InlineData(22, "nd")]
    [InlineData(23, "rd")]
    [InlineData(101, "st")]
    [InlineData(111, "th")]
    public void Ordinal_ShouldReturnCorrectSuffix(int number, string expectedSuffix)
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["one"] = "#st",
            ["two"] = "#nd",
            ["few"] = "#rd",
            ["other"] = "#th"
        };

        // Act
        var result = TextFormat.Ordinal(number, forms);

        // Assert
        result.Should().Be(expectedSuffix);
    }

    [Fact]
    public void Gender_WithMasculine_ShouldReturnMasculineForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["masculine"] = "Le",
            ["feminine"] = "La"
        };

        // Act
        var result = TextFormat.Gender(TextGender.Masculine, forms);

        // Assert
        result.Should().Be("Le");
    }

    [Fact]
    public void Gender_WithFeminine_ShouldReturnFeminineForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["masculine"] = "Le",
            ["feminine"] = "La"
        };

        // Act
        var result = TextFormat.Gender(TextGender.Feminine, forms);

        // Assert
        result.Should().Be("La");
    }

    [Fact]
    public void Gender_WithNeuter_ShouldReturnNeuterForm()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["masculine"] = "He",
            ["feminine"] = "She",
            ["neuter"] = "It"
        };

        // Act
        var result = TextFormat.Gender(TextGender.Neuter, forms);

        // Assert
        result.Should().Be("It");
    }

    [Fact]
    public void Gender_WithMissingForm_ShouldReturnEmpty()
    {
        // Arrange
        var forms = new Dictionary<string, string>
        {
            ["masculine"] = "Le"
        };

        // Act
        var result = TextFormat.Gender(TextGender.Feminine, forms);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void HangulPostposition_WithConsonant_ShouldReturnConsonantForm()
    {
        // Arrange
        var text = "사람"; // ends with consonant

        // Act
        var result = TextFormat.HangulPostposition(text, "은", "는");

        // Assert
        result.Should().Be("은");
    }

    [Fact]
    public void HangulPostposition_WithVowel_ShouldReturnVowelForm()
    {
        // Arrange
        var text = "사자"; // ends with vowel in Korean context

        // Act
        var result = TextFormat.HangulPostposition(text, "은", "는");

        // Assert
        result.Should().Be("는");
    }

    [Fact]
    public void FormatNamed_WithMissingKey_ShouldKeepPlaceholder()
    {
        // Arrange
        var format = "Hello, {name}! Today is {day}.";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Be("Hello, World! Today is {day}.");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("No placeholders", "No placeholders")]
    [InlineData("Braces {{escaped}}", "Braces {{escaped}}")]
    public void Format_EdgeCases_ShouldHandleCorrectly(string input, string expected)
    {
        // Arrange
        var args = new Dictionary<string, object?>();

        // Act
        var result = TextFormat.Format(input, args);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Format_WithDuplicatePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var format = "{name} said {name} is here";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "John"
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Be("John said John is here");
    }

    [Fact]
    public void Format_WithNumericStringKey_ShouldReplace()
    {
        // Arrange
        var format = "Value: {0}";
        var args = new Dictionary<string, object?>
        {
            ["0"] = 42
        };

        // Act
        var result = TextFormat.FormatNamed(format, args);

        // Assert
        result.Should().Be("Value: 42");
    }

    [Fact]
    public void Format_WithObjectArg_ShouldConvertToString()
    {
        // Arrange
        var format = "Number: {num}";
        var args = new Dictionary<string, object?>
        {
            ["num"] = 123
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Be("Number: 123");
    }

    [Fact]
    public void Format_WithDateTimeArg_ShouldConvertToString()
    {
        // Arrange
        var format = "Date: {date}";
        var date = new DateTime(2024, 5, 15);
        var args = new Dictionary<string, object?>
        {
            ["date"] = date
        };

        // Act
        var result = TextFormat.Format(format, args);

        // Assert
        result.Should().Contain("2024");
    }
}
