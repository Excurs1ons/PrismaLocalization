using FluentAssertions;
using Xunit;

namespace PrismaLocalization.Tests;

public class LocalizationTableConverterTests
{
    private readonly LocalizationWorkflow _workflow;
    private readonly LocalizationTableConverter _converter;

    public LocalizationTableConverterTests()
    {
        _workflow = new LocalizationWorkflow
        {
            Namespace = "TestGame",
            DefaultCategory = LocalizationCategory.General
        };
        _converter = new LocalizationTableConverter(_workflow);
    }

    [Fact]
    public void ProtectPlaceholders_WithSimplePlaceholder_ShouldReplace()
    {
        // Arrange
        var text = "Hello, {name}!";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("Hello, {0}!");
        result.PlaceholderMap.Should().ContainKey("{0}");
        result.PlaceholderMap["{0}"].Should().Be("{name}");
    }

    [Fact]
    public void ProtectPlaceholders_WithMultiplePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var text = "{greeting}, {name}! You have {count} messages.";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("{0}, {1}! You have {2} messages.");
        result.PlaceholderMap.Should().HaveCount(3);
    }

    [Fact]
    public void ProtectPlaceholders_WithIndexedPlaceholder_ShouldReplace()
    {
        // Arrange
        var text = "Value: {0}";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("Value: {0}");
        result.PlaceholderMap["{0}"].Should().Be("{0}");
    }

    [Fact]
    public void ProtectPlaceholders_WithICUPlural_ShouldReplace()
    {
        // Arrange
        var text = "You have {count, plural, one{# item} other{# items}}.";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("You have {0}.");
        result.PlaceholderMap["{0}"].Should().Be("{count, plural, one{# item} other{# items}}");
    }

    [Fact]
    public void ProtectPlaceholders_WithICUSelect_ShouldReplace()
    {
        // Arrange
        var text = "{gender, select, male{He} female{She} other{They}} is here";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("{0} is here");
        result.PlaceholderMap["{0}"].Should().Be("{gender, select, male{He} female{She} other{They}}");
    }

    [Fact]
    public void ProtectPlaceholders_WithICUOrdinal_ShouldReplace()
    {
        // Arrange
        var text = "You finished {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}!";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("You finished {0}!");
    }

    [Fact]
    public void ProtectPlaceholders_WithMixedPlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var text = "{name} has {count, plural, one{# item} other{# items}}.";

        // Act
        var result = _converter.ProtectPlaceholders(text);

        // Assert
        result.Replaced.Should().Be("{0} has {1}.");
        result.PlaceholderMap.Should().HaveCount(2);
    }

    [Fact]
    public void RestorePlaceholders_ShouldRestoreOriginalPlaceholders()
    {
        // Arrange
        var original = "Hello, {name}! You have {count, plural, one{# item} other{# items}}.";
        var protectedResult = _converter.ProtectPlaceholders(original);

        // Act
        var restored = _converter.RestorePlaceholders(protectedResult.Replaced, protectedResult.PlaceholderMap);

        // Assert
        restored.Should().Be(original);
    }

    [Fact]
    public void RestorePlaceholders_WithPartialTranslation_ShouldRestoreCorrectly()
    {
        // Arrange
        var original = "Hello, {name}!";
        var protectedResult = _converter.ProtectPlaceholders(original);
        var translated = "你好，{0}！";

        // Act
        var restored = _converter.RestorePlaceholders(translated, protectedResult.PlaceholderMap);

        // Assert
        restored.Should().Be("你好，{name}！");
    }

    [Fact]
    public void FlattenToTable_ShouldCreateCorrectRows()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddSource("Goodbye");
        var cultures = new[] { "zh-CN", "en-US" };

        // Act
        var table = _converter.FlattenToTable(cultures, protectPlaceholders: false);

        // Assert
        table.Should().HaveCount(2);
        table[0].Key.Should().Be("HelloWorld");
        table[1].Key.Should().Be("Goodbye");
    }

    [Fact]
    public void FlattenToTable_WithProtectedPlaceholders_ShouldProtect()
    {
        // Arrange
        _workflow.AddSource("Hello, {name}!");
        var cultures = new[] { "zh-CN" };

        // Act
        var table = _converter.FlattenToTable(cultures, protectPlaceholders: true);

        // Assert
        table[0].Source.Should().Be("Hello, {0}!");
    }

    [Fact]
    public void ExportToCsv_ShouldCreateValidCsv()
    {
        // Arrange
        _workflow.AddSource("Hello World", "Context", "Comment");
        _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");
        var cultures = new[] { "zh-CN", "en-US" };

        // Act
        var csv = _converter.ExportToCsv(cultures, "\t");

        // Assert
        var lines = csv.Split('\n');
        lines.Should().HaveCountGreaterThan(1);
        lines[0].Should().Contain("Key");
        lines[0].Should().Contain("Source");
        lines[0].Should().Contain("zh-CN");
        lines[0].Should().Contain("en-US");
    }

    [Fact]
    public void ExportToExcelCsv_ShouldUseCommaDelimiter()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var cultures = new[] { "zh-CN" };

        // Act
        var csv = _converter.ExportToExcelCsv(cultures);

        // Assert
        var lines = csv.Split('\n');
        lines[0].Should().Contain(",");
    }

    [Fact]
    public void ExportToCsv_WithSpecialCharacters_ShouldEscape()
    {
        // Arrange
        _workflow.AddSource("Text with \"quotes\" and, commas");
        var cultures = new[] { "zh-CN" };

        // Act
        var csv = _converter.ExportToCsv(cultures, ",");

        // Assert
        csv.Should().Contain("\"");
    }

    [Fact]
    public void ImportFromCsv_ShouldImportTranslations()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var csv = "Key\tNamespace\tCategory\tSource\tContext\tComment\tzh-CN\n" +
                   "HelloWorld\tTestGame\tGeneral\tHello World\t\t\t你好世界";
        var cultures = new[] { "zh-CN" };

        // First export to get the maps
        _converter.FlattenToTable(cultures, protectPlaceholders: true);

        // Act
        var imported = _converter.ImportFromCsv(csv, "\t", targetCultures: cultures);

        // Assert
        imported.Should().BeGreaterThan(0);
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Key == "HelloWorld");
        entry.Should().NotBeNull();
    }

    [Fact]
    public void ImportFromCsv_WithProtectedPlaceholders_ShouldRestore()
    {
        // Arrange
        _workflow.AddSource("Hello, {name}!");
        var cultures = new[] { "zh-CN" };
        _converter.FlattenToTable(cultures, protectPlaceholders: true);

        // CSV with protected placeholders
        var csv = "Key\tNamespace\tCategory\tSource\tContext\tComment\tzh-CN\n" +
                   "Hello\tTestGame\tGeneral\tHello, {0}!\t\t\t你好，{name}！";

        // Act
        _converter.ImportFromCsv(csv, "\t", targetCultures: cultures);
        var entry = _workflow.GetPendingEntries().FirstOrDefault(e => e.Key == "Hello");

        // Assert
        entry.Should().NotBeNull();
        entry!.Translations["zh-CN"].Should().Be("你好，{name}！");
    }

    [Fact]
    public void ExportToExcelHtml_ShouldCreateValidHtml()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var cultures = new[] { "zh-CN", "en-US" };

        // Act
        var html = _converter.ExportToExcelHtml(cultures);

        // Assert
        html.Should().Contain("<html>");
        html.Should().Contain("<table>");
        html.Should().Contain("<th>Key</th>");
        html.Should().Contain("<th>Source</th>");
        html.Should().Contain("<th>zh-CN</th>");
        html.Should().Contain("Hello World");
    }

    [Fact]
    public void SaveAndLoadPlaceholderMaps_ShouldPreserveMaps()
    {
        // Arrange
        var text = "Hello, {name}! You have {count, plural, one{# item} other{# items}}.";
        var result = _converter.ProtectPlaceholders(text);

        // Act - Save
        _converter.SavePlaceholderMaps("/tmp/test_maps.json");

        // Create new converter and load
        var newConverter = new LocalizationTableConverter();
        newConverter.LoadPlaceholderMaps("/tmp/test_maps.json");

        // Assert
        var restored = newConverter.RestorePlaceholders(result.Replaced, result.PlaceholderMap);
        restored.Should().Be(text);
    }

    [Fact]
    public void ParseCsvLine_ShouldHandleSimpleValues()
    {
        // Arrange
        var line = "Key1\tValue1\tValue2";

        // Act
        var result = _converter.ExportToCsv(new[] { "zh-CN" }, "\t");
        var lines = result.Split('\n');
        var values = lines[0].Split('\t');

        // Assert
        values.Should().Contain("Key");
        values.Should().Contain("Source");
    }

    [Fact]
    public void ParseCsvLine_WithQuotes_ShouldHandleCorrectly()
    {
        // Arrange
        var line = "\"Key,With,Commas\"\t\"Value \"\"With\"\" Quotes\"";

        // Act
        var result = _converter.ExportToCsv(new[] { "zh-CN" }, ",");

        // Assert
        result.Should().Contain("\"");
    }

    [Fact]
    public void Workflow_ExportAndSave_ShouldCreateFile()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        _workflow.AddTranslationByKey("HelloWorld", "zh-CN", "你好世界");
        var cultures = new[] { "zh-CN" };

        // Act
        _workflow.ExportAndSave("/tmp/test_localization.csv", cultures, TableFormat.Csv);

        // Assert
        File.Exists("/tmp/test_localization.csv").Should().BeTrue();
        File.Exists("/tmp/test_localization.map.json").Should().BeTrue();

        var content = File.ReadAllText("/tmp/test_localization.csv");
        content.Should().Contain("Hello World");

        // Cleanup
        File.Delete("/tmp/test_localization.csv");
        File.Delete("/tmp/test_localization.map.json");
    }

    [Fact]
    public void Workflow_ImportFromFile_ShouldImportTranslations()
    {
        // Arrange
        _workflow.AddSource("Hello World");
        var cultures = new[] { "zh-CN" };
        _workflow.ExportAndSave("/tmp/test_import.csv", cultures, TableFormat.Csv);

        // Modify the CSV to add translation
        var csv = File.ReadAllText("/tmp/test_import.csv");
        csv = csv.Replace("\t\n", "\t你好世界\n");
        File.WriteAllText("/tmp/test_import.csv", csv);

        // Clear workflow and re-add only source
        _workflow.Clear();
        _workflow.AddSource("Hello World");
        _workflow.Namespace = "TestGame";

        // Act
        var imported = _workflow.ImportFromFile("/tmp/test_import.csv", cultures);

        // Assert
        imported.Should().BeGreaterThan(0);

        // Cleanup
        File.Delete("/tmp/test_import.csv");
        File.Delete("/tmp/test_import.map.json");
    }

    [Theory]
    [InlineData(TableFormat.Tsv, "\t")]
    [InlineData(TableFormat.Csv, ",")]
    [InlineData(TableFormat.ExcelHtml, null)]
    public void ExportAndSave_WithDifferentFormats_ShouldCreateCorrectFiles(TableFormat format, string? expectedDelimiter)
    {
        // Arrange
        _workflow.AddSource("Test");
        var cultures = new[] { "zh-CN" };

        // Act
        _workflow.ExportAndSave("/tmp/test_format", cultures, format);

        // Assert
        var exists = format switch
        {
            TableFormat.Tsv => File.Exists("/tmp/test_format.tsv"),
            TableFormat.Csv => File.Exists("/tmp/test_format.csv"),
            TableFormat.ExcelHtml => File.Exists("/tmp/test_format.html"),
            _ => false
        };
        exists.Should().BeTrue();

        // Cleanup
        var ext = format switch
        {
            TableFormat.Tsv => ".tsv",
            TableFormat.Csv => ".csv",
            TableFormat.ExcelHtml => ".html",
            _ => ""
        };
        if (File.Exists("/tmp/test_format" + ext))
            File.Delete("/tmp/test_format" + ext);
        if (File.Exists("/tmp/test_format.map.json"))
            File.Delete("/tmp/test_format.map.json");
    }
}
