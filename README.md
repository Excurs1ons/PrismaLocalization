# PrismaLocalization

<div align="center">

![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-.NET%208-brightgreen)
![Unity](https://img.shields.io/badge/Unity-2020.3%2B-black)

**一个强大的 C# 本地化组件库，灵感来源于 Unreal Engine 的 FText 系统**

[功能特性](#功能特性) • [快速开始](#快速开始) • [API 文档](#api-文档) • [示例](#示例)

</div>

---

## 简介

PrismaLocalization 是一个功能完整的本地化解决方案，旨在实现类似 Unreal Engine 的强大文本模板系统。它支持多种本地化特性，包括文本分类、语法变体、ICU 消息格式等，并且可以脱离 Unity 独立使用，也可以完美集成到 Unity 项目中。

### 核心特性

- **文本分类系统** - 支持名词、动词、代词、UI 文本等多种分类
- **语法变体支持** - 支持名词单复数、人称变格、时态变体等语法变体
- **ICU 消息格式** - 完整支持 ICU MessageFormat 语法（复数、选择、序数形式）
- **文本生成器** - 数值、日期时间、货币等文化相关的文本生成
- **JSON 数据格式** - 使用 JSON 存储本地化数据，易于编辑和版本控制
- **字符串表** - 集中管理本地化文本，支持运行时加载
- **Unity 集成** - 提供 Unity 特定的组件和工具（通过条件编译）
- **运行时热更** - 支持运行时添加和更新本地化数据（Polyglot Data）

---

## 功能特性

### 文本分类

支持多种预定义的文本分类，便于组织和管理翻译：

| 分类 | 描述 |
|------|------|
| `General` | 通用文本 |
| `Noun` | 名词 |
| `Verb` | 动词 |
| `Pronoun` | 代词（人称代词） |
| `Number` | 数字 |
| `UI` | UI 标签和界面文本 |
| `Error` | 错误消息和警告 |
| `Dialog` | 对话文本 |
| `Menu` | 菜单和导航文本 |

### 语法变体

支持多种语法变体，覆盖不同语言的需求：

```csharp
// 名词变体
LocalizationVariant.Singular    // 单数
LocalizationVariant.Plural      // 复数

// 代词变格
LocalizationVariant.Nominative      // 主格（I, he, they）
LocalizationVariant.Accusative      // 宾格（me, him, them）
LocalizationVariant.Genitive        // 所有格（mine, his, theirs）

// 性别变体
LocalizationVariant.Masculine   // 阳性
LocalizationVariant.Feminine    // 阴性

// 时态变体
LocalizationVariant.Present     // 现在时
LocalizationVariant.Past        // 过去时
```

### ICU 消息格式

完整支持 ICU MessageFormat 语法：

```csharp
// 复数形式
"{count, plural, one{# cat} other{# cats}}"

// 选择形式
"{gender, select, male{He} female{She} other{They}}"

// 序数形式
"{place, selectordinal, one{#st} two{#nd} few{#rd} other{#th}}"
```

---

## 快速开始

### 安装

将 `PrismaLocalization.csproj` 添加到你的解决方案中，或直接复制源代码到你的项目。

```bash
# 通过 NuGet 安装依赖
dotnet add package SmartFormat
dotnet add package Newtonsoft.Json
```

### 基本使用

```csharp
using PrismaLocalization;

// 1. 初始化本地化管理器
var manager = LocalizationManager.Instance;

// 2. 添加本地化提供程序
var provider = new JsonLocalizationProvider("./Localization");
manager.AddProvider(provider);

// 3. 设置当前文化
manager.CurrentCulture = "zh-CN";

// 4. 获取本地化文本
var key = new LocalizationKey("Common", "HelloWorld", defaultValue: "你好，世界！");
var text = manager.GetText(key);
Console.WriteLine(text); // 输出: 你好，世界！
```

### 使用参数格式化

```csharp
// 使用位置参数
var key = new LocalizationKey("UI", "ItemCount");
var text = manager.GetText(key, 5);
Console.WriteLine(text); // 输出: 你有 5 个物品。

// 使用命名参数
var args = new Dictionary<string, object?>
{
    ["player"] = "张三",
    ["score"] = 100
};
var text2 = manager.GetText(key, args);
```

### 使用 ICU 格式化

```csharp
// 复数形式
var pattern = "{count, plural, one{You have # item} other{You have # items}}";
var result = pattern.FormatICU(new Dictionary<string, object?> { ["count"] = 5 });
// 输出: You have 5 items

// 选择形式
var genderPattern = "{gender, select, male{He is} female{She is} other{They are}} here";
var result2 = genderPattern.FormatICU(new Dictionary<string, object?> { ["gender"] = "female" });
// 输出: She is here
```

---

## API 文档

### LocalizationKey

表示本地化键的强类型结构：

```csharp
// 创建本地化键
var key1 = new LocalizationKey("MyNamespace", "MyKey");
var key2 = new LocalizationKey("MyKey"); // 无命名空间
var key3 = "MyKey"; // 隐式转换

// 带分类和变体
var key4 = new LocalizationKey(
    "MyNamespace",
    "MyKey",
    LocalizationCategory.Noun,
    LocalizationVariant.Plural
);

// 获取完整键路径
string fullKey = key.FullKey;        // "MyNamespace:MyKey"
string variantKey = key.VariantKey;  // "MyNamespace:MyKey_pl"
```

### LocalizationManager

主本地化管理器：

```csharp
// 设置文化
LocalizationManager.Instance.CurrentCulture = "zh-CN";

// 获取文本
string text = LocalizationManager.Instance.GetText(key);
string text2 = LocalizationManager.Instance.GetText(key, "en-US");
string text3 = LocalizationManager.Instance.GetText(key, arg1, arg2);

// 检查是否存在模板
bool exists = LocalizationManager.Instance.HasTemplate(key);

// 获取所有可用文化
var cultures = LocalizationManager.Instance.GetAvailableCultures();
```

### 文本生成器

```csharp
// 数值格式化
string number = TextGenerator.AsNumber(1234.5, "zh-CN");  // "1,234.5"

// 百分比
string percent = TextGenerator.AsPercent(0.85, "zh-CN");  // "85%"

// 货币
string currency = TextGenerator.AsCurrency(1234.50, "CNY", "zh-CN");  // "¥1,234.50"

// 日期
string date = TextGenerator.AsDate(DateTime.Now, "zh-CN", "long");

// 大小写转换
string lower = TextGenerator.ToLower("HELLO", "zh-CN");
```

### 字符串表

```csharp
// 创建字符串表
var table = new StringTable("GameUI", "Game");

// 添加条目
table.SetEntry("StartButton", new LocalizedString("Menu_Start"));

// 获取条目
var localizedString = table.GetEntry("StartButton");
string text = localizedString.ToString();

// 从 JSON 加载
var table2 = StringTable.FromJson("localization.zh-CN.json");

// 保存为 JSON
table.ToJsonFile("localization.zh-CN.json");
```

---

## Unity 集成

PrismaLocalization 提供了完整的 Unity 支持（通过 `#if UNITY_5_3_OR_NEWER` 条件编译）：

### LocalizedTextComponent

自动更新本地化文本的 Unity 组件：

```csharp
// 在 Inspector 中配置
[RequireComponent(typeof(Text))]
public class MyUI : MonoBehaviour
{
    public LocalizedTextComponent localizedText;

    void Start()
    {
        // 更新文本
        localizedText.UpdateText(arg1, arg2);
    }
}
```

### Unity 扩展方法

```csharp
// 设置 Text 组件的本地化文本
myTextComponent.SetLocalizedText(key, args);

// 设置 TextMeshPro 组件的本地化文本
myTmpTextComponent.SetLocalizedText(key, args);
```

### Unity 本地化管理器

```csharp
// 获取 Unity 本地化管理器
var manager = UnityLocalizationManager.Instance;

// 设置文化
manager.SetCulture("zh-CN");

// 使用系统语言
manager.UseSystemLanguage();
```

---

## JSON 数据格式

本地化数据使用 JSON 格式存储：

```json
{
  "culture": "zh-CN",
  "entries": [
    {
      "key": "HelloWorld",
      "namespace": "Common",
      "category": "General",
      "text": "你好，世界！",
      "context": "欢迎问候语",
      "comment": "显示在主界面"
    }
  ]
}
```

### 支持变体的 JSON 格式

```json
{
  "culture": "en-US",
  "entries": [
    {
      "key": "Item",
      "namespace": "UI",
      "category": "Noun",
      "text": "item",
      "variants": {
        "plural": "items"
      }
    }
  ]
}
```

---

## 示例

### 完整的使用示例

```csharp
using PrismaLocalization;

class Program
{
    static void Main()
    {
        // 初始化
        var manager = LocalizationManager.Instance;
        var provider = new JsonLocalizationProvider("./Examples/Localization");
        manager.AddProvider(provider);
        manager.CurrentCulture = "zh-CN";

        // 简单文本
        var helloKey = new LocalizationKey("Common", "HelloWorld");
        Console.WriteLine(manager.GetText(helloKey));

        // 带参数的文本
        var itemCountKey = new LocalizationKey("UI", "ItemCount");
        Console.WriteLine(manager.GetText(itemCountKey, 10));

        // 使用字符串表
        var table = StringTable.FromJson("./Examples/Localization/localization.zh-CN.json");
        StringTableManager.Instance.RegisterTable(table);

        var menuText = StringTableManager.Instance.GetString("localization.zh-CN", "Menu_Start");
        Console.WriteLine(menuText?.ToString());

        // ICU 格式化
        var pluralPattern = "{count, plural, one{# item} other{# items}}";
        Console.WriteLine(pluralPattern.FormatICU(new Dictionary<string, object?> { ["count"] = 3 }));
    }
}
```

---

## 项目结构

```
PrismaLocalization/
├── PrismaLocalization/
│   ├── LocalizationKey.cs          # 本地化键
│   ├── LocalizationCategory.cs     # 文本分类枚举
│   ├── LocalizationVariant.cs      # 语法变体枚举
│   ├── LocalizationManager.cs      # 本地化管理器
│   ├── LocalizedString.cs          # 本地化字符串
│   ├── ILocalizationProvider.cs    # 提供程序接口
│   ├── JsonLocalizationProvider.cs # JSON 提供程序
│   ├── LocalizationEntry.cs        # JSON 数据模型
│   ├── ICUFormatter.cs             # ICU 格式化器
│   ├── TextFormat.cs               # 文本格式化工具
│   ├── TextGenerator.cs            # 文本生成器
│   ├── StringTable.cs              # 字符串表
│   ├── ITextStringResolver.cs      # 对象文本解析接口
│   └── Unity/                      # Unity 支持
│       └── UnityTextResolver.cs
├── Examples/
│   └── Localization/
│       ├── localization.zh-CN.json
│       ├── localization.en-US.json
│       └── localization.ja-JP.json
└── README.md
```

---

## 依赖

- **SmartFormat** (>= 3.6.0) - 智能字符串格式化
- **Newtonsoft.Json** (>= 13.0.3) - JSON 序列化

---

## 许可证

MIT License

---

## 贡献

欢迎提交 Issue 和 Pull Request！

---

## 参考

- [Unreal Engine Text Localization](https://dev.epicgames.com/documentation/en-us/unreal-engine/text-localization-in-unreal-engine)
- [ICU MessageFormat](http://icu-project.org/apiref/icu4c/classicu_1_1MessageFormat.html)
- [Unicode CLDR](https://cldr.unicode.org/)

---

**Sources:**
- [Text Localization in Unreal Engine](https://dev.epicgames.com/documentation/en-us/unreal-engine/text-localization-in-unreal-engine)
