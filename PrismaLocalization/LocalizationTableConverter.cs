using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PrismaLocalization;

/// <summary>
/// 本地化表格转换器。
/// 用于在结构化词条和扁平化翻译表格之间转换，
/// 并保护格式化占位符不被翻译人员误修改。
/// </summary>
public class LocalizationTableConverter
{
    private readonly LocalizationWorkflow _workflow;
    private readonly List<PlaceholderPattern> _placeholderPatterns = new();

    /// <summary>
    /// 占位符模式。
    /// </summary>
    public class PlaceholderPattern
    {
        /// <summary>
        /// 模式类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 正则表达式模式。
        /// </summary>
        public Regex Regex { get; set; } = null!;

        /// <summary>
        /// 占位符前缀。
        /// </summary>
        public string Prefix { get; set; } = "{";

        /// <summary>
        /// 占位符后缀。
        /// </summary>
        public string Suffix { get; set; } = "}";

        /// <summary>
        /// 是否为索引占位符。
        /// </summary>
        public bool IsIndexed { get; set; }

        /// <summary>
        /// 匹配组名。
        /// </summary>
        public string GroupName { get; set; } = "content";
    }

    /// <summary>
    /// 替换条目。
    /// </summary>
    public class ReplacementEntry
    {
        /// <summary>
        /// 原始文本。
        /// </summary>
        public string Original { get; set; } = string.Empty;

        /// <summary>
        /// 替换后的文本。
        /// </summary>
        public string Replaced { get; set; } = string.Empty;

        /// <summary>
        /// 占位符映射（占位符 -> 原始表达式）。
        /// </summary>
        public Dictionary<string, string> PlaceholderMap { get; set; } = new();
    }

    /// <summary>
    /// 扁平化表格行。
    /// </summary>
    public class FlatTableRow
    {
        /// <summary>
        /// 键名。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 命名空间。
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// 分类。
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 原文。
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 上下文。
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
        /// 最大长度限制（可选）。
        /// </summary>
        public int? MaxLength { get; set; }
    }

    /// <summary>
    /// 初始化 LocalizationTableConverter 的新实例。
    /// </summary>
    public LocalizationTableConverter(LocalizationWorkflow? workflow = null)
    {
        _workflow = workflow ?? new LocalizationWorkflow();
        InitializeDefaultPatterns();
    }

    /// <summary>
    /// 初始化默认的占位符模式。
    /// </summary>
    private void InitializeDefaultPatterns()
    {
        // SmartFormat 索引占位符: {0}, {1}, {2}...
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "Indexed",
            Regex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = true,
            GroupName = "index"
        });

        // SmartFormat 命名占位符: {Name}, {PlayerName}...
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "Named",
            Regex = new Regex(@"\{([a-zA-Z_][a-zA-Z0-9_]*)\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "name"
        });

        // ICU 复数形式: {count, plural, one{...} other{...}}
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "ICU_Plural",
            Regex = new Regex(@"\{(\w+),\s*plural,([^}]+)\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "plural_content"
        });

        // ICU 选择形式: {gender, select, male{...} female{...} other{...}}
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "ICU_Select",
            Regex = new Regex(@"\{(\w+),\s*select,([^}]+)\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "select_content"
        });

        // ICU 序数形式: {place, selectordinal, one{...} other{...}}
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "ICU_Ordinal",
            Regex = new Regex(@"\{(\w+),\s*selectordinal,([^}]+)\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "ordinal_content"
        });

        // ICU 日期/时间: {date, date}
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "ICU_Date",
            Regex = new Regex(@"\{(\w+),\s*date\s*(?:,\s*([^}]*))?\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "date_content"
        });

        // ICU 数字: {num, number}
        _placeholderPatterns.Add(new PlaceholderPattern
        {
            Type = "ICU_Number",
            Regex = new Regex(@"\{(\w+),\s*number\s*(?:,\s*([^}]*))?\}", RegexOptions.Compiled),
            Prefix = "{",
            Suffix = "}",
            IsIndexed = false,
            GroupName = "number_content"
        });
    }

    /// <summary>
    /// 添加自定义占位符模式。
    /// </summary>
    public void AddPlaceholderPattern(PlaceholderPattern pattern)
    {
        _placeholderPatterns.Add(pattern);
    }

    /// <summary>
    /// 转换单个文本，保护占位符。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>替换条目。</returns>
    public ReplacementEntry ProtectPlaceholders(string text)
    {
        var result = new ReplacementEntry
        {
            Original = text,
            Replaced = text,
            PlaceholderMap = new Dictionary<string, string>()
        };

        var placeholderIndex = 0;

        // 按优先级处理各种模式
        // 先处理 ICU 复杂模式（因为它们包含嵌套的大括号）
        foreach (var pattern in _placeholderPatterns.Where(p => p.Type.StartsWith("ICU_")))
        {
            result = ProcessPattern(result, pattern, ref placeholderIndex);
        }

        // 再处理简单的命名和索引占位符
        foreach (var pattern in _placeholderPatterns.Where(p => !p.Type.StartsWith("ICU_")))
        {
            result = ProcessPattern(result, pattern, ref placeholderIndex);
        }

        return result;
    }

    /// <summary>
    /// 处理单个模式。
    /// </summary>
    private ReplacementEntry ProcessPattern(
        ReplacementEntry entry,
        PlaceholderPattern pattern,
        ref int placeholderIndex)
    {
        var matches = pattern.Regex.Matches(entry.Replaced).Cast<Match>().ToList();
        if (!matches.Any())
            return entry;

        // 按位置倒序处理，避免索引变化
        foreach (var match in matches.OrderByDescending(m => m.Index))
        {
            var originalMatch = match.Value;
            var placeholder = $"{pattern.Prefix}{placeholderIndex}{pattern.Suffix}";

            entry.PlaceholderMap[placeholder] = originalMatch;
            entry.Replaced = entry.Replaced.Remove(match.Index, match.Length)
                                   .Insert(match.Index, placeholder);

            placeholderIndex++;
        }

        return entry;
    }

    /// <summary>
    /// 还原占位符为原始表达式。
    /// </summary>
    /// <param name="text">包含占位符的文本。</param>
    /// <param name="placeholderMap">占位符映射。</param>
    /// <returns>还原后的文本。</returns>
    public string RestorePlaceholders(string text, Dictionary<string, string> placeholderMap)
    {
        var result = text;

        // 按占位符索引降序处理，避免替换冲突
        foreach (var kvp in placeholderMap.OrderByDescending(x =>
        {
            var match = Regex.Match(x.Key, @"\{(\d+)\}");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }))
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }

        return result;
    }

    /// <summary>
    /// 将工作流程条目转换为扁平化表格。
    /// </summary>
    /// <param name="cultures">目标文化列表。</param>
    /// <param name="protectPlaceholders">是否保护占位符。</param>
    /// <returns>扁平化表格行列表。</returns>
    public List<FlatTableRow> FlattenToTable(
        string[] cultures,
        bool protectPlaceholders = true)
    {
        var rows = new List<FlatTableRow>();
        var placeholderMaps = new Dictionary<string, Dictionary<string, string>>();

        foreach (var entry in _workflow.GetAllEntries())
        {
            var row = new FlatTableRow
            {
                Key = entry.Key,
                Namespace = entry.Namespace,
                Category = entry.Category,
                Source = entry.Source,
                Context = entry.Context,
                Comment = entry.Comment,
                Translations = new Dictionary<string, string>()
            };

            // 处理原文
            if (protectPlaceholders)
            {
                var sourceReplacement = ProtectPlaceholders(entry.Source);
                row.Source = sourceReplacement.Replaced;
                placeholderMaps[$"{entry.Namespace}:{entry.Key}:source"] = sourceReplacement.PlaceholderMap;
            }
            else
            {
                row.Source = entry.Source;
            }

            // 处理各文化的翻译
            foreach (var culture in cultures)
            {
                var translation = entry.Translations.TryGetValue(culture, out var t) ? t : "";
                var mapKey = $"{entry.Namespace}:{entry.Key}:{culture}";

                if (protectPlaceholders && !string.IsNullOrEmpty(translation))
                {
                    var translationReplacement = ProtectPlaceholders(translation);
                    row.Translations[culture] = translationReplacement.Replaced;
                    placeholderMaps[mapKey] = translationReplacement.PlaceholderMap;
                }
                else
                {
                    row.Translations[culture] = translation;
                }
            }

            rows.Add(row);
        }

        // 存储映射供导入使用
        _storedPlaceholderMaps = placeholderMaps;

        return rows;
    }

    private Dictionary<string, Dictionary<string, string>> _storedPlaceholderMaps = new();

    /// <summary>
    /// 导出为 CSV 格式。
    /// </summary>
    /// <param name="cultures">目标文化列表。</param>
    /// <param name="delimiter">分隔符（默认为制表符）。</param>
    /// <returns>CSV 字符串。</returns>
    public string ExportToCsv(
        string[] cultures,
        string delimiter = "\t")
    {
        var rows = FlattenToTable(cultures, protectPlaceholders: true);

        var sb = new System.Text.StringBuilder();

        // 表头
        var headers = new List<string> { "Key", "Namespace", "Category", "Source", "Context", "Comment" };
        foreach (var culture in cultures)
        {
            headers.Add(culture);
        }
        headers.Add("MaxLength");
        sb.AppendLine(string.Join(delimiter, headers));

        // 数据行
        foreach (var row in rows)
        {
            var values = new List<string>
            {
                EscapeCsvValue(row.Key, delimiter),
                EscapeCsvValue(row.Namespace, delimiter),
                EscapeCsvValue(row.Category, delimiter),
                EscapeCsvValue(row.Source, delimiter),
                EscapeCsvValue(row.Context ?? "", delimiter),
                EscapeCsvValue(row.Comment ?? "", delimiter)
            };

            foreach (var culture in cultures)
            {
                values.Add(EscapeCsvValue(row.Translations.GetValueOrDefault(culture, ""), delimiter));
            }

            values.Add(row.MaxLength?.ToString() ?? "");
            sb.AppendLine(string.Join(delimiter, values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// 导出为 Excel 友好的 CSV 格式（使用逗号分隔）。
    /// </summary>
    public string ExportToExcelCsv(string[] cultures)
    {
        return ExportToCsv(cultures, ",");
    }

    /// <summary>
    /// 从 CSV 导入翻译。
    /// </summary>
    /// <param name="csv">CSV 内容。</param>
    /// <param name="delimiter">分隔符。</param>
    /// <param name="sourceCulture">源文化（用于获取占位符映射）。</param>
    /// <param name="targetCultures">目标文化列表。</param>
    /// <returns>导入的条目数量。</returns>
    public int ImportFromCsv(
        string csv,
        string delimiter = "\t",
        string? sourceCulture = null,
        string[]? targetCultures = null)
    {
        var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            return 0;

        // 解析表头
        var headers = ParseCsvLine(lines[0], delimiter);
        var cultureColumns = new List<int>();

        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i].Trim();
            if (header == "Key" || header == "Namespace" || header == "Category" ||
                header == "Source" || header == "Context" || header == "Comment" ||
                header == "MaxLength")
                continue;

            cultureColumns.Add(i);
        }

        if (targetCultures == null)
        {
            targetCultures = cultureColumns.Select(i => headers[i]).ToArray();
        }

        var importedCount = 0;

        // 解析数据行
        for (int row = 1; row < lines.Length; row++)
        {
            var values = ParseCsvLine(lines[row], delimiter);
            if (values.Count < headers.Count)
                continue;

            var key = UnescapeCsvValue(values[headers.IndexOf("Key")]);
            var ns = UnescapeCsvValue(values[headers.IndexOf("Namespace")]);
            var entryId = $"{ns}:{key}";

            foreach (var culture in targetCultures)
            {
                var colIndex = headers.IndexOf(culture);
                if (colIndex < 0 || colIndex >= values.Count)
                    continue;

                var translated = UnescapeCsvValue(values[colIndex]);
                if (string.IsNullOrEmpty(translated))
                    continue;

                // 还原占位符
                var mapKey = $"{entryId}:{culture}";
                if (_storedPlaceholderMaps.TryGetValue(mapKey, out var placeholderMap))
                {
                    translated = RestorePlaceholders(translated, placeholderMap);
                }

                // 添加翻译
                if (_workflow.AddTranslation(entryId, culture, translated))
                {
                    importedCount++;
                }
            }
        }

        return importedCount;
    }

    /// <summary>
    /// 解析 CSV 行。
    /// </summary>
    private List<string> ParseCsvLine(string line, string delimiter)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes && line.Substring(i).StartsWith(delimiter))
            {
                result.Add(current.ToString());
                current.Clear();
                i += delimiter.Length - 1;
            }
            else
            {
                current.Append(ch);
            }
        }

        result.Add(current.ToString());
        return result;
    }

    /// <summary>
    /// 转义 CSV 值。
    /// </summary>
    private string EscapeCsvValue(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        var needsQuotes = value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r');

        if (!needsQuotes)
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    /// <summary>
    /// 反转义 CSV 值。
    /// </summary>
    private string UnescapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            return value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
        }

        return value;
    }

    /// <summary>
    /// 导出为 XLSX 格式（使用简化格式）。
    /// 注意：此方法返回一个 HTML 表格，Excel 可以打开。
    /// </summary>
    public string ExportToExcelHtml(string[] cultures)
    {
        var rows = FlattenToTable(cultures, protectPlaceholders: true);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<style>");
        sb.AppendLine("table { border-collapse: collapse; }");
        sb.AppendLine("td, th { border: 1px solid #ccc; padding: 8px; text-align: left; }");
        sb.AppendLine("th { background-color: #f0f0f0; font-weight: bold; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<table>");

        // 表头
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>Key</th>");
        sb.AppendLine("<th>Namespace</th>");
        sb.AppendLine("<th>Category</th>");
        sb.AppendLine("<th>Source</th>");
        sb.AppendLine("<th>Context</th>");
        sb.AppendLine("<th>Comment</th>");
        foreach (var culture in cultures)
        {
            sb.AppendLine($"<th>{culture}</th>");
        }
        sb.AppendLine("</tr>");

        // 数据行
        foreach (var row in rows)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Key)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Namespace)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Category)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Source)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Context ?? "")}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Comment ?? "")}</td>");
            foreach (var culture in cultures)
            {
                sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(row.Translations.GetValueOrDefault(culture, ""))}</td>");
            }
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// 保存占位符映射到文件。
    /// </summary>
    public void SavePlaceholderMaps(string filePath)
    {
        var json = JsonConvert.SerializeObject(_storedPlaceholderMaps, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从文件加载占位符映射。
    /// </summary>
    public void LoadPlaceholderMaps(string filePath)
    {
        var json = File.ReadAllText(filePath);
        _storedPlaceholderMaps = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json)
            ?? new Dictionary<string, Dictionary<string, string>>();
    }
}

/// <summary>
/// 表格转换器的扩展方法。
/// </summary>
public static class LocalizationTableConverterExtensions
{
    /// <summary>
    /// 导出翻译表格并保存到文件。
    /// </summary>
    public static void ExportAndSave(
        this LocalizationWorkflow workflow,
        string filePath,
        string[] cultures,
        TableFormat format = TableFormat.Tsv)
    {
        var converter = new LocalizationTableConverter(workflow);

        var content = format switch
        {
            TableFormat.Tsv => converter.ExportToCsv(cultures, "\t"),
            TableFormat.Csv => converter.ExportToExcelCsv(cultures),
            TableFormat.ExcelHtml => converter.ExportToExcelHtml(cultures),
            _ => converter.ExportToCsv(cultures, "\t")
        };

        File.WriteAllText(filePath, content);

        // 保存占位符映射
        var mapPath = Path.ChangeExtension(filePath, ".map.json");
        converter.SavePlaceholderMaps(mapPath);
    }

    /// <summary>
    /// 从文件导入翻译表格。
    /// </summary>
    public static int ImportFromFile(
        this LocalizationWorkflow workflow,
        string filePath,
        string[]? targetCultures = null)
    {
        var converter = new LocalizationTableConverter(workflow);

        // 加载占位符映射
        var mapPath = Path.ChangeExtension(filePath, ".map.json");
        if (File.Exists(mapPath))
        {
            converter.LoadPlaceholderMaps(mapPath);
        }

        var content = File.ReadAllText(filePath);
        var delimiter = Path.GetExtension(filePath).ToLower() switch
        {
            ".csv" => ",",
            _ => "\t"
        };

        return converter.ImportFromCsv(content, delimiter, targetCultures: targetCultures);
    }
}

/// <summary>
/// 表格格式。
/// </summary>
public enum TableFormat
{
    /// <summary>
    /// 制表符分隔值（TSV）。
    /// </summary>
    Tsv,

    /// <summary>
    /// 逗号分隔值（CSV）。
    /// </summary>
    Csv,

    /// <summary>
    /// Excel HTML 格式。
    /// </summary>
    ExcelHtml
}
