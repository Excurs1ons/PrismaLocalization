#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.UI;

namespace PrismaLocalization.Unity
{
    /// <summary>
    /// Unity 文本组件的本地化扩展。
    /// </summary>
    public static class UnityTextExtensions
    {
        /// <summary>
        /// 设置 Text 组件的本地化文本。
        /// </summary>
        /// <param name="text">Text 组件。</param>
        /// <param name="key">本地化键。</param>
        /// <param name="args">格式化参数。</param>
        public static void SetLocalizedText(this Text text, LocalizationKey key, params object[] args)
        {
            if (text == null) return;
            text.text = LocalizationManager.Instance.GetText(key, args);
        }

        /// <summary>
        /// 设置 Text 组件的本地化文本（指定文化）。
        /// </summary>
        /// <param name="text">Text 组件。</param>
        /// <param name="key">本地化键。</param>
        /// <param name="culture">文化代码。</param>
        /// <param name="args">格式化参数。</param>
        public static void SetLocalizedText(this Text text, LocalizationKey key, string culture, params object[] args)
        {
            if (text == null) return;
            text.text = LocalizationManager.Instance.GetText(key, culture, args);
        }

        /// <summary>
        /// 设置 TextMeshPro 组件的本地化文本。
        /// </summary>
        /// <param name="text">TextMeshPro 组件。</param>
        /// <param name="key">本地化键。</param>
        /// <param name="args">格式化参数。</param>
        public static void SetLocalizedText(this TMPro.TMP_Text text, LocalizationKey key, params object[] args)
        {
            if (text == null) return;
            text.text = LocalizationManager.Instance.GetText(key, args);
        }

        /// <summary>
        /// 设置 TextMeshPro 组件的本地化文本（指定文化）。
        /// </summary>
        /// <param name="text">TextMeshPro 组件。</param>
        /// <param name="key">本地化键。</param>
        /// <param name="culture">文化代码。</param>
        /// <param name="args">格式化参数。</param>
        public static void SetLocalizedText(this TMPro.TMP_Text text, LocalizationKey key, string culture, params object[] args)
        {
            if (text == null) return;
            text.text = LocalizationManager.Instance.GetText(key, culture, args);
        }
    }

    /// <summary>
    /// Unity 对象的文本字符串解析器。
    /// 用于从 Unity 对象中提取本地化相关的字符串。
    /// </summary>
    public class UnityObjectTextResolver : ITextStringResolver
    {
        /// <summary>
        /// 尝试从 Unity 对象中解析本地化字符串。
        /// </summary>
        public bool TryResolve(object? obj, out string? result)
        {
            result = obj switch
            {
                Object unityObj => GetTextFromUnityObject(unityObj),
                _ => null
            };
            return result != null;
        }

        private static string? GetTextFromUnityObject(Object obj)
        {
            return obj switch
            {
                GameObject go => go.name,
                Component component => GetTextFromUnityObject(component.gameObject),
                Text text => text.text,
                TMPro.TMP_Text tmpText => tmpText.text,
                _ => obj.ToString()
            };
        }
    }

    /// <summary>
    /// 本地化文本组件，自动更新显示的本地化文本。
    /// </summary>
    [AddComponentMenu("Localization/Localized Text")]
    [HelpURL("https://github.com/your-repo/PrismaLocalization")]
    public class LocalizedTextComponent : MonoBehaviour
    {
        [Header("Localization Settings")]
        [SerializeField]
        private string _namespace = "";

        [SerializeField]
        private string _key = "";

        [SerializeField]
        private string _defaultValue = "";

        [SerializeField]
        private LocalizationCategory _category = LocalizationCategory.General;

        [Header("References")]
        [SerializeField]
        private Text _targetText;

        [SerializeField]
        private TMPro.TMP_Text _targetTextMeshPro;

        [Header("Format Arguments")]
        [SerializeField]
        private string[] _argumentKeys = Array.Empty<string>();

        private LocalizationKey _localizationKey;
        private bool _isInitialized = false;

        /// <summary>
        /// 获取或设置本地化键。
        /// </summary>
        public LocalizationKey LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                _namespace = value.Namespace;
                _key = value.Key;
                _defaultValue = value.DefaultValue ?? "";
                _category = value.Category;
                UpdateText();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            UpdateText();
        }

        private void Initialize()
        {
            _localizationKey = new LocalizationKey(_namespace, _key, _category, _defaultValue);

            // 自动查找 Text 组件
            if (_targetText == null && _targetTextMeshPro == null)
            {
                _targetText = GetComponent<Text>();
                _targetTextMeshPro = GetComponent<TMPro.TMP_Text>();
            }

            _isInitialized = true;
        }

        /// <summary>
        /// 更新显示的文本。
        /// </summary>
        /// <param name="args">格式化参数。</param>
        public void UpdateText(params object[] args)
        {
            var text = LocalizationManager.Instance.GetText(_localizationKey, args);
            SetText(text);
        }

        private void SetText(string text)
        {
            if (_targetTextMeshPro != null)
            {
                _targetTextMeshPro.text = text;
            }
            else if (_targetText != null)
            {
                _targetText.text = text;
            }
        }

        /// <summary>
        /// 当文化改变时调用此方法以更新文本。
        /// </summary>
        public void OnCultureChanged()
        {
            UpdateText();
        }

        /// <summary>
        /// 使用命名参数更新文本。
        /// </summary>
        public void UpdateTextNamed(Dictionary<string, object?> args)
        {
            var text = LocalizationManager.Instance.GetText(_localizationKey, args);
            SetText(text);
        }
    }

    /// <summary>
    /// Unity 资源本地化提供程序。
    /// 从 Unity 的 Resources 或 Addressables 加载本地化数据。
    /// </summary>
    public class UnityResourceLocalizationProvider : ILocalizationProvider
    {
        private readonly string _resourcesPath;
        private readonly Dictionary<string, Dictionary<string, string>> _loadedData = new();

        /// <summary>
        /// 初始化 UnityResourceLocalizationProvider 的新实例。
        /// </summary>
        /// <param name="resourcesPath">Resources 文件夹中的路径。</param>
        public UnityResourceLocalizationProvider(string resourcesPath = "Localization")
        {
            _resourcesPath = resourcesPath;
        }

        /// <summary>
        /// 加载指定文化的本地化数据。
        /// </summary>
        /// <param name="culture">文化代码。</param>
        public void LoadCulture(string culture)
        {
            var assetPath = $"{_resourcesPath}/localization.{culture}";
            var textAsset = Resources.Load<TextAsset>(assetPath);

            if (textAsset != null)
            {
                ParseAndStoreData(culture, textAsset.text);
            }
        }

        /// <summary>
        /// 加载所有可用的本地化数据。
        /// </summary>
        public void LoadAllCultures()
        {
            // 加载 JSON 文件
            var textAssets = Resources.LoadAll<TextAsset>(_resourcesPath);
            foreach (var asset in textAssets)
            {
                if (asset.name.StartsWith("localization."))
                {
                    var culture = asset.name.Replace("localization.", "");
                    ParseAndStoreData(culture, asset.text);
                }
            }
        }

        private void ParseAndStoreData(string culture, string jsonData)
        {
            try
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                if (data != null)
                {
                    _loadedData[culture] = data;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse localization data for culture '{culture}': {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定本地化键的文本模板。
        /// </summary>
        public string? GetTemplate(LocalizationKey key, string? culture = null)
        {
            culture ??= Application.systemLanguage.ToString();

            if (_loadedData.TryGetValue(culture, out var cultureData))
            {
                if (cultureData.TryGetValue(key.FullKey, out var text))
                    return text;
            }

            return key.DefaultValue;
        }

        /// <summary>
        /// 检查指定键和文化是否存在模板。
        /// </summary>
        public bool HasTemplate(LocalizationKey key, string? culture = null)
        {
            culture ??= Application.systemLanguage.ToString();

            if (_loadedData.TryGetValue(culture, out var cultureData))
            {
                return cultureData.ContainsKey(key.FullKey);
            }
            return false;
        }

        /// <summary>
        /// 获取所有可用的文化。
        /// </summary>
        public IEnumerable<string> GetAvailableCultures()
        {
            return _loadedData.Keys;
        }

        /// <summary>
        /// 重新加载本地化数据。
        /// </summary>
        public void Reload()
        {
            _loadedData.Clear();
            LoadAllCultures();
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        public void Unload()
        {
            _loadedData.Clear();
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// Unity 本地化管理器。
    /// 提供与 Unity 生命周期集成的本地化功能。
    /// </summary>
    public class UnityLocalizationManager : MonoBehaviour
    {
        private static UnityLocalizationManager _instance;
        private string _currentCulture;

        /// <summary>
        /// 获取单例实例。
        /// </summary>
        public static UnityLocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("PrismaLocalizationManager");
                    _instance = go.AddComponent<UnityLocalizationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 获取或设置当前文化。
        /// </summary>
        public string CurrentCulture
        {
            get => _currentCulture ??= Application.systemLanguage.ToString();
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    LocalizationManager.Instance.CurrentCulture = value;
                    NotifyCultureChanged();
                }
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化默认文化
            _currentCulture = Application.systemLanguage.ToString();
            LocalizationManager.Instance.CurrentCulture = _currentCulture;
        }

        /// <summary>
        /// 当文化改变时通知所有监听器。
        /// </summary>
        private void NotifyCultureChanged()
        {
            var components = FindObjectsOfType<LocalizedTextComponent>();
            foreach (var component in components)
            {
                component.OnCultureChanged();
            }
        }

        /// <summary>
        /// 设置本地化文化。
        /// </summary>
        /// <param name="culture">文化代码（例如 "zh-CN", "en-US"）。</param>
        public void SetCulture(string culture)
        {
            CurrentCulture = culture;
        }

        /// <summary>
        /// 使用系统语言。
        /// </summary>
        public void UseSystemLanguage()
        {
            CurrentCulture = Application.systemLanguage.ToString();
        }
    }
}
#endif
