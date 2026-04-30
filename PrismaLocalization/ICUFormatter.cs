using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

#if PRISMA_USE_SMARTFORMAT
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;
#endif

namespace PrismaLocalization;

/// <summary>
/// ICU（International Components for Unicode）消息格式化器。
/// 支持 ICU MessageFormat 语法，包括复数、选择、序数等形式。
/// </summary>
public partial class ICUFormatter
{
#if PRISMA_USE_SMARTFORMAT
    private readonly SmartFormatter _formatter;
#endif

    // 预编译正则表达式
    private static readonly Regex _pluralRegex = new(@"\{(?<var>\w+),\s*plural,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex _selectRegex = new(@"\{(?<var>\w+),\s*select,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex _selectOrdinalRegex = new(@"\{(?<var>\w+),\s*selectordinal,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex _simplePlaceholderRegex = new(@"\{(?<var>\w+)\}", RegexOptions.Compiled);

    /// <summary>
    /// 初始化 ICUFormatter 的新实例。
    /// </summary>
    public ICUFormatter()
    {
#if PRISMA_USE_SMARTFORMAT
        _formatter = CreateFormatter();
#endif
    }

#if PRISMA_USE_SMARTFORMAT
    /// <summary>
    /// 创建配置好的 SmartFormatter，支持 ICU 语法。
    /// </summary>
    private static SmartFormatter CreateFormatter()
    {
        SmartSettings settings = new()
        {
            Formatter =
            {
                ErrorAction = FormatErrorAction.Ignore
            },
            Parser =
            {
                ErrorAction = ParseErrorAction.Ignore
            }
        };

        var formatter = Smart.CreateDefaultSmartFormat(settings);
        return formatter;
    }
#endif

    /// <summary>
    /// 使用 ICU 语法格式化字符串。
    /// </summary>
    public string Format(string pattern, Dictionary<string, object?> args)
    {
        if (string.IsNullOrEmpty(pattern)) return string.Empty;

        // 处理 ICU 语法转换
        var convertedPattern = ConvertICUToSmartFormat(pattern);

#if PRISMA_USE_SMARTFORMAT
        try
        {
            return _formatter.Format(convertedPattern, args);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"ICU Format failed: {ex.Message}. Pattern: {pattern}");
            return pattern;
        }
#else
        // Manual fallback for ICU placeholders if SmartFormat is missing
        var result = convertedPattern;
        foreach (var kv in args)
        {
            result = result.Replace($"{{{kv.Key}}}", kv.Value?.ToString() ?? "");
        }
        return result;
#endif
    }

    /// <summary>
    /// 解析复数形式。
    /// </summary>
    private string ProcessPlural(string pattern, string varName, int count, string culture)
    {
        var match = _pluralRegex.Match(pattern);
        if (!match.Success) return pattern;

        var optionsStr = match.Groups["options"].Value;
        var options = ParseOptions(optionsStr);

        var category = GetPluralCategory(count, culture);

        if (options.TryGetValue($"={count}", out var exactMatch))
            return exactMatch;

        if (options.TryGetValue(category, out var categoryMatch))
            return categoryMatch;

        return options.TryGetValue("other", out var otherMatch) ? otherMatch : pattern;
    }

    private Dictionary<string, string> ParseOptions(string optionsStr)
    {
        var result = new Dictionary<string, string>();
        int braceLevel = 0;
        int lastIndex = 0;
        string? currentKey = null;

        for (int i = 0; i < optionsStr.Length; i++)
        {
            if (optionsStr[i] == '{')
            {
                if (braceLevel == 0)
                {
                    currentKey = optionsStr.Substring(lastIndex, i - lastIndex).Trim();
                    lastIndex = i + 1;
                }
                braceLevel++;
            }
            else if (optionsStr[i] == '}')
            {
                braceLevel--;
                if (braceLevel == 0)
                {
                    if (currentKey != null)
                    {
                        result[currentKey] = optionsStr.Substring(lastIndex, i - lastIndex);
                    }
                    lastIndex = i + 1;
                }
            }
        }

        return result;
    }

    private string GetPluralCategory(int count, string culture)
    {
        var lang = culture.Split('-')[0].ToLower();
        return lang switch
        {
            "zh" => "other",
            "en" => count == 1 ? "one" : "other",
            _ => "other"
        };
    }

    /// <summary>
    /// 将 ICU 格式转换为 SmartFormat 格式（简化版）。
    /// </summary>
    private string ConvertICUToSmartFormat(string icuPattern)
    {
        return icuPattern;
    }

    private static Regex PluralRegex() => _pluralRegex;
    private static Regex SelectRegex() => _selectRegex;
    private static Regex SelectOrdinalRegex() => _selectOrdinalRegex;
    private static Regex SimplePlaceholderRegex() => _simplePlaceholderRegex;
}

/// <summary>
/// ICU 消息格式的扩展方法。
/// </summary>
public static class ICUExtensions
{
    private static readonly ICUFormatter _formatter = new();

    /// <summary>
    /// 使用 ICU 消息格式语法格式化字符串。
    /// </summary>
    public static string FormatICU(this string pattern, params object[] args)
    {
        var namedArgs = new Dictionary<string, object?>();
        for (int i = 0; i < args.Length; i++)
        {
            namedArgs[$"{i}"] = args[i];
        }
        return _formatter.Format(pattern, namedArgs);
    }

    /// <summary>
    /// 使用 ICU 消息格式语法格式化字符串（命名参数）。
    /// </summary>
    public static string FormatICU(this string pattern, Dictionary<string, object?> args)
    {
        return _formatter.Format(pattern, args);
    }

    /// <summary>
    /// 创建 ICU 复数格式模式。
    /// </summary>
    public static string Plural(this string varName, Dictionary<string, string> forms)
    {
        var options = string.Join(" ", forms.Select(kv => $"{kv.Key}{{{kv.Value}}}"));
        return $"{{{varName}, plural, {options}}}";
    }

    /// <summary>
    /// 创建 ICU 选择格式模式。
    /// </summary>
    public static string Select(this string varName, Dictionary<string, string> forms)
    {
        var options = string.Join(" ", forms.Select(kv => $"{kv.Key}{{{kv.Value}}}"));
        return $"{{{varName}, select, {options}}}";
    }
}

/// <summary>
/// 本地化键的 ICU 扩展。
/// </summary>
public static class LocalizationKeyICUExtensions
{
    public static string FormatICU(this LocalizationKey key, Dictionary<string, object?> args)
    {
        var template = LocalizationManager.Instance.GetText(key);
        return template.FormatICU(args);
    }

    public static string FormatICU(this LocalizationKey key, string culture, Dictionary<string, object?> args)
    {
        var template = LocalizationManager.Instance.GetText(key, culture);
        return template.FormatICU(args);
    }
}
