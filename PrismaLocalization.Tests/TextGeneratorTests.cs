using FluentAssertions;
using Xunit;
using System.Globalization;

namespace PrismaLocalization.Tests;

public class TextGeneratorTests
{
    [Fact]
    public void AsNumber_WithDouble_ShouldFormatCorrectly()
    {
        // Act
        var result = TextGenerator.AsNumber(1234.56);

        // Assert
        result.Should().Be("1,234.56");
    }

    [Fact]
    public void AsNumber_WithInteger_ShouldFormatCorrectly()
    {
        // Act
        var result = TextGenerator.AsNumber(12345);

        // Assert
        result.Should().Be("12,345");
    }

    [Fact]
    public void AsNumber_WithChineseCulture_ShouldUseGrouping()
    {
        // Act
        var result = TextGenerator.AsNumber(12345.67, "zh-CN");

        // Assert
        result.Should().Be("12,345.67");
    }

    [Fact]
    public void AsNumber_WithoutGrouping_ShouldNotUseSeparator()
    {
        // Act
        var result = TextGenerator.AsNumber(12345, useGrouping: false);

        // Assert
        result.Should().Be("12345");
    }

    [Fact]
    public void AsPercent_WithValue_ShouldConvertToPercentage()
    {
        // Act
        var result = TextGenerator.AsPercent(0.85);

        // Assert
        result.Should().Be("85%");
    }

    [Fact]
    public void AsPercent_WithFraction_ShouldRoundCorrectly()
    {
        // Act
        var result = TextGenerator.AsPercent(0.856);

        // Assert
        result.Should().Be("86%");
    }

    [Fact]
    public void AsMemory_WithBytes_ShouldReturnBytes()
    {
        // Act
        var result = TextGenerator.AsMemory(512);

        // Assert
        result.Should().Be("512 B");
    }

    [Fact]
    public void AsMemory_WithKilobytes_ShouldReturnKiB()
    {
        // Act
        var result = TextGenerator.AsMemory(2048);

        // Assert
        result.Should().Be("2.00 KiB");
    }

    [Fact]
    public void AsMemory_WithMegabytes_ShouldReturnMiB()
    {
        // Act
        var result = TextGenerator.AsMemory(3 * 1024 * 1024);

        // Assert
        result.Should().Be("3.00 MiB");
    }

    [Fact]
    public void AsMemory_WithGigabytes_ShouldReturnGiB()
    {
        // Act
        var result = TextGenerator.AsMemory(2L * 1024 * 1024 * 1024);

        // Assert
        result.Should().Be("2.00 GiB");
    }

    [Fact]
    public void AsMemory_WithTerabytes_ShouldReturnTiB()
    {
        // Act
        var result = TextGenerator.AsMemory(1500L * 1024 * 1024 * 1024);

        // Assert
        result.Should().Be("1.46 TiB");
    }

    [Fact]
    public void AsCurrency_WithUSD_ShouldFormatCorrectly()
    {
        // Act
        var result = TextGenerator.AsCurrency(1234.50, "USD");

        // Assert
        result.Should().Contain("$");
        result.Should().Contain("1,234.50");
    }

    [Fact]
    public void AsCurrency_WithCNY_ShouldFormatCorrectly()
    {
        // Act
        var result = TextGenerator.AsCurrency(1234.50, "CNY", "zh-CN");

        // Assert
        result.Should().Contain("¥");
        result.Should().Contain("1,234.50");
    }

    [Fact]
    public void AsDate_WithDefaultFormat_ShouldReturnShortDate()
    {
        // Arrange
        var date = new DateTime(2024, 5, 15);

        // Act
        var result = TextGenerator.AsDate(date);

        // Assert
        result.Should().Contain("2024");
        result.Should().Contain("5");
        result.Should().Contain("15");
    }

    [Fact]
    public void AsDate_WithLongFormat_ShouldReturnLongDate()
    {
        // Arrange
        var date = new DateTime(2024, 5, 15);

        // Act
        var result = TextGenerator.AsDate(date, format: "long");

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void AsTime_WithDateTime_ShouldReturnTime()
    {
        // Arrange
        var time = new DateTime(2024, 5, 15, 14, 30, 45);

        // Act
        var result = TextGenerator.AsTime(time);

        // Assert
        result.Should().Contain("14");
    }

    [Fact]
    public void AsDateTime_WithDateTime_ShouldReturnDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2024, 5, 15, 14, 30, 0);

        // Act
        var result = TextGenerator.AsDateTime(dateTime);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void AsTimeSpan_WithSeconds_ShouldReturnSeconds()
    {
        // Arrange
        var span = TimeSpan.FromSeconds(45);

        // Act
        var result = TextGenerator.AsTimeSpan(span);

        // Assert
        result.Should().Contain("45");
    }

    [Fact]
    public void AsTimeSpan_WithMinutes_ShouldReturnMinutes()
    {
        // Arrange
        var span = TimeSpan.FromMinutes(5);

        // Act
        var result = TextGenerator.AsTimeSpan(span);

        // Assert
        result.Should().Contain("5");
    }

    [Fact]
    public void AsTimeSpan_WithHours_ShouldReturnHours()
    {
        // Arrange
        var span = TimeSpan.FromHours(2);

        // Act
        var result = TextGenerator.AsTimeSpan(span);

        // Assert
        result.Should().Contain("2");
    }

    [Fact]
    public void AsTimeSpan_WithDays_ShouldReturnDays()
    {
        // Arrange
        var span = TimeSpan.FromDays(3);

        // Act
        var result = TextGenerator.AsTimeSpan(span);

        // Assert
        result.Should().Contain("3");
    }

    [Fact]
    public void ToLower_ShouldConvertToLowercase()
    {
        // Act
        var result = TextGenerator.ToLower("HELLO WORLD");

        // Assert
        result.Should().Be("hello world");
    }

    [Fact]
    public void ToUpper_ShouldConvertToUppercase()
    {
        // Act
        var result = TextGenerator.ToUpper("hello world");

        // Assert
        result.Should().Be("HELLO WORLD");
    }

    [Fact]
    public void ToTitleCase_ShouldCapitalizeFirstLetter()
    {
        // Act
        var result = TextGenerator.ToTitleCase("hello world");

        // Assert
        result.Should().Be("Hello World");
    }

    [Fact]
    public void ToLower_WithCulture_ShouldRespectCulture()
    {
        // Act
        var result = TextGenerator.ToLower("HELLO", "tr-TR"); // Turkish has special lowercase rules

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ToNumber_WithNegative_ShouldHandleCorrectly()
    {
        // Act
        var result = TextGenerator.AsNumber(-1234.56);

        // Assert
        result.Should().StartWith("-");
        result.Should().Contain("1,234.56");
    }

    [Fact]
    public void AsNumber_WithZero_ShouldReturnZero()
    {
        // Act
        var result = TextGenerator.AsNumber(0);

        // Assert
        result.Should().Be("0");
    }

    [Fact]
    public void AsPercent_WithZero_ShouldReturnZeroPercent()
    {
        // Act
        var result = TextGenerator.AsPercent(0);

        // Assert
        result.Should().Be("0%");
    }

    [Fact]
    public void AsPercent_WithOne_ShouldReturn100Percent()
    {
        // Act
        var result = TextGenerator.AsPercent(1.0);

        // Assert
        result.Should().Be("100%");
    }

    [Theory]
    [InlineData(0.5, "50%")]
    [InlineData(0.25, "25%")]
    [InlineData(0.75, "75%")]
    [InlineData(1.5, "150%")]
    [InlineData(2.0, "200%")]
    public void AsPercent_WithVariousValues_ShouldConvertCorrectly(double input, string expected)
    {
        // Act
        var result = TextGenerator.AsPercent(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100, "100 B")]
    [InlineData(1024, "1.00 KiB")]
    [InlineData(1024 * 1024, "1.00 MiB")]
    [InlineData(1024 * 1024 * 1024, "1.00 GiB")]
    public void AsMemory_WithPowersOf1024_ShouldFormatCorrectly(long bytes, string expected)
    {
        // Act
        var result = TextGenerator.AsMemory(bytes);

        // Assert
        result.Should().Be(expected);
    }
}
