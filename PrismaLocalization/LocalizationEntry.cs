using Newtonsoft.Json;

namespace PrismaLocalization;

/// <summary>
/// Represents a single localization entry for JSON serialization.
/// </summary>
public class LocalizationEntry
{
    /// <summary>
    /// The localization key.
    /// </summary>
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The namespace for the key (optional).
    /// </summary>
    [JsonProperty("namespace")]
    public string? Namespace { get; set; }

    /// <summary>
    /// The category of this entry.
    /// </summary>
    [JsonProperty("category")]
    public string Category { get; set; } = "General";

    /// <summary>
    /// The translated text template.
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional context information for translators.
    /// </summary>
    [JsonProperty("context")]
    public string? Context { get; set; }

    /// <summary>
    /// Optional developer comments.
    /// </summary>
    [JsonProperty("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Converts this entry to a LocalizationKey.
    /// </summary>
    public LocalizationKey ToLocalizationKey()
    {
        var category = Enum.TryParse<LocalizationCategory>(Category, out var cat)
            ? cat
            : LocalizationCategory.General;

        return new LocalizationKey(
            Namespace ?? string.Empty,
            Key,
            category,
            Text // Use text as default value
        );
    }

    /// <summary>
    /// Creates a LocalizationEntry from a LocalizationKey.
    /// </summary>
    public static LocalizationEntry FromKey(LocalizationKey key, string text, string? context = null, string? comment = null)
    {
        return new LocalizationEntry
        {
            Key = key.Key,
            Namespace = string.IsNullOrEmpty(key.Namespace) ? null : key.Namespace,
            Category = key.Category.ToString(),
            Text = text,
            Context = context,
            Comment = comment
        };
    }
}

/// <summary>
/// Container for all localization entries for a specific culture.
/// </summary>
public class LocalizationData
{
    /// <summary>
    /// The culture code (e.g., "en-US", "zh-CN").
    /// </summary>
    [JsonProperty("culture")]
    public string Culture { get; set; } = string.Empty;

    /// <summary>
    /// The localization entries, optionally grouped by category.
    /// </summary>
    [JsonProperty("entries")]
    public List<LocalizationEntry> Entries { get; set; } = new();

    /// <summary>
    /// Optional entries grouped by category for easier editing.
    /// </summary>
    [JsonProperty("byCategory")]
    public Dictionary<string, List<LocalizationEntry>>? ByCategory { get; set; }

    /// <summary>
    /// Gets entries for a specific category.
    /// </summary>
    public List<LocalizationEntry> GetEntriesByCategory(LocalizationCategory category)
    {
        return Entries.Where(e => e.Category.Equals(category.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
