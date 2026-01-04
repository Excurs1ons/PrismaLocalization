namespace PrismaLocalization;

/// <summary>
/// 表示可解析为翻译文本的可本地化字符串。
/// 类似于 Unreal Engine 的 FText。
/// </summary>
public sealed class LocalizedString
{
    private readonly LocalizationKey _key;
    private readonly object[] _args;

    /// <summary>
    /// 初始化 LocalizedString 的新实例。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="args">用于格式化的可选参数。</param>
    public LocalizedString(LocalizationKey key, params object[] args)
    {
        _key = key;
        _args = args;
    }

    /// <summary>
    /// 使用字符串键初始化新实例。
    /// </summary>
    /// <param name="key">本地化键字符串。</param>
    /// <param name="args">用于格式化的可选参数。</param>
    public LocalizedString(string key, params object[] args)
        : this(new LocalizationKey(key), args)
    {
    }

    /// <summary>
    /// 获取本地化键。
    /// </summary>
    public LocalizationKey Key => _key;

    /// <summary>
    /// 解析并返回翻译后的文本。
    /// </summary>
    /// <param name="culture">可选的文化覆盖。</param>
    /// <returns>翻译并格式化后的文本。</returns>
    public string ToString(string? culture)
    {
        return string.IsNullOrEmpty(culture)
            ? LocalizationManager.Instance.GetText(_key, _args)
            : LocalizationManager.Instance.GetText(_key, culture, _args);
    }

    /// <summary>
    /// 使用当前文化解析并返回翻译后的文本。
    /// </summary>
    public override string ToString()
    {
        return ToString(null);
    }

    /// <summary>
    /// 从字符串隐式转换。
    /// </summary>
    public static implicit operator LocalizedString(string key) =>
        new LocalizedString(key);

    /// <summary>
    /// 转换为字符串。
    /// </summary>
    public static implicit operator string(LocalizedString localizedString) =>
        localizedString.ToString();
}
