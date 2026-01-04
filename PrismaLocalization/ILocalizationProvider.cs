namespace PrismaLocalization;

/// <summary>
/// 用于提供本地化文本模板的接口。
/// </summary>
public interface ILocalizationProvider
{
    /// <summary>
    /// 获取指定本地化键的文本模板。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="culture">文化名称（例如 "en-US"、"zh-CN"）。</param>
    /// <returns>文本模板，如果未找到则返回 null。</returns>
    string? GetTemplate(LocalizationKey key, string? culture = null);

    /// <summary>
    /// 检查指定键和文化是否存在模板。
    /// </summary>
    bool HasTemplate(LocalizationKey key, string? culture = null);

    /// <summary>
    /// 获取所有可用的文化。
    /// </summary>
    IEnumerable<string> GetAvailableCultures();

    /// <summary>
    /// 重新加载本地化数据。
    /// </summary>
    void Reload();
}
