using System.Globalization;
using SmartFormat;
using SmartFormat.Core.Settings;

namespace PrismaLocalization;

/// <summary>
/// 主本地化管理器，协调本地化提供程序和文本格式化。
/// </summary>
public class LocalizationManager : IDisposable
{
    private readonly SmartFormatter _formatter;
    private readonly List<ILocalizationProvider> _providers = [];
    private string _currentCulture;

    /// <summary>
    /// 获取 LocalizationManager 的单例实例。
    /// </summary>
    public static LocalizationManager Instance { get; } = new();

    /// <summary>
    /// 初始化 LocalizationManager 的新实例。
    /// </summary>
    public LocalizationManager()
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
        _formatter = Smart.CreateDefaultSmartFormat(settings);
        _currentCulture = CultureInfo.CurrentCulture.Name;
    }

    /// <summary>
    /// 获取或设置当前文化。
    /// </summary>
    public string CurrentCulture
    {
        get => _currentCulture;
        set
        {
            _currentCulture = value;
            CultureInfo.CurrentCulture = new CultureInfo(value);
            CultureInfo.CurrentUICulture = new CultureInfo(value);
        }
    }

    /// <summary>
    /// 向管理器添加本地化提供程序。
    /// </summary>
    public void AddProvider(ILocalizationProvider provider)
    {
        _providers.Add(provider);
    }

    /// <summary>
    /// 从管理器中移除本地化提供程序。
    /// </summary>
    public bool RemoveProvider(ILocalizationProvider provider)
    {
        return _providers.Remove(provider);
    }

    /// <summary>
    /// 获取指定键的本地化文本，支持可选参数。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="args">用于格式化模板的参数。</param>
    /// <returns>格式化后的本地化文本。</returns>
    public string GetText(LocalizationKey key, params object[] args)
    {
        return GetText(key, CurrentCulture, args);
    }

    /// <summary>
    /// 获取指定键和文化的本地化文本，支持可选参数。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="culture">文化名称。</param>
    /// <param name="args">用于格式化模板的参数。</param>
    /// <returns>格式化后的本地化文本。</returns>
    public string GetText(LocalizationKey key, string culture, params object[] args)
    {
        var template = GetTemplate(key, culture);
        return FormatText(template, args);
    }

    /// <summary>
    /// 获取指定键的本地化文本，支持命名参数。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="args">用于格式化模板的命名参数。</param>
    /// <returns>格式化后的本地化文本。</returns>
    public string GetText(LocalizationKey key, Dictionary<string, object?> args)
    {
        return GetText(key, CurrentCulture, args);
    }

    /// <summary>
    /// 获取指定键和文化的本地化文本，支持命名参数。
    /// </summary>
    /// <param name="key">本地化键。</param>
    /// <param name="culture">文化名称。</param>
    /// <param name="args">用于格式化模板的命名参数。</param>
    /// <returns>格式化后的本地化文本。</returns>
    public string GetText(LocalizationKey key, string culture, Dictionary<string, object?> args)
    {
        var template = GetTemplate(key, culture);
        return FormatText(template, args);
    }

    /// <summary>
    /// 获取不带格式化的文本模板。
    /// </summary>
    private string GetTemplate(LocalizationKey key, string culture)
    {
        foreach (var provider in _providers)
        {
            var template = provider.GetTemplate(key, culture);
            if (template != null)
                return template;
        }

        // 返回默认值或键作为回退
        return key.DefaultValue ?? key.FullKey;
    }

    /// <summary>
    /// 使用提供的参数格式化模板。
    /// </summary>
    private string FormatText(string template, params object[] args)
    {
        if (args.Length == 0)
            return template;

        try
        {
            return _formatter.Format(template, args);
        }
        catch
        {
            return string.Format(template, args);
        }
    }

    /// <summary>
    /// 使用提供的命名参数格式化模板。
    /// </summary>
    private string FormatText(string template, Dictionary<string, object?> args)
    {
        if (args.Count == 0)
            return template;

        try
        {
            return _formatter.Format(template, args);
        }
        catch
        {
            // 回退：手动替换占位符
            var result = template;
            foreach (var (key, value) in args)
            {
                result = result.Replace($"{{{key}}}", value?.ToString() ?? "");
            }
            return result;
        }
    }

    /// <summary>
    /// 检查指定键是否存在模板。
    /// </summary>
    public bool HasTemplate(LocalizationKey key, string? culture = null)
    {
        culture ??= CurrentCulture;
        return _providers.Any(p => p.HasTemplate(key, culture));
    }

    /// <summary>
    /// 从所有提供程序获取所有可用的文化。
    /// </summary>
    public IEnumerable<string> GetAvailableCultures()
    {
        return _providers
            .SelectMany(p => p.GetAvailableCultures())
            .Distinct()
            .OrderBy(c => c);
    }

    /// <summary>
    /// 重新加载所有提供程序。
    /// </summary>
    public void Reload()
    {
        foreach (var provider in _providers)
        {
            provider.Reload();
        }
    }

    public void Dispose()
    {
        if (this == Instance)
            return;

        // 释放资源（如果需要）
        GC.SuppressFinalize(this);
    }
}
