using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace PrismaLocalization;

/// <summary>
/// UE 风格的字符串表，用于集中管理本地化文本。
/// </summary>
public class StringTable
{
    private readonly ConcurrentDictionary<string, LocalizedString> _entries = new();
    private readonly string _tableId;

    /// <summary>
    /// 获取字符串表的标识符。
    /// </summary>
    public string TableId => _tableId;

    /// <summary>
    /// 获取字符串表的命名空间。
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// 初始化 StringTable 的新实例。
    /// </summary>
    /// <param name="tableId">表的唯一标识符。</param>
    /// <param name="ns">命名空间（可选）。</param>
    public StringTable(string tableId, string? ns = null)
    {
        _tableId = tableId;
        Namespace = ns ?? tableId;
    }

    /// <summary>
    /// 添加或更新字符串表条目。
    /// </summary>
    /// <param name="key">键。</param>
    /// <param name="localizedString">本地化字符串。</param>
    public void SetEntry(string key, LocalizedString localizedString)
    {
        _entries.AddOrUpdate(key, localizedString, (_, _) => localizedString);
    }

    /// <summary>
    /// 添加或更新字符串表条目。
    /// </summary>
    /// <param name="key">键。</param>
    /// <param name="localizationKey">本地化键。</param>
    public void SetEntry(string key, LocalizationKey localizationKey)
    {
        _entries.AddOrUpdate(key, new LocalizedString(localizationKey), (_, _) => new LocalizedString(localizationKey));
    }

    /// <summary>
    /// 获取指定键的本地化字符串。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns>本地化字符串，如果未找到则返回 null。</returns>
    public LocalizedString? GetEntry(string key)
    {
        return _entries.TryGetValue(key, out var entry) ? entry : null;
    }

    /// <summary>
    /// 检查字符串表是否包含指定键。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns>如果包含则返回 true，否则返回 false。</returns>
    public bool HasEntry(string key)
    {
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// 移除指定键的条目。
    /// </summary>
    /// <param name="key">键。</param>
    /// <returns>如果成功移除则返回 true，否则返回 false。</returns>
    public bool RemoveEntry(string key)
    {
        return _entries.TryRemove(key, out _);
    }

    /// <summary>
    /// 获取所有键。
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        return _entries.Keys.OrderBy(k => k);
    }

    /// <summary>
    /// 从 JSON 文件加载字符串表。
    /// </summary>
    /// <param name="filePath">JSON 文件路径。</param>
    /// <returns>加载的字符串表。</returns>
    public static StringTable FromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return FromJson(json, Path.GetFileNameWithoutExtension(filePath));
    }

    /// <summary>
    /// 从 JSON 字符串加载字符串表。
    /// </summary>
    /// <param name="json">JSON 字符串。</param>
    /// <param name="tableId">表标识符。</param>
    /// <returns>加载的字符串表。</returns>
    public static StringTable FromJson(string json, string tableId)
    {
        var data = JsonConvert.DeserializeObject<LocalizationData>(json)
            ?? new LocalizationData { Culture = tableId };

        var table = new StringTable(tableId);

        foreach (var entry in data.Entries)
        {
            var key = new LocalizationKey(
                entry.Namespace ?? table.Namespace,
                entry.Key,
                Enum.TryParse<LocalizationCategory>(entry.Category, out var cat) ? cat : LocalizationCategory.General,
                LocalizationVariant.None,
                entry.Text
            );
            table.SetEntry(entry.Key, new LocalizedString(key));
        }

        return table;
    }

    /// <summary>
    /// 将字符串表保存为 JSON 文件。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    public void ToJsonFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 将字符串表转换为 JSON 字符串。
    /// </summary>
    public string ToJson()
    {
        var data = new LocalizationData
        {
            Culture = _tableId,
            Entries = _entries.Select kvp =>
            {
                var key = kvp.Value.Key;
                return LocalizationEntry.FromKey(key, key.DefaultValue ?? key.Key);
            }).ToList()
        };

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    /// <summary>
    /// 清除所有条目。
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }

    /// <summary>
    /// 获取条目数量。
    /// </summary>
    public int Count => _entries.Count;
}

/// <summary>
/// 字符串表集合管理器。
/// </summary>
public class StringTableManager
{
    private readonly ConcurrentDictionary<string, StringTable> _tables = new();

    /// <summary>
    /// 获取默认实例。
    /// </summary>
    public static StringTableManager Instance { get; } = new();

    /// <summary>
    /// 注册字符串表。
    /// </summary>
    /// <param name="table">字符串表。</param>
    public void RegisterTable(StringTable table)
    {
        _tables.TryAdd(table.TableId, table);
    }

    /// <summary>
    /// 注销字符串表。
    /// </summary>
    /// <param name="tableId">表标识符。</param>
    /// <returns>如果成功注销则返回 true，否则返回 false。</returns>
    public bool UnregisterTable(string tableId)
    {
        return _tables.TryRemove(tableId, out _);
    }

    /// <summary>
    /// 获取指定表的本地化字符串。
    /// </summary>
    /// <param name="tableId">表标识符。</param>
    /// <param name="key">键。</param>
    /// <returns>本地化字符串，如果未找到则返回 null。</returns>
    public LocalizedString? GetString(string tableId, string key)
    {
        return _tables.TryGetValue(tableId, out var table)
            ? table.GetEntry(key)
            : null;
    }

    /// <summary>
    /// 检查是否存在指定的表。
    /// </summary>
    /// <param name="tableId">表标识符。</param>
    /// <returns>如果存在则返回 true，否则返回 false。</returns>
    public bool HasTable(string tableId)
    {
        return _tables.ContainsKey(tableId);
    }

    /// <summary>
    /// 获取所有已注册的表 ID。
    /// </summary>
    public IEnumerable<string> GetTableIds()
    {
        return _tables.Keys.OrderBy(id => id);
    }
}
