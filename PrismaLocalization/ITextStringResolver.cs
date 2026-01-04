namespace PrismaLocalization;

/// <summary>
/// 从对象解析本地化字符串的接口。
/// 用于在具体业务逻辑中实现自定义的对象到本地化文本的转换。
/// </summary>
public interface ITextStringResolver
{
    /// <summary>
    /// 尝试从对象中解析本地化字符串。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="result">解析结果。</param>
    /// <returns>如果解析成功返回 true，否则返回 false。</returns>
    bool TryResolve(object? obj, out string? result);

    /// <summary>
    /// 从对象中解析本地化字符串，如果失败则返回默认值。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>解析的字符串或默认值。</returns>
    string? Resolve(object? obj, string? defaultValue = null)
    {
        return TryResolve(obj, out var result) ? result : defaultValue;
    }
}

/// <summary>
/// 泛型版本的文本字符串解析器。
/// </summary>
/// <typeparam name="T">要解析的对象类型。</typeparam>
public interface ITextStringResolver<in T> : ITextStringResolver
{
    /// <summary>
    /// 尝试从指定类型的对象中解析本地化字符串。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="result">解析结果。</param>
    /// <returns>如果解析成功返回 true，否则返回 false。</returns>
    bool TryResolve(T obj, out string? result);
}

/// <summary>
/// 基于委托的文本字符串解析器。
/// </summary>
public class DelegateTextStringResolver : ITextStringResolver
{
    private readonly Func<object?, string?> _resolver;

    /// <summary>
    /// 初始化 DelegateTextStringResolver 的新实例。
    /// </summary>
    /// <param name="resolver">解析委托。</param>
    public DelegateTextStringResolver(Func<object?, string?> resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// 尝试从对象中解析本地化字符串。
    /// </summary>
    public bool TryResolve(object? obj, out string? result)
    {
        result = _resolver(obj);
        return result != null;
    }
}

/// <summary>
/// 泛型版本的基于委托的文本字符串解析器。
/// </summary>
/// <typeparam name="T">要解析的对象类型。</typeparam>
public class DelegateTextStringResolver<T> : ITextStringResolver<T>
{
    private readonly Func<T, string?> _resolver;

    /// <summary>
    /// 初始化 DelegateTextStringResolver 的新实例。
    /// </summary>
    /// <param name="resolver">解析委托。</param>
    public DelegateTextStringResolver(Func<T, string?> resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// 尝试从指定类型的对象中解析本地化字符串。
    /// </summary>
    public bool TryResolve(T obj, out string? result)
    {
        result = _resolver(obj);
        return result != null;
    }

    /// <summary>
    /// 尝试从对象中解析本地化字符串。
    /// </summary>
    public bool TryResolve(object? obj, out string? result)
    {
        if (obj is T typedObj)
        {
            return TryResolve(typedObj, out result);
        }
        result = null;
        return false;
    }
}

/// <summary>
/// 文本字符串解析器管理器。
/// 用于注册和管理多个解析器。
/// </summary>
public class TextStringResolverManager
{
    private readonly List<ITextStringResolver> _resolvers = [];

    /// <summary>
    /// 获取默认实例。
    /// </summary>
    public static TextStringResolverManager Instance { get; } = new();

    /// <summary>
    /// 注册解析器。
    /// </summary>
    /// <param name="resolver">要注册的解析器。</param>
    public void RegisterResolver(ITextStringResolver resolver)
    {
        _resolvers.Add(resolver);
    }

    /// <summary>
    /// 注册泛型解析器。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="resolver">解析委托。</param>
    public void RegisterResolver<T>(Func<T, string?> resolver)
    {
        _resolvers.Add(new DelegateTextStringResolver<T>(resolver));
    }

    /// <summary>
    /// 尝试从对象中解析本地化字符串。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="result">解析结果。</param>
    /// <returns>如果任何解析器成功解析返回 true，否则返回 false。</returns>
    public bool TryResolve(object? obj, out string? result)
    {
        foreach (var resolver in _resolvers)
        {
            if (resolver.TryResolve(obj, out result))
                return true;
        }

        result = null;
        return false;
    }

    /// <summary>
    /// 从对象中解析本地化字符串，如果失败则返回默认值。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>解析的字符串或默认值。</returns>
    public string? Resolve(object? obj, string? defaultValue = null)
    {
        return TryResolve(obj, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// 移除所有已注册的解析器。
    /// </summary>
    public void Clear()
    {
        _resolvers.Clear();
    }

    /// <summary>
    /// 获取已注册的解析器数量。
    /// </summary>
    public int Count => _resolvers.Count;
}

/// <summary>
/// 文本字符串解析器的扩展方法。
/// </summary>
public static class TextStringResolverExtensions
{
    /// <summary>
    /// 使用解析器管理器从对象中获取本地化字符串。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>解析的字符串或默认值。</returns>
    public static string? ResolveLocalizedString(this object? obj, string? defaultValue = null)
    {
        return TextStringResolverManager.Instance.Resolve(obj, defaultValue);
    }

    /// <summary>
    /// 使用指定的解析器从对象中获取本地化字符串。
    /// </summary>
    /// <param name="obj">要解析的对象。</param>
    /// <param name="resolver">解析器。</param>
    /// <param name="defaultValue">默认值。</param>
    /// <returns>解析的字符串或默认值。</returns>
    public static string? ResolveLocalizedString(this object? obj, ITextStringResolver resolver, string? defaultValue = null)
    {
        return resolver.Resolve(obj, defaultValue);
    }
}
