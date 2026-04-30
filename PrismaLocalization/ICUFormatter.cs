using System.Globalization;
using System.Text.RegularExpressions;
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;

namespace PrismaLocalization;

/// <summary>
/// ICU（International Components for Unicode）消息格式化器。
/// 支持 ICU MessageFormat 语法，包括复数、选择、序数等形式。
/// </summary>
public partial class ICUFormatter
{
    private readonly SmartFormatter _formatter;

    /// <summary>
    /// 初始化 ICUFormatter 的新实例。
    /// </summary>
    public ICUFormatter()
    {
        _formatter = CreateFormatter();
    }

    /// <summary>
    /// 创建配置好的 SmartFormatter，支持 ICU 语法。
    /// </summary>
    private static SmartFormatter CreateFormatter()
    {
        SmartSettings settings = new()
        {
            Formatter =
            {
                ErrorAction = FormatErrorAction.ThrowError
            },
            Parser =
            {
                ErrorAction = ParseErrorAction.ThrowError
            }
        };

        var formatter = Smart.CreateDefaultSmartFormat(settings);
        // 添加 ICU 风格的扩展
        // 注意：SmartFormat 本身不直接支持 ICU 语法，我们需要转换

        return formatter;
    }

    /// <summary>
    /// 使用 ICU 消息格式语法格式化文本。
    /// </summary>
    /// <param name="pattern">ICU 格式模式。</param>
    /// <param name="args">参数字典。</param>
    /// <returns>格式化后的文本。</returns>
    public string Format(string pattern, Dictionary<string, object?> args)
    {
        if (args.Count == 0)
            return pattern;

        // 将 ICU 格式转换为 SmartFormat 格式
        var convertedPattern = ConvertICUToSmartFormat(pattern);

        try
        {
            return _formatter.Format(convertedPattern, args);
        }
        catch
        {
            // 如果转换失败，尝试手动处理 ICU 语法
            return FormatICUPattern(pattern, args);
        }
    }

    /// <summary>
    /// 使用 ICU 消息格式语法格式化文本（位置参数）。
    /// </summary>
    /// <param name="pattern">ICU 格式模式。</param>
    /// <param name="args">参数数组。</param>
    /// <returns>格式化后的文本。</returns>
    public string Format(string pattern, params object[] args)
    {
        if (args.Length == 0)
            return pattern;

        var namedArgs = new Dictionary<string, object?>();
        for (int i = 0; i < args.Length; i++)
        {
            namedArgs[$"{i}"] = args[i];
        }

        return Format(pattern, namedArgs);
    }

    /// <summary>
    /// 手动处理 ICU 格式模式。
    /// </summary>
    private string FormatICUPattern(string pattern, Dictionary<string, object?> args)
    {
        var result = pattern;

        // 处理复数形式: {name, plural, one{...} other{...}}
        result = ProcessPluralForms(result, args);

        // 处理选择形式: {name, select, male{...} female{...} other{...}}
        result = ProcessSelectForms(result, args);

        // 处理序数形式: {name, selectordinal, one{...} two{...} few{...} other{...}}
        result = ProcessSelectOrdinalForms(result, args);

        // 替换简单占位符: {name}
        result = ReplaceSimplePlaceholders(result, args);

        return result;
    }

    /// <summary>
    /// 处理复数形式。
    /// ICU 语法: {count, plural, one{# cat} other{# cats}}
    /// </summary>
    private string ProcessPluralForms(string pattern, Dictionary<string, object?> args)
    {
        var regex = PluralRegex();
        var matches = regex.Matches(pattern);

        var result = pattern;
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var varName = match.Groups["var"].Value;
            var options = match.Groups["options"].Value;

            if (!args.TryGetValue(varName, out var value) || value == null)
                continue;

            var number = Convert.ToDouble(value);
            var selectedOption = GetPluralCategory(number);

            // 解析选项
            var optionValue = ParsePluralOptions(options, number, selectedOption);
            result = result.Remove(match.Index, match.Length).Insert(match.Index, optionValue);
        }

        return result;
    }

    /// <summary>
    /// 解析复数选项并返回匹配的值。
    /// </summary>
    private string ParsePluralOptions(string options, double number, string category)
    {
        // 解析格式: one{# item} other{# items}
        var pattern = @"(\w+)\{([^}]*)\}";
        var matches = Regex.Matches(options, pattern);

        foreach (Match match in matches.Cast<Match>())
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            if (key == category)
            {
                return value.Replace("#", number.ToString(CultureInfo.InvariantCulture));
            }
        }

        // 回退到 'other'
        foreach (Match match in matches.Cast<Match>())
        {
            if (match.Groups[1].Value == "other")
            {
                var value = match.Groups[2].Value;
                return value.Replace("#", number.ToString(CultureInfo.InvariantCulture));
            }
        }

        return number.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 获取数字的复数类别（基于英语规则）。
    /// 实际实现应使用 CLDR 数据支持所有语言。
    /// </summary>
    private string GetPluralCategory(double number)
    {
        if (number == 0)
            return "zero";
        if (number == 1)
            return "one";
        if (number == 2)
            return "two";
        return "other";
    }

    /// <summary>
    /// 处理选择形式。
    /// ICU 语法: {gender, select, male{He} female{She} other{They}}
    /// </summary>
    private string ProcessSelectForms(string pattern, Dictionary<string, object?> args)
    {
        var regex = SelectRegex();
        var matches = regex.Matches(pattern);

        var result = pattern;
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var varName = match.Groups["var"].Value;
            var options = match.Groups["options"].Value;

            if (!args.TryGetValue(varName, out var value))
                continue;

            var selectedOption = value?.ToString() ?? "other";
            var optionValue = ParseSelectOptions(options, selectedOption);
            result = result.Remove(match.Index, match.Length).Insert(match.Index, optionValue);
        }

        return result;
    }

    /// <summary>
    /// 解析选择选项并返回匹配的值。
    /// </summary>
    private string ParseSelectOptions(string options, string selectedKey)
    {
        // 解析格式: male{He} female{She} other{They}
        var pattern = @"(\w+)\{([^}]*)\}";
        var matches = Regex.Matches(options, pattern);

        foreach (Match match in matches.Cast<Match>())
        {
            if (match.Groups[1].Value == selectedKey)
            {
                return match.Groups[2].Value;
            }
        }

        // 回退到 'other'
        foreach (Match match in matches.Cast<Match>())
        {
            if (match.Groups[1].Value == "other")
            {
                return match.Groups[2].Value;
            }
        }

        return selectedKey;
    }

    /// <summary>
    /// 处理序数形式。
    /// ICU 语法: {place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}
    /// </summary>
    private string ProcessSelectOrdinalForms(string pattern, Dictionary<string, object?> args)
    {
        var regex = SelectOrdinalRegex();
        var matches = regex.Matches(pattern);

        var result = pattern;
        foreach (Match match in matches.Cast<Match>().Reverse())
        {
            var varName = match.Groups["var"].Value;
            var options = match.Groups["options"].Value;

            if (!args.TryGetValue(varName, out var value) || value == null)
                continue;

            var number = Convert.ToInt32(value);
            var selectedOption = GetOrdinalCategory(number);

            var optionValue = ParseOrdinalOptions(options, number, selectedOption);
            result = result.Remove(match.Index, match.Length).Insert(match.Index, optionValue);
        }

        return result;
    }

    /// <summary>
    /// 获取数字的序数类别（基于英语规则）。
    /// </summary>
    private string GetOrdinalCategory(int number)
    {
        if (number % 100 is >= 11 and <= 13)
            return "other";

        return (number % 10) switch
        {
            1 => "one",
            2 => "two",
            3 => "few",
            _ => "other"
        };
    }

    /// <summary>
    /// 解析序数选项并返回匹配的值。
    /// </summary>
    private string ParseOrdinalOptions(string options, int number, string category)
    {
        var pattern = @"(\w+)\{([^}]*)\}";
        var matches = Regex.Matches(options, pattern);

        foreach (Match match in matches.Cast<Match>())
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            if (key == category)
            {
                return value.Replace("#", number.ToString(CultureInfo.InvariantCulture));
            }
        }

        // 回退到 'other'
        foreach (Match match in matches.Cast<Match>())
        {
            if (match.Groups[1].Value == "other")
            {
                var value = match.Groups[2].Value;
                return value.Replace("#", number.ToString(CultureInfo.InvariantCulture));
            }
        }

        return number.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 替换简单占位符 {varName}。
    /// </summary>
    private string ReplaceSimplePlaceholders(string pattern, Dictionary<string, object?> args)
    {
        var regex = SimplePlaceholderRegex();
        return regex.Replace(pattern, m =>
        {
            var key = m.Groups["var"].Value;
            return args.TryGetValue(key, out var value) ? value?.ToString() ?? "" : m.Value;
        });
    }

    /// <summary>
    /// 将 ICU 格式转换为 SmartFormat 格式（简化版）。
    /// </summary>
    private string ConvertICUToSmartFormat(string icuPattern)
    {
        // 简单转换：ICU 的 {var} 已经兼容 SmartFormat
        // 复杂的 ICU 语法需要手动处理
        return icuPattern;
    }

    // 正则表达式模式
    [GeneratedRegex(@"\{(?<var>\w+),\s*plural,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex PluralRegex();

    [GeneratedRegex(@"\{(?<var>\w+),\s*select,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex SelectRegex();

    [GeneratedRegex(@"\{(?<var>\w+),\s*selectordinal,\s*(?<options>.+?)\}", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex SelectOrdinalRegex();

    [GeneratedRegex(@"\{(?<var>\w+)\}", RegexOptions.Compiled)]
    private static partial Regex SimplePlaceholderRegex();
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
    /// <param name="pattern">ICU 格式模式。</param>
    /// <param name="args">参数。</param>
    /// <returns>格式化后的字符串。</returns>
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
    /// <param name="pattern">ICU 格式模式。</param>
    /// <param name="args">命名参数字典。</param>
    /// <returns>格式化后的字符串。</returns>
    public static string FormatICU(this string pattern, Dictionary<string, object?> args)
    {
        return _formatter.Format(pattern, args);
    }

    /// <summary>
    /// 创建 ICU 复数格式模式。
    /// </summary>
    public static string Plural(this string pattern, string varName, Dictionary<string, string> forms)
    {
        var options = string.Join(" ", forms.Select(kv => $"{kv.Key}{{{kv.Value}}}"));
        return $"{{{varName}, plural, {options}}}";
    }

    /// <summary>
    /// 创建 ICU 选择格式模式。
    /// </summary>
    public static string Select(this string pattern, string varName, Dictionary<string, string> forms)
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
    /// <summary>
    /// 使用 ICU 参数格式化本地化文本。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="args">命名参数。</param>
    /// <returns>格式化后的文本。</returns>
    public static string FormatICU(this LocalizationKey key, Dictionary<string, object?> args)
    {
        var template = LocalizationManager.Instance.GetText(key);
        return template.FormatICU(args);
    }

    /// <summary>
    /// 使用 ICU 参数格式化本地化文本（指定文化）。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="culture">文化代码。</param>
    /// <param name="args">命名参数。</param>
    /// <returns>格式化后的文本。</returns>
    public static string FormatICU(this LocalizationKey key, string culture, Dictionary<string, object?> args)
    {
        var template = LocalizationManager.Instance.GetText(key, culture);
        return template.FormatICU(args);
    }
}
