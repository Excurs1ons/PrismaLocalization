using System;
using System.Collections.Generic;

namespace PrismaLocalization
{
    /// <summary>
    /// 文本字符串解析器管理器。
    /// 用于注册和管理多个解析器。
    /// </summary>
    public class TextStringResolverManager
    {
        private readonly List<ITextStringResolver> _resolvers = new();

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
}