using System.Globalization;
using SmartFormat;

namespace PrismaLocalization;

/// <summary>
/// UE 风格的文本生成器。
/// 用于生成数值、日期时间等文化相关的文本。
/// </summary>
public static class TextGenerator
{
    /// <summary>
    /// 将数字转换为用户友好的文本表示。
    /// </summary>
    /// <param name="value">数值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <param name="useGrouping">是否使用千分位分隔符。</param>
    /// <returns>格式化后的文本。</returns>
    public static string AsNumber(double value, string? culture = null, bool useGrouping = true)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return value.ToString(useGrouping ? "N" : "F", cultureInfo);
    }

    /// <summary>
    /// 将整数转换为用户友好的文本表示。
    /// </summary>
    /// <param name="value">整数值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <param name="useGrouping">是否使用千分位分隔符。</param>
    /// <returns>格式化后的文本。</returns>
    public static string AsNumber(int value, string? culture = null, bool useGrouping = true)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return value.ToString(useGrouping ? "N0" : "F0", cultureInfo);
    }

    /// <summary>
    /// 将数字转换为百分比文本表示。
    /// </summary>
    /// <param name="value">0-1 之间的数值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>格式化后的百分比文本。</returns>
    public static string AsPercent(double value, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return value.ToString("P0", cultureInfo);
    }

    /// <summary>
    /// 将字节数转换为用户友好的内存表示。
    /// </summary>
    /// <param name="bytes">字节数。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>格式化后的内存文本（例如 "1.2 KiB"）。</returns>
    public static string AsMemory(long bytes, string? culture = null)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;
        const long TB = GB * 1024;

        string format;
        double value;

        if (bytes >= TB)
        {
            value = (double)bytes / TB;
            format = "{0:F2} TiB";
        }
        else if (bytes >= GB)
        {
            value = (double)bytes / GB;
            format = "{0:F2} GiB";
        }
        else if (bytes >= MB)
        {
            value = (double)bytes / MB;
            format = "{0:F2} MiB";
        }
        else if (bytes >= KB)
        {
            value = (double)bytes / KB;
            format = "{0:F2} KiB";
        }
        else
        {
            value = bytes;
            format = "{0} B";
        }

        return string.Format(CultureInfo.InvariantCulture, format, value);
    }

    /// <summary>
    /// 将数值转换为货币文本表示。
    /// </summary>
    /// <param name="value">数值。</param>
    /// <param name="currencyCode">货币代码（例如 "USD", "CNY"）。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>格式化后的货币文本。</returns>
    public static string AsCurrency(double value, string currencyCode, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        try
        {
            var regionInfo = new RegionInfo(cultureInfo.Name);
            return value.ToString("C", cultureInfo);
        }
        catch
        {
            return $"{value:F2} {currencyCode}";
        }
    }

    /// <summary>
    /// 将日期转换为用户友好的文本表示。
    /// </summary>
    /// <param name="date">日期值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <param name="format">日期格式（"short", "long", "full"）。</param>
    /// <returns>格式化后的日期文本。</returns>
    public static string AsDate(DateTime date, string? culture = null, string format = "short")
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        var dateFormat = format switch
        {
            "short" => cultureInfo.DateTimeFormat.ShortDatePattern,
            "long" => cultureInfo.DateTimeFormat.LongDatePattern,
            "full" => "D",
            _ => cultureInfo.DateTimeFormat.ShortDatePattern
        };

        return date.ToString(dateFormat, cultureInfo);
    }

    /// <summary>
    /// 将时间转换为用户友好的文本表示。
    /// </summary>
    /// <param name="time">时间值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <param name="format">时间格式（"short", "long"）。</param>
    /// <returns>格式化后的时间文本。</returns>
    public static string AsTime(DateTime time, string? culture = null, string format = "short")
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        var timeFormat = format switch
        {
            "short" => cultureInfo.DateTimeFormat.ShortTimePattern,
            "long" => cultureInfo.DateTimeFormat.LongTimePattern,
            _ => cultureInfo.DateTimeFormat.ShortTimePattern
        };

        return time.ToString(timeFormat, cultureInfo);
    }

    /// <summary>
    /// 将日期时间转换为用户友好的文本表示。
    /// </summary>
    /// <param name="dateTime">日期时间值。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>格式化后的日期时间文本。</returns>
    public static string AsDateTime(DateTime dateTime, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return dateTime.ToString(cultureInfo);
    }

    /// <summary>
    /// 将时间跨度转换为用户友好的文本表示。
    /// </summary>
    /// <param name="timeSpan">时间跨度。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>格式化后的时间跨度文本。</returns>
    public static string AsTimeSpan(TimeSpan timeSpan, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        if (timeSpan.TotalDays >= 1)
            return $"{timeSpan.Days:F0} {GetResourceKey("Time.Days", cultureInfo)}";
        if (timeSpan.TotalHours >= 1)
            return $"{timeSpan.Hours:F0} {GetResourceKey("Time.Hours", cultureInfo)}";
        if (timeSpan.TotalMinutes >= 1)
            return $"{timeSpan.Minutes:F0} {GetResourceKey("Time.Minutes", cultureInfo)}";
        return $"{timeSpan.Seconds:F0} {GetResourceKey("Time.Seconds", cultureInfo)}";
    }

    private static string GetResourceKey(string key, CultureInfo culture)
    {
        // 简化实现，实际应从本地化资源获取
        return key switch
        {
            "Time.Days" => "days",
            "Time.Hours" => "hours",
            "Time.Minutes" => "minutes",
            "Time.Seconds" => "seconds",
            _ => key
        };
    }

    /// <summary>
    /// 将文本转换为小写。
    /// </summary>
    /// <param name="text">输入文本。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>小写文本。</returns>
    public static string ToLower(string text, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return text.ToLower(cultureInfo);
    }

    /// <summary>
    /// 将文本转换为大写。
    /// </summary>
    /// <param name="text">输入文本。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>大写文本。</returns>
    public static string ToUpper(string text, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        return text.ToUpper(cultureInfo);
    }

    /// <summary>
    /// 将文本转换为首字母大写。
    /// </summary>
    /// <param name="text">输入文本。</param>
    /// <param name="culture">文化代码（可选）。</param>
    /// <returns>首字母大写的文本。</returns>
    public static string ToTitleCase(string text, string? culture = null)
    {
        var cultureInfo = string.IsNullOrEmpty(culture)
            ? CultureInfo.CurrentCulture
            : new CultureInfo(culture);

        var textInfo = cultureInfo.TextInfo;
        return textInfo.ToTitleCase(text);
    }
}
