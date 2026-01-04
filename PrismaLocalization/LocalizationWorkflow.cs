using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrismaLocalization;

/// <summary>
/// 本地化工作流程管理器。
/// 处理从原文模板到 key，再到翻译结果的完整流程。
/// </summary>
public class LocalizationWorkflow
{
    private readonly ConcurrentDictionary<string, WorkflowEntry> _pendingEntries = new();
    private readonly ConcurrentDictionary<string, WorkflowEntry> _translatedEntries = new();
    private string _namespace = "Default";
    private LocalizationCategory _defaultCategory = LocalizationCategory.General;

    /// <summary>
    /// 获取或设置默认命名空间。
    /// </summary>
    public string Namespace
    {
        get => _namespace;
        set => _namespace = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// 获取或设置默认分类。
    /// </summary>
    public LocalizationCategory DefaultCategory
    {
        get => _defaultCategory;
        set => _defaultCategory = value;
    }

    /// <summary>
    /// 工作流程条目。
    /// </summary>
    public class WorkflowEntry
    {
        /// <summary>
        /// 唯一标识符。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 命名空间。
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// 键名。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 原文（源文本）。
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 分类。
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// 上下文信息（用于译者理解）。
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// 开发者注释。
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 各文化的翻译。
        /// </summary>
        public Dictionary<string, string> Translations { get; set; } = new();

        /// <summary>
        /// 变体翻译（支持复数、人称变格等）。
        /// 键格式为 "{Culture}:{VariantSuffix}"，例如 "zh-CN:pl" 表示中文复数形式。
        /// </summary>
        public Dictionary<string, string> VariantTranslations { get; set; } = new();

        /// <summary>
        /// 支持的变体类型。
        /// </summary>
        public HashSet<string> SupportedVariants { get; set; } = new();

        /// <summary>
        /// 是否已翻译。
        /// </summary>
        public bool IsFullyTranslated =>
            Translations.Count > 0 && Translations.All(t => !string.IsNullOrEmpty(t.Value));

        /// <summary>
        /// 是否已完成变体翻译。
        /// </summary>
        public bool IsVariantTranslationComplete =>
            SupportedVariants.Count == 0 ||
            VariantTranslations.Count >= SupportedVariants.Count * Translations.Count;

        /// <summary>
        /// 转换为 LocalizationKey。
        /// </summary>
        /// <param name="variant">可选的变体。</param>
        public LocalizationKey ToLocalizationKey(LocalizationVariant variant = LocalizationVariant.None)
        {
            Enum.TryParse<LocalizationCategory>(Category, out var category);
            return new LocalizationKey(Namespace, Key, category, variant, Source);
        }

        /// <summary>
        /// 转换为 LocalizationEntry。
        /// </summary>
        public LocalizationEntry ToLocalizationEntry()
        {
            return new LocalizationEntry
            {
                Key = Key,
                Namespace = Namespace,
                Category = Category,
                Text = Source,
                Context = Context,
                Comment = Comment
            };
        }

        /// <summary>
        /// 添加变体支持。
        /// </summary>
        public void AddVariantSupport(LocalizationVariant variant)
        {
            SupportedVariants.Add(variant.ToString());
        }

        /// <summary>
        /// 获取指定文化和变体的翻译。
        /// </summary>
        public string? GetVariantTranslation(string culture, LocalizationVariant variant)
        {
            if (variant == LocalizationVariant.None)
                return Translations.TryGetValue(culture, out var t) ? t : null;

            var key = $"{culture}:{variant.ToSuffix()}";
            return VariantTranslations.TryGetValue(key, out var vt) ? vt : null;
        }

        /// <summary>
        /// 设置指定文化和变体的翻译。
        /// </summary>
        public void SetVariantTranslation(string culture, LocalizationVariant variant, string translation)
        {
            if (variant == LocalizationVariant.None)
            {
                Translations[culture] = translation;
            }
            else
            {
                SupportedVariants.Add(variant.ToString());
                VariantTranslations[$"{culture}:{variant.ToSuffix()}"] = translation;
            }
        }
    }

    /// <summary>
    /// 添加源文本到工作流程。
    /// </summary>
    /// <param name="source">源文本。</param>
    /// <param name="context">上下文信息。</param>
    /// <param name="comment">开发者注释。</param>
    /// <returns>创建的工作流程条目。</returns>
    public WorkflowEntry AddSource(string source, string? context = null, string? comment = null)
    {
        return AddSource(source, _namespace, _defaultCategory, context, comment);
    }

    /// <summary>
    /// 添加源文本到工作流程（指定命名空间和分类）。
    /// </summary>
    /// <param name="source">源文本。</param>
    /// <param name="ns">命名空间。</param>
    /// <param name="category">分类。</param>
    /// <param name="context">上下文信息。</param>
    /// <param name="comment">开发者注释。</param>
    /// <returns>创建的工作流程条目。</returns>
    public WorkflowEntry AddSource(
        string source,
        string ns,
        LocalizationCategory category,
        string? context = null,
        string? comment = null)
    {
        var key = GenerateKeyFromSource(source);
        var id = $"{ns}:{key}";

        var entry = new WorkflowEntry
        {
            Id = id,
            Namespace = ns,
            Key = key,
            Source = source,
            Category = category.ToString(),
            Context = context,
            Comment = comment,
            Translations = new Dictionary<string, string>()
        };

        _pendingEntries.TryAdd(id, entry);
        return entry;
    }

    /// <summary>
    /// 从源文本生成键名。
    /// </summary>
    private string GenerateKeyFromSource(string source)
    {
        // 移除特殊字符，保留字母、数字和下划线
        var cleaned = System.Text.RegularExpressions.Regex.Replace(source, @"[^\w\s]", "");
        // 转换为 PascalCase
        var words = cleaned.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return "EmptyKey";

        var key = string.Concat(words.Select(w =>
            char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));

        return key;
    }

    /// <summary>
    /// 为源文本指定自定义键名。
    /// </summary>
    /// <param name="source">源文本。</param>
    /// <param name="customKey">自定义键名。</param>
    /// <returns>创建的工作流程条目。</returns>
    public WorkflowEntry AddSourceWithKey(string source, string customKey)
    {
        var id = $"{_namespace}:{customKey}";

        var entry = new WorkflowEntry
        {
            Id = id,
            Namespace = _namespace,
            Key = customKey,
            Source = source,
            Category = _defaultCategory.ToString(),
            Translations = new Dictionary<string, string>()
        };

        _pendingEntries.TryAdd(id, entry);
        return entry;
    }

    /// <summary>
    /// 添加翻译。
    /// </summary>
    /// <param name="entryId">条目 ID。</param>
    /// <param name="culture">文化代码。</param>
    /// <param name="translation">翻译文本。</param>
    public bool AddTranslation(string entryId, string culture, string translation)
    {
        if (_pendingEntries.TryGetValue(entryId, out var entry))
        {
            entry.Translations[culture] = translation;

            // 检查是否已完成所有翻译
            if (entry.IsFullyTranslated)
            {
                _pendingEntries.TryRemove(entryId, out _);
                _translatedEntries.TryAdd(entryId, entry);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 通过键名添加翻译。
    /// </summary>
    public bool AddTranslationByKey(string key, string culture, string translation)
    {
        var entryId = $"{_namespace}:{key}";
        return AddTranslation(entryId, culture, translation);
    }

    /// <summary>
    /// 获取待翻译的条目。
    /// </summary>
    public IEnumerable<WorkflowEntry> GetPendingEntries()
    {
        return _pendingEntries.Values.OrderBy(e => e.Id);
    }

    /// <summary>
    /// 获取已翻译的条目。
    /// </summary>
    public IEnumerable<WorkflowEntry> GetTranslatedEntries()
    {
        return _translatedEntries.Values.OrderBy(e => e.Id);
    }

    /// <summary>
    /// 获取所有条目。
    /// </summary>
    public IEnumerable<WorkflowEntry> GetAllEntries()
    {
        return _pendingEntries.Values.Concat(_translatedEntries.Values).OrderBy(e => e.Id);
    }

    /// <summary>
    /// 标记条目为已翻译。
    /// </summary>
    public bool MarkAsTranslated(string entryId)
    {
        if (_pendingEntries.TryRemove(entryId, out var entry))
        {
            _translatedEntries.TryAdd(entryId, entry);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 导出为待翻译文档（JSON 格式）。
    /// </summary>
    public string ExportToLocalizationJson()
    {
        var data = new LocalizationData
        {
            Culture = _namespace,
            Entries = GetAllEntries().Select(e => new LocalizationEntry
            {
                Key = e.Key,
                Namespace = e.Namespace,
                Category = e.Category,
                Text = e.Source,
                Context = e.Context,
                Comment = e.Comment
            }).ToList()
        };

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    /// <summary>
    /// 导出为翻译表格（类似 PO 文件格式）。
    /// </summary>
    /// <param name="cultures">要包含的文化列表。</param>
    public string ExportToTranslationTable(string[] cultures)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# PrismaLocalization Translation Table");
        sb.AppendLine($"# Namespace: {_namespace}");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("Key\tSource\t" + string.Join("\t", cultures));

        foreach (var entry in GetAllEntries())
        {
            sb.Append(entry.Key);
            sb.Append('\t');
            sb.Append(entry.Source);

            foreach (var culture in cultures)
            {
                sb.Append('\t');
                if (entry.Translations.TryGetValue(culture, out var translation))
                {
                    sb.Append(translation);
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// 从 JSON 导入翻译。
    /// </summary>
    public void ImportFromJson(string json, string culture)
    {
        var data = JsonConvert.DeserializeObject<LocalizationData>(json);
        if (data?.Entries == null)
            return;

        foreach (var entry in data.Entries)
        {
            var entryId = $"{entry.Namespace ?? _namespace}:{entry.Key}";

            // 查找或创建条目
            var workflowEntry = _pendingEntries.GetOrAdd(entryId, id =>
                new WorkflowEntry
                {
                    Id = id,
                    Namespace = entry.Namespace ?? _namespace,
                    Key = entry.Key,
                    Source = entry.Text,
                    Category = entry.Category,
                    Context = entry.Context,
                    Comment = entry.Comment,
                    Translations = new Dictionary<string, string>()
                });

            workflowEntry.Translations[culture] = entry.Text;
        }
    }

    /// <summary>
    /// 应用翻译到 LocalizationManager。
    /// </summary>
    public void ApplyToManager()
    {
        foreach (var entry in _translatedEntries.Values)
        {
            var key = entry.ToLocalizationKey();

            foreach (var (culture, translation) in entry.Translations)
            {
                // 这里需要通过某种方式更新 LocalizationManager
                // 由于 JsonLocalizationProvider 不直接支持运行时更新，
                // 可以使用 Polyglot 数据机制
            }
        }
    }

    /// <summary>
    /// 清除所有条目。
    /// </summary>
    public void Clear()
    {
        _pendingEntries.Clear();
        _translatedEntries.Clear();
    }

    /// <summary>
    /// 获取统计信息。
    /// </summary>
    public (int total, int translated, int pending) GetStatistics()
    {
        var total = GetAllEntries().Count();
        var translated = _translatedEntries.Count;
        var pending = _pendingEntries.Count;
        return (total, translated, pending);
    }

    /// <summary>
    /// 创建一个新的本地化键并添加到工作流程。
    /// </summary>
    public LocalizationKey CreateKey(
        string source,
        string? customKey = null,
        LocalizationCategory? category = null,
        string? context = null)
    {
        var key = customKey ?? GenerateKeyFromSource(source);
        var cat = category ?? _defaultCategory;

        AddSourceWithKey(source, key);

        return new LocalizationKey(_namespace, key, cat, source);
    }

    /// <summary>
    /// 添加一个带有复数支持的名词。
    /// </summary>
    /// <param name="singular">单数形式。</param>
    /// <param name="plural">复数形式。</param>
    /// <param name="customKey">自定义键名（可选）。</param>
    /// <returns>创建的复数条目。</returns>
    public WorkflowEntry AddNounWithPlural(
        string singular,
        string plural,
        string? customKey = null)
    {
        var key = customKey ?? GenerateKeyFromSource(singular);
        var id = $"{_namespace}:{key}";

        var entry = new WorkflowEntry
        {
            Id = id,
            Namespace = _namespace,
            Key = key,
            Source = singular,
            Category = LocalizationCategory.Noun.ToString(),
            Translations = new Dictionary<string, string>(),
            SupportedVariants = new HashSet<string> { LocalizationVariant.Singular.ToString(), LocalizationVariant.Plural.ToString() },
            VariantTranslations = new Dictionary<string, string>
            {
                [$":{LocalizationVariant.Singular.ToSuffix()}"] = singular,
                [$":{LocalizationVariant.Plural.ToSuffix()}"] = plural
            }
        };

        _pendingEntries.TryAdd(id, entry);
        return entry;
    }

    /// <summary>
    /// 添加一个人称代词及其变格。
    /// </summary>
    /// <param name="nominative">主格形式（例如：I, he, she）。</param>
    /// <param name="accusative">宾格形式（例如：me, him, her）。</param>
    /// <param name="customKey">自定义键名（可选）。</param>
    /// <returns>创建的代词条目。</returns>
    public WorkflowEntry AddPronounWithDeclensions(
        string nominative,
        string accusative,
        string? customKey = null)
    {
        return AddPronounWithDeclensions(nominative, accusative, null, null, customKey);
    }

    /// <summary>
    /// 添加一个人称代词及其完整变格。
    /// </summary>
    /// <param name="nominative">主格形式。</param>
    /// <param name="accusative">宾格形式。</param>
    /// <param name="genitive">所有格形式（可选）。</param>
    /// <param name="possessiveDeterminer">所有格限定词（可选）。</param>
    /// <param name="customKey">自定义键名（可选）。</param>
    /// <returns>创建的代词条目。</returns>
    public WorkflowEntry AddPronounWithDeclensions(
        string nominative,
        string accusative,
        string? genitive,
        string? possessiveDeterminer,
        string? customKey = null)
    {
        var key = customKey ?? GenerateKeyFromSource(nominative);
        var id = $"{_namespace}:{key}";

        var entry = new WorkflowEntry
        {
            Id = id,
            Namespace = _namespace,
            Key = key,
            Source = nominative,
            Category = LocalizationCategory.Pronoun.ToString(),
            Translations = new Dictionary<string, string>(),
            SupportedVariants = new HashSet<string>
            {
                LocalizationVariant.Nominative.ToString(),
                LocalizationVariant.Accusative.ToString()
            },
            VariantTranslations = new Dictionary<string, string>
            {
                [$":{LocalizationVariant.Nominative.ToSuffix()}"] = nominative,
                [$":{LocalizationVariant.Accusative.ToSuffix()}"] = accusative
            }
        };

        if (genitive != null)
        {
            entry.SupportedVariants.Add(LocalizationVariant.Genitive.ToString());
            entry.VariantTranslations[$":{LocalizationVariant.Genitive.ToSuffix()}"] = genitive;
        }

        if (possessiveDeterminer != null)
        {
            entry.SupportedVariants.Add(LocalizationVariant.PossessiveDeterminer.ToString());
            entry.VariantTranslations[$":{LocalizationVariant.PossessiveDeterminer.ToSuffix()}"] = possessiveDeterminer;
        }

        _pendingEntries.TryAdd(id, entry);
        return entry;
    }

    /// <summary>
    /// 为名词条目设置复数翻译。
    /// </summary>
    public bool SetPluralTranslation(string key, string culture, string singular, string plural)
    {
        var entryId = $"{_namespace}:{key}";
        if (!_pendingEntries.TryGetValue(entryId, out var entry))
            return false;

        entry.Translations[culture] = singular;
        entry.SetVariantTranslation(culture, LocalizationVariant.Singular, singular);
        entry.SetVariantTranslation(culture, LocalizationVariant.Plural, plural);

        return true;
    }

    /// <summary>
    /// 为代词条目设置变格翻译。
    /// </summary>
    public bool SetPronounDeclensions(
        string key,
        string culture,
        string? nominative = null,
        string? accusative = null,
        string? genitive = null,
        string? possessiveDeterminer = null)
    {
        var entryId = $"{_namespace}:{key}";
        if (!_pendingEntries.TryGetValue(entryId, out var entry))
            return false;

        if (nominative != null)
        {
            entry.Translations[culture] = nominative;
            entry.SetVariantTranslation(culture, LocalizationVariant.Nominative, nominative);
        }
        if (accusative != null)
            entry.SetVariantTranslation(culture, LocalizationVariant.Accusative, accusative);
        if (genitive != null)
            entry.SetVariantTranslation(culture, LocalizationVariant.Genitive, genitive);
        if (possessiveDeterminer != null)
            entry.SetVariantTranslation(culture, LocalizationVariant.PossessiveDeterminer, possessiveDeterminer);

        return true;
    }

    /// <summary>
    /// 导出包含变体的翻译表格。
    /// </summary>
    public string ExportToVariantTranslationTable(string[] cultures, LocalizationVariant[] variants)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# PrismaLocalization Translation Table with Variants");
        sb.AppendLine($"# Namespace: {_namespace}");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // 表头
        sb.Append("Key\tCategory\tSource");
        foreach (var culture in cultures)
        {
            sb.Append($"\t{culture}");
            foreach (var variant in variants)
            {
                sb.Append($"\t{culture}:{variant.ToSuffix()}");
            }
        }
        sb.AppendLine();

        foreach (var entry in GetAllEntries())
        {
            sb.Append(entry.Key);
            sb.Append('\t');
            sb.Append(entry.Category);
            sb.Append('\t');
            sb.Append(entry.Source);

            foreach (var culture in cultures)
            {
                // 基础翻译
                sb.Append('\t');
                if (entry.Translations.TryGetValue(culture, out var translation))
                {
                    sb.Append(translation);
                }

                // 变体翻译
                foreach (var variant in variants)
                {
                    sb.Append('\t');
                    var variantTranslation = entry.GetVariantTranslation(culture, variant);
                    sb.Append(variationTranslation ?? "");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

/// <summary>
/// 本地化工作流程的扩展方法。
/// </summary>
public static class LocalizationWorkflowExtensions
{
    /// <summary>
    /// 从字符串创建本地化键并添加到工作流程。
    /// </summary>
    public static LocalizationKey Localize(
        this string source,
        LocalizationWorkflow workflow,
        string? customKey = null,
        LocalizationCategory? category = null)
    {
        return workflow.CreateKey(source, customKey, category);
    }

    /// <summary>
    /// 批量本地化多个字符串。
    /// </summary>
    public static Dictionary<string, LocalizationKey> LocalizeBatch(
        this IEnumerable<string> sources,
        LocalizationWorkflow workflow,
        LocalizationCategory category = LocalizationCategory.General)
    {
        var result = new Dictionary<string, LocalizationKey>();

        foreach (var source in sources)
        {
            var key = workflow.CreateKey(source, null, category);
            result[source] = key;
        }

        return result;
    }
}
