using System.Diagnostics;

namespace PrismaLocalization;

/// <summary>
/// 表示强类型的本地化键。
/// 灵感来源于 Unreal Engine 的 FText 本地化系统。
/// </summary>
/// <param name="Namespace">本地化键的命名空间。</param>
/// <param name="Key">唯一的键标识符。</param>
/// <param name="Category">本地化条目的分类。</param>
/// <param name="Variant">此键的语法变体。</param>
/// <param name="DefaultValue">默认（回退）文本模板。</param>
public readonly record struct LocalizationKey(
    string Namespace,
    string Key,
    LocalizationCategory Category = LocalizationCategory.General,
    LocalizationVariant Variant = LocalizationVariant.None,
    string? DefaultValue = null)
{
    /// <summary>
    /// 创建一个带有空命名空间的本地化键。
    /// </summary>
    public LocalizationKey(string key, LocalizationCategory category = LocalizationCategory.General, LocalizationVariant variant = LocalizationVariant.None, string? defaultValue = null)
        : this(string.Empty, key, category, variant, defaultValue)
    {
    }

    /// <summary>
    /// 返回格式为 "Namespace:Key" 的完整键路径。
    /// </summary>
    public string FullKey => string.IsNullOrEmpty(Namespace)
        ? Key
        : $"{Namespace}:{Key}";

    /// <summary>
    /// 返回用于存储的变体键（包含变体后缀）。
    /// </summary>
    public string VariantKey => Variant == LocalizationVariant.None
        ? FullKey
        : $"{FullKey}{Variant.ToSuffix()}";

    /// <summary>
    /// 创建带有指定变体的新 LocalizationKey。
    /// </summary>
    public LocalizationKey WithVariant(LocalizationVariant variant) =>
        new LocalizationKey(Namespace, Key, Category, variant, DefaultValue);

    /// <summary>
    /// 创建带有指定分类的新 LocalizationKey。
    /// </summary>
    public LocalizationKey WithCategory(LocalizationCategory category) =>
        new LocalizationKey(Namespace, Key, category, Variant, DefaultValue);

    /// <summary>
    /// 从字符串隐式转换。
    /// </summary>
    public static implicit operator LocalizationKey(string key) =>
        new LocalizationKey(key);

    /// <summary>
    /// 从格式为 "Namespace:Key" 的字符串创建本地化键。
    /// </summary>
    public static LocalizationKey Parse(string fullKey)
    {
        var parts = fullKey.Split(':', 2);
        return parts.Length == 2
            ? new LocalizationKey(parts[0], parts[1])
            : new LocalizationKey(fullKey);
    }

    [DebuggerStepThrough]
    public override string ToString() => FullKey;
}
