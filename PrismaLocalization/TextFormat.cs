using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace PrismaLocalization;

/// <summary>
/// UE 风格的文本格式化工具。
/// 支持复数、性别等参数修饰符。
/// </summary>
public static class TextFormat
{
    private static readonly SmartFormatter _formatter = CreateFormatter();

    /// <summary>
    /// 创建配置好的 SmartFormatter。
    /// </summary>
    private static SmartFormatter CreateFormatter()
    {
        var formatter = Smart.CreateDefaultSmartFormat();
        formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;
        formatter.Settings.ParseErrorAction = ErrorAction.ThrowError;

        // 注册自定义格式化扩展
        // formatter.AddExtensions(new PluralFormatter());

        return formatter;
    }

    /// <summary>
    /// 格式化带有索引参数的文本。
    /// </summary>
    /// <param name="format">格式化模式。</param>
    /// <param name="args">参数数组。</param>
    /// <returns>格式化后的文本。</returns>
    public static string Format(string format, params object[] args)
    {
        if (args.Length == 0)
            return format;

        try
        {
            return _formatter.Format(format, args);
        }
        catch
        {
            return string.Format(format, args);
        }
    }

    /// <summary>
    /// 格式化带有命名参数的文本。
    /// </summary>
    /// <param name="format">格式化模式。</param>
    /// <param name="args">命名参数字典。</param>
    /// <returns>格式化后的文本。</returns>
    public static string FormatNamed(string format, Dictionary<string, object?> args)
    {
        if (args.Count == 0)
            return format;

        try
        {
            return _formatter.Format(format, args);
        }
        catch
        {
            var result = format;
            foreach (var (key, value) in args)
            {
                result = result.Replace($"{{{key}}}", value?.ToString() ?? "");
            }
            return result;
        }
    }

    /// <summary>
    /// 格式化复数形式（UE 风格）。
    /// </summary>
    /// <param name="count">数量。</param>
    /// <param name="forms">复数形式字典（例如：{"one": "cat", "other": "cats"}）。</param>
    /// <returns>正确的复数形式。</returns>
    public static string Plural(int count, Dictionary<string, string> forms)
    {
        var pluralForm = GetPluralForm(count);
        return forms.TryGetValue(pluralForm, out var result)
            ? result
            : forms.TryGetValue("other", out result) ? result : "";
    }

    /// <summary>
    /// 获取数量的复数形式。
    /// </summary>
    private static string GetPluralForm(int count)
    {
        // 简化的复数规则（实际实现应使用 CLDR 数据）
        if (count == 0)
            return "zero";
        if (count == 1)
            return "one";
        if (count == 2)
            return "two";
        return "other";
    }

    /// <summary>
    /// 格式化序数形式（UE 风格）。
    /// </summary>
    /// <param name="number">数字。</param>
    /// <param name="forms">序数形式字典（例如：{"one": "st", "two": "nd", "few": "rd", "other": "th"}）。</param>
    /// <returns>正确的序数形式。</returns>
    public static string Ordinal(int number, Dictionary<string, string> forms)
    {
        var ordinalForm = GetOrdinalForm(number);
        return forms.TryGetValue(ordinalForm, out var result)
            ? result
            : forms.TryGetValue("other", out result) ? result : "";
    }

    /// <summary>
    /// 获取数字的序数形式。
    /// </summary>
    private static string GetOrdinalForm(int number)
    {
        // 简化的英语序数规则
        if (number % 100 >= 11 && number % 100 <= 13)
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
    /// 格式化性别形式（UE 风格）。
    /// </summary>
    /// <param name="gender">性别枚举。</param>
    /// <param name="forms">性别形式字典（例如：{"masculine": "Le", "feminine": "La"}）。</param>
    /// <returns>正确的性别形式。</returns>
    public static string Gender(TextGender gender, Dictionary<string, string> forms)
    {
        var genderKey = gender switch
        {
            TextGender.Masculine => "masculine",
            TextGender.Feminine => "feminine",
            TextGender.Neuter => "neuter",
            _ => "other"
        };

        return forms.TryGetValue(genderKey, out var result) ? result : "";
    }

    /// <summary>
    /// 格式化韩语后置词（UE 风格）。
    /// </summary>
    /// <param name="text">韩语文本。</param>
    /// <param name="consonantPostposition">以辅音结尾的后置词。</param>
    /// <param name="vowelPostposition">以元音结尾的后置词。</param>
    /// <returns>正确的后置词。</returns>
    public static string HangulPostposition(string text, string consonantPostposition, string vowelPostposition)
    {
        if (string.IsNullOrEmpty(text))
            return consonantPostposition;

        var lastChar = text[^1];
        // 韩文 Unicode 范围：AC00-D7AF
        // 简化判断：检查最后一个字符
        var isConsonant = (lastChar & 0x1F) != 0;

        return isConsonant ? consonantPostposition : vowelPostposition;
    }
}

/// <summary>
/// 文本性别枚举（UE 风格）。
/// </summary>
public enum TextGender
{
    /// <summary>
    /// 阳性。
    /// </summary>
    Masculine,

    /// <summary>
    /// 阴性。
    /// </summary>
    Feminine,

    /// <summary>
    /// 中性。
    /// </summary>
    Neuter
}
