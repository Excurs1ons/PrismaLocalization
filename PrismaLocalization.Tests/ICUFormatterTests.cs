using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class ICUFormatterTests
{
    private readonly ICUFormatter _formatter = new();

    [Fact]
    public void Format_WithSimplePlaceholder_ShouldReplaceCorrectly()
    {
        // Arrange
        var pattern = "Hello, {name}!";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void Format_WithMultiplePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var pattern = "{greeting}, {name}! You have {count} messages.";
        var args = new Dictionary<string, object?>
        {
            ["greeting"] = "Hello",
            ["name"] = "Alice",
            ["count"] = 5
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("Hello, Alice! You have 5 messages.");
    }

    [Fact]
    public void Format_WithPluralOne_ShouldUseOneForm()
    {
        // Arrange
        var pattern = "{count, plural, one{# cat} other{# cats}}";
        var args = new Dictionary<string, object?>
        {
            ["count"] = 1
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("1 cat");
    }

    [Fact]
    public void Format_WithPluralOther_ShouldUseOtherForm()
    {
        // Arrange
        var pattern = "{count, plural, one{# cat} other{# cats}}";
        var args = new Dictionary<string, object?>
        {
            ["count"] = 5
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("5 cats");
    }

    [Fact]
    public void Format_WithPluralZero_ShouldUseZeroForm()
    {
        // Arrange
        var pattern = "{count, plural, zero{No cats} one{# cat} other{# cats}}";
        var args = new Dictionary<string, object?>
        {
            ["count"] = 0
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("No cats");
    }

    [Fact]
    public void Format_WithPluralTwo_ShouldUseTwoForm()
    {
        // Arrange
        var pattern = "{count, plural, one{# item} two{# items} other{# items}}";
        var args = new Dictionary<string, object?>
        {
            ["count"] = 2
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("2 items");
    }

    [Fact]
    public void Format_WithSelect_ShouldSelectCorrectForm()
    {
        // Arrange
        var pattern = "{gender, select, male{He is} female{She is} other{They are}} here";
        var args = new Dictionary<string, object?>
        {
            ["gender"] = "male"
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("He is here");
    }

    [Fact]
    public void Format_WithSelectFemale_ShouldSelectFemaleForm()
    {
        // Arrange
        var pattern = "{gender, select, male{He} female{She} other{They}}";
        var args = new Dictionary<string, object?>
        {
            ["gender"] = "female"
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("She");
    }

    [Fact]
    public void Format_WithSelectOther_ShouldSelectOtherForm()
    {
        // Arrange
        var pattern = "{gender, select, male{He} female{She} other{They}}";
        var args = new Dictionary<string, object?>
        {
            ["gender"] = "other"
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("They");
    }

    [Fact]
    public void Format_WithSelectOrdinalOne_ShouldUseSt()
    {
        // Arrange
        var pattern = "You came {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";
        var args = new Dictionary<string, object?>
        {
            ["place"] = 1
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("You came 1st!");
    }

    [Fact]
    public void Format_WithSelectOrdinalTwo_ShouldUseNd()
    {
        // Arrange
        var pattern = "You came {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";
        var args = new Dictionary<string, object?>
        {
            ["place"] = 2
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("You came 2nd!");
    }

    [Fact]
    public void Format_WithSelectOrdinalFew_ShouldUseRd()
    {
        // Arrange
        var pattern = "You came {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";
        var args = new Dictionary<string, object?>
        {
            ["place"] = 3
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("You came 3rd!");
    }

    [Fact]
    public void Format_WithSelectOrdinalOther_ShouldUseTh()
    {
        // Arrange
        var pattern = "You came {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";
        var args = new Dictionary<string, object?>
        {
            ["place"] = 4
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("You came 4th!");
    }

    [Fact]
    public void Format_WithSelectOrdinalEleven_ShouldUseTh()
    {
        // Arrange
        var pattern = "You came {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";
        var args = new Dictionary<string, object?>
        {
            ["place"] = 11
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("You came 11th!");
    }

    [Fact]
    public void Format_WithComplexPlural_ShouldHandleCorrectly()
    {
        // Arrange
        var pattern = "There {count, plural, one{is # cat} other{are # cats}}.";
        var args = new Dictionary<string, object?>
        {
            ["count"] = 3
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("There are 3 cats.");
    }

    [Fact]
    public void Format_WithNestedPluralInSentence_ShouldHandleCorrectly()
    {
        // Arrange
        var pattern = "{name, select, John{He has} Mary{She has} other{They have}} {count, plural, one{# item} other{# items}}.";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["count"] = 5
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("He has 5 items.");
    }

    [Fact]
    public void Format_WithEmptyArgs_ShouldReturnOriginalPattern()
    {
        // Arrange
        var pattern = "Hello, {name}!";
        var args = new Dictionary<string, object?>();

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("Hello, {name}!");
    }

    [Fact]
    public void Format_WithMissingPlaceholder_ShouldKeepOriginal()
    {
        // Arrange
        var pattern = "Hello, {name}! Today is {day}.";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("Hello, World! Today is {day}.");
    }

    [Fact]
    public void Format_WithIndexedParams_ShouldReplaceInOrder()
    {
        // Arrange
        var pattern = "{0} + {1} = {2}";
        var args = new Dictionary<string, object?>
        {
            ["0"] = 2,
            ["1"] = 3,
            ["2"] = 5
        };

        // Act
        var result = _formatter.Format(pattern, args);

        // Assert
        result.Should().Be("2 + 3 = 5");
    }

    [Fact]
    public void FormatICU_ExtensionMethod_ShouldWork()
    {
        // Arrange
        var pattern = "Hello, {name}!";
        var args = new Dictionary<string, object?>
        {
            ["name"] = "World"
        };

        // Act
        var result = pattern.FormatICU(args);

        // Assert
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void FormatICU_WithParams_ShouldWork()
    {
        // Arrange
        var pattern = "Values: {0}, {1}, {2}";

        // Act
        var result = pattern.FormatICU(1, 2, 3);

        // Assert
        result.Should().Be("Values: 1, 2, 3");
    }
}
