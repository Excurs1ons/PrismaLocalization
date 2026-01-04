using System.Collections.Concurrent;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrismaLocalization;

/// <summary>
/// 基于 JSON 的本地化提供程序。
/// 支持分层 JSON 结构来组织翻译数据。
/// </summary>
public class JsonLocalizationProvider : ILocalizationProvider
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _translations = new();
    private readonly string _baseDirectory;
    private readonly string _filePattern;

    /// <summary>
    /// 初始化 JsonLocalizationProvider 的新实例。
    /// </summary>
    /// <param name="baseDirectory">包含本地化文件的基础目录。</param>
    /// <param name="filePattern">文件模式（例如 "localization.{culture}.json"）。</param>
    public JsonLocalizationProvider(string baseDirectory, string filePattern = "localization.{culture}.json")
    {
        _baseDirectory = baseDirectory;
        _filePattern = filePattern;
        LoadAllCultures();
    }

    /// <summary>
    /// 使用预加载的 JSON 数据初始化新实例。
    /// </summary>
    /// <param name="translationsData">文化名称到 JSON 翻译数据的映射字典。</param>
    public JsonLocalizationProvider(Dictionary<string, string> translationsData)
    {
        foreach (var (culture, json) in translationsData)
        {
            LoadCultureFromJson(culture, json);
        }
    }

    /// <summary>
    /// 从基础目录加载所有本地化文件。
    /// </summary>
    private void LoadAllCultures()
    {
        if (!Directory.Exists(_baseDirectory))
            return;

        var pattern = _filePattern.Replace("{culture}", "*");
        foreach (var file in Directory.GetFiles(_baseDirectory, pattern))
        {
            var culture = Path.GetFileNameWithoutExtension(file)
                .Replace(_filePattern.Replace("{culture}", "").Replace(".json", ""), "")
                .Replace("localization.", "");

            try
            {
                var json = File.ReadAllText(file);
                LoadCultureFromJson(culture, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载本地化文件 '{file}' 失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 从 JSON 字符串加载指定文化的翻译数据。
    /// </summary>
    private void LoadCultureFromJson(string culture, string json)
    {
        var token = JToken.Parse(json);
        var cultureDict = _translations.GetOrAdd(culture, _ => new ConcurrentDictionary<string, string>());

        if (token is JObject obj)
        {
            FlattenJsonObject(obj, "", cultureDict);
        }
        else if (token is LocalizationData data)
        {
            // 支持 LocalizationEntry 格式
            foreach (var entry in data.Entries)
            {
                var key = new LocalizationKey(
                    entry.Namespace ?? "",
                    entry.Key,
                    Enum.TryParse<LocalizationCategory>(entry.Category, out var cat) ? cat : LocalizationCategory.General
                );
                cultureDict.TryAdd(key.VariantKey, entry.Text);
            }
        }
    }

    /// <summary>
    /// 将嵌套的 JSON 对象展平为点号分隔的键。
    /// </summary>
    private void FlattenJsonObject(JObject obj, string prefix, ConcurrentDictionary<string, string> target)
    {
        foreach (var property in obj.Properties())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            switch (property.Value)
            {
                case JObject nestedObj:
                    FlattenJsonObject(nestedObj, key, target);
                    break;
                case JValue value:
                    if (value.Type == JTokenType.String)
                    {
                        target.TryAdd(key, value.ToString());
                    }
                    break;
                case JArray arr:
                    target.TryAdd(key, arr.ToString(Formatting.None));
                    break;
            }
        }
    }

    /// <summary>
    /// 获取指定本地化键的文本模板。
    /// </summary>
    public string? GetTemplate(LocalizationKey key, string? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture.Name;

        // 首先尝试带变体后缀的键
        var variantKey = key.VariantKey;

        // 尝试特定文化（例如 "zh-CN"）
        if (_translations.TryGetValue(culture, out var cultureDict))
        {
            if (cultureDict.TryGetValue(variantKey, out var template))
                return template;

            // 回退到不带变体的键
            if (key.Variant != LocalizationVariant.None)
            {
                if (cultureDict.TryGetValue(key.FullKey, out template))
                    return template;
            }

            // 尝试不带命名空间的键
            if (!string.IsNullOrEmpty(key.Namespace) && cultureDict.TryGetValue(key.Key, out template))
                return template;
        }

        // 尝试父文化（例如从 "zh-CN" 回退到 "zh"）
        var parentCulture = culture.Contains('-') ? culture.Split('-')[0] : null;
        if (parentCulture != null && _translations.TryGetValue(parentCulture, out cultureDict))
        {
            if (cultureDict.TryGetValue(variantKey, out var template))
                return template;

            if (key.Variant != LocalizationVariant.None)
            {
                if (cultureDict.TryGetValue(key.FullKey, out template))
                    return template;
            }

            if (!string.IsNullOrEmpty(key.Namespace) && cultureDict.TryGetValue(key.Key, out template))
                return template;
        }

        // 返回默认值（如果可用）
        return key.DefaultValue;
    }

    /// <summary>
    /// 检查指定键和文化是否存在模板。
    /// </summary>
    public bool HasTemplate(LocalizationKey key, string? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture.Name;
        var variantKey = key.VariantKey;

        if (_translations.TryGetValue(culture, out var cultureDict))
        {
            if (cultureDict.ContainsKey(variantKey))
                return true;

            if (!string.IsNullOrEmpty(key.Namespace) && cultureDict.ContainsKey(key.Key))
                return true;
        }

        var parentCulture = culture.Contains('-') ? culture.Split('-')[0] : null;
        if (parentCulture != null && _translations.TryGetValue(parentCulture, out cultureDict))
        {
            if (cultureDict.ContainsKey(variantKey))
                return true;

            if (!string.IsNullOrEmpty(key.Namespace) && cultureDict.ContainsKey(key.Key))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 获取所有可用的文化。
    /// </summary>
    public IEnumerable<string> GetAvailableCultures() => _translations.Keys;

    /// <summary>
    /// 重新加载本地化数据。
    /// </summary>
    public void Reload()
    {
        _translations.Clear();
        LoadAllCultures();
    }

    /// <summary>
    /// 以编程方式添加或更新翻译。
    /// </summary>
    public void SetTranslation(LocalizationKey key, string culture, string template)
    {
        var cultureDict = _translations.GetOrAdd(culture, _ => new ConcurrentDictionary<string, string>());
        cultureDict.AddOrUpdate(key.VariantKey, template, (_, _) => template);
    }

    /// <summary>
    /// 获取指定文化的所有翻译键。
    /// </summary>
    public IEnumerable<string> GetKeys(string culture)
    {
        return _translations.TryGetValue(culture, out var cultureDict)
            ? cultureDict.Keys
            : Enumerable.Empty<string>();
    }
}
