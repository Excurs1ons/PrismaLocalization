using System;
using System.Collections.Generic;
using System.Linq;

#if PRISMA_USE_SMARTFORMAT
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;
#endif

namespace PrismaLocalization;

/// <summary>
/// UE 风格的文本格式化工具。
/// 支持复数、性别等参数修饰符。
/// </summary>
public static class TextFormat
{
#if PRISMA_USE_SMARTFORMAT
    private static readonly SmartFormatter _formatter = CreateFormatter();

    /// <summary>
    /// 创建配置好的 SmartFormatter。
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
        return formatter;
    }
#endif

    /// <summary>
    /// 格式化带有索引参数的文本。
    /// </summary>
    public static string Format(string format, params object[] args)
    {
        if (args.Length == 0)
            return format;

#if PRISMA_USE_SMARTFORMAT
        try
        {
            return _formatter.Format(format, args);
        }
        catch
        {
            return string.Format(format, args);
        }
#else
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
#endif
    }

    /// <summary>
    /// 格式化带有命名参数的文本。
    /// </summary>
    public static string FormatNamed(string format, Dictionary<string, object> args)
    {
        if (args.Count == 0)
            return format;

#if PRISMA_USE_SMARTFORMAT
        try
        {
            return _formatter.Format(format, args);
        }
        catch
#endif
        {
            var result = format;
            foreach (var kv in args)
            {
                result = result.Replace($"{{{kv.Key}}}", kv.Value?.ToString() ?? "");
            }
            return result;
        }
    }

    /// <summary>
    /// 格式化复数形式（UE 风格）。
    /// </summary>
    public static string Plural(int count, Dictionary<string, string> forms)
    {
        var pluralForm = GetPluralForm(count);
        return forms.TryGetValue(pluralForm, out var result)
            ? result
            : forms.TryGetValue("other", out result) ? result : "";
    }

    private static string GetPluralForm(int count)
    {
        if (count == 0) return "zero";
        if (count == 1) return "one";
        if (count == 2) return "two";
        return "other";
    }

    /// <summary>
    /// 格式化序数形式（UE 风格）。
    /// </summary>
    public static string Ordinal(int number, Dictionary<string, string> forms)
    {
        var ordinalForm = GetOrdinalForm(number);
        return forms.TryGetValue(ordinalForm, out var result)
            ? result
            : forms.TryGetValue("other", out result) ? result : "";
    }

    private static string GetOrdinalForm(int number)
    {
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
    public static string HangulPostposition(string text, string consonantPostposition, string vowelPostposition)
    {
        if (string.IsNullOrEmpty(text))
            return consonantPostposition;

        var lastChar = text[^1];
        var isConsonant = (lastChar & 0x1F) != 0;

        return isConsonant ? consonantPostposition : vowelPostposition;
    }
}

/// <summary>
/// 文本性别枚举（UE 风格）。
/// </summary>
public enum TextGender
{
    Masculine,
    Feminine,
    Neuter
}
