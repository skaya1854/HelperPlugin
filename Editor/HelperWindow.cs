#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Main Helper EditorWindow - Development tool for viewing detailed information
    /// about Unity projects. Extensible via IHelperExtension and HelperButtonAttribute.
    /// </summary>
    public partial class HelperWindow : EditorWindow
    {
        #region Constants

        private const string VERSION = "v2.0.0";
        private const float MIN_WIDTH = 650f;
        private const float MIN_HEIGHT = 500f;
        private const float HEADER_HEIGHT = 44f;
        private const float TAB_BAR_HEIGHT = 28f;
        private const float INFO_PANEL_HEIGHT = 70f;
        private const string PREFS_PREFIX = "Helper_Section_";

        #endregion

        #region State

        private Vector2 _contentScrollView;
        private Vector2 _tabScrollPosition;
        private bool _showInfoPanel = true;
        private double _lastUpdateTime;
        private const double UPDATE_INTERVAL = 0.5;

        // GameObject info cache
        private int _polyCount;
        private string _childCount = "0";
        private string _allChildCount = "0";

        // Tab management
        private int _selectedTabIndex;
        private List<TabInfo> _tabs = new List<TabInfo>();

        // Preview
        private GameObject _previewTarget;
        private Editor _gameObjectEditor;

        // Cached styles for header
        private static GUIStyle _headerTitleStyle;
        private static GUIStyle _headerVersionStyle;
        private static GUIStyle _objNameStyle;
        private static GUIStyle _objPathStyle;
        private static GUIStyle _tagLabelStyle;
        private static GUIStyle _layerLabelStyle;

        private static HelperWindow _window;

        #endregion

        #region Tab System

        private class TabInfo
        {
            public string Name;
            public Color Color;
            public int Order;
            public Action OnGUI;
            public Action OnSelectionChange;
            public Action OnEditorUpdate;
            public bool IsBuiltIn;
            public Func<int> GetBadgeCount;
        }

        #endregion

        #region Lifecycle

        [MenuItem("Window/Helper/Helper Window %#Z")]
        public static void ShowWindow()
        {
            if (_window != null)
            {
                _window.Close();
                return;
            }

            _window = GetWindow<HelperWindow>(false, "Helper");
            _window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            _window.InitializeTabs();
        }

        /// <summary>
        /// Open Helper Window programmatically
        /// </summary>
        public static HelperWindow Open()
        {
            _window = GetWindow<HelperWindow>(false, "Helper");
            _window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            _window.InitializeTabs();
            return _window;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Helper");
            EditorApplication.update += OnEditorUpdate;
            Selection.selectionChanged += OnSelectionChanged;
            InitializeTabs();
            UpdateGameObjectInfo();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            Selection.selectionChanged -= OnSelectionChanged;
            HelperUIComponents.ClearCachedStyles();
            ClearWindowStyles();

            if (_gameObjectEditor != null)
            {
                DestroyImmediate(_gameObjectEditor);
            }
        }

        private static void ClearWindowStyles()
        {
            _headerTitleStyle = null;
            _headerVersionStyle = null;
            _objNameStyle = null;
            _objPathStyle = null;
            _tagLabelStyle = null;
            _layerLabelStyle = null;
            ClearTabStyles();
        }

        private void InitializeTabs()
        {
            _tabs.Clear();

            // Built-in tabs
            _tabs.Add(new TabInfo
            {
                Name = "Test", Color = HelperTheme.ModeTest, Order = 0,
                OnGUI = DrawTestTab, IsBuiltIn = true,
                GetBadgeCount = () => HelperButtonRegistry.Buttons.Count
            });
            _tabs.Add(new TabInfo
            {
                Name = "Component", Color = HelperTheme.ModeComponent, Order = 1,
                OnGUI = DrawComponentTab, OnSelectionChange = UpdateGameObjectInfo, IsBuiltIn = true,
                GetBadgeCount = GetComponentBadgeCount
            });
            _tabs.Add(new TabInfo
            {
                Name = "Tag", Color = HelperTheme.ModeTag, Order = 2,
                OnGUI = DrawTagTab, IsBuiltIn = true
            });
            _tabs.Add(new TabInfo
            {
                Name = "Layer", Color = HelperTheme.ModeLayer, Order = 3,
                OnGUI = DrawLayerTab, IsBuiltIn = true
            });
            _tabs.Add(new TabInfo
            {
                Name = "Viewer", Color = HelperTheme.ModeViewer, Order = 4,
                OnGUI = DrawViewerTab, OnSelectionChange = ResetViewer, IsBuiltIn = true
            });
            _tabs.Add(new TabInfo
            {
                Name = "Debug", Color = HelperTheme.ModeDebug, Order = 5,
                OnGUI = DrawDebugTab, OnEditorUpdate = () => { }, IsBuiltIn = true
            });
            _tabs.Add(new TabInfo
            {
                Name = "Perf", Color = HelperTheme.ModePerformance, Order = 6,
                OnGUI = DrawPerformanceTab, OnEditorUpdate = () => { }, IsBuiltIn = true
            });

            // Add extension tabs
            foreach (var extension in HelperExtensionRegistry.Extensions)
            {
                _tabs.Add(new TabInfo
                {
                    Name = extension.TabName,
                    Color = extension.TabColor,
                    Order = extension.Order,
                    OnGUI = extension.OnGUI,
                    OnSelectionChange = extension.OnSelectionChange,
                    OnEditorUpdate = extension.OnEditorUpdate,
                    IsBuiltIn = false
                });
            }

            _tabs = _tabs.OrderBy(t => t.Order).ToList();
        }

        private int GetComponentBadgeCount()
        {
            if (Selection.activeGameObject == null) return 0;
            return Selection.activeGameObject.GetComponentsInChildren(typeof(Component), true)
                .Count(c => c != null);
        }

        private void OnEditorUpdate()
        {
            // Real-time update for Debug/Performance modes
            var currentTab = _tabs.ElementAtOrDefault(_selectedTabIndex);
            if (currentTab?.OnEditorUpdate != null)
            {
                if (EditorApplication.timeSinceStartup - _lastUpdateTime > UPDATE_INTERVAL)
                {
                    _lastUpdateTime = EditorApplication.timeSinceStartup;
                    Repaint();
                }
            }
        }

        private void OnSelectionChanged()
        {
            UpdateGameObjectInfo();
            _tabs.ElementAtOrDefault(_selectedTabIndex)?.OnSelectionChange?.Invoke();
            Repaint();
        }

        private void UpdateGameObjectInfo()
        {
            _polyCount = GetPoly();
            _childCount = GetChildCount("N0");
            _allChildCount = GetAllChildCount("N0");
        }

        #endregion

        #region Main GUI

        private void OnGUI()
        {
            var fullRect = new Rect(0, 0, position.width, position.height);
            EditorGUI.DrawRect(fullRect, HelperTheme.Surface0);

            DrawHeader();
            DrawTabBar();

            if (_showInfoPanel)
            {
                DrawGameObjectInfo();
            }

            DrawMainContent();
        }

        private void DrawHeader()
        {
            var headerRect = new Rect(0, 0, position.width, HEADER_HEIGHT);
            EditorGUI.DrawRect(headerRect, HelperTheme.Surface1);
            EditorGUI.DrawRect(new Rect(0, 0, 4, HEADER_HEIGHT), HelperTheme.Primary);

            GUILayout.BeginArea(new Rect(12, 0, position.width - 24, HEADER_HEIGHT));
            using (new GUILayout.HorizontalScope())
            {
                _headerTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = HelperTheme.TextHigh }
                };
                GUILayout.Label("Helper", _headerTitleStyle, GUILayout.Height(HEADER_HEIGHT));

                // Version badge
                _headerVersionStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = HelperTheme.TextLow }
                };
                GUILayout.Label(VERSION, _headerVersionStyle, GUILayout.Height(HEADER_HEIGHT));

                GUILayout.FlexibleSpace();

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        // Refresh button
                        if (HelperUIComponents.DrawButton("\u21BB Refresh", HelperTheme.Surface2, 80, 22))
                        {
                            HelperSceneCache.Refresh();
                            UpdateGameObjectInfo();
                            Repaint();
                        }

                        GUILayout.Space(4);

                        // Info toggle
                        if (HelperUIComponents.DrawButton(_showInfoPanel ? "Hide Info" : "Show Info", HelperTheme.Surface2, 80, 22))
                        {
                            _showInfoPanel = !_showInfoPanel;
                        }

                        GUILayout.Space(4);

                        // Settings placeholder
                        if (HelperUIComponents.DrawButton("\u2699", HelperTheme.Surface2, 26, 22))
                        {
                            // Future settings
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
            }
            GUILayout.EndArea();
        }

        private void DrawTabBar()
        {
            var tabRect = new Rect(0, HEADER_HEIGHT, position.width, TAB_BAR_HEIGHT);
            EditorGUI.DrawRect(tabRect, HelperTheme.Surface0);

            GUILayout.BeginArea(tabRect);

            // Horizontal scroll for tab overflow
            using (var scroll = new GUILayout.ScrollViewScope(_tabScrollPosition, false, false,
                GUIStyle.none, GUIStyle.none, GUIStyle.none))
            {
                _tabScrollPosition = scroll.scrollPosition;

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(4);

                    for (int i = 0; i < _tabs.Count; i++)
                    {
                        var tab = _tabs[i];
                        int badgeCount = tab.GetBadgeCount?.Invoke() ?? 0;
                        bool clicked;

                        if (badgeCount > 0)
                        {
                            clicked = HelperUIComponents.DrawModeTabWithBadge(
                                tab.Name, _selectedTabIndex == i, tab.Color, badgeCount, 75);
                        }
                        else
                        {
                            clicked = HelperUIComponents.DrawModeTab(
                                tab.Name, _selectedTabIndex == i, tab.Color, 60);
                        }

                        if (clicked)
                        {
                            _selectedTabIndex = i;
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
            }

            GUILayout.EndArea();
        }

        private void DrawGameObjectInfo()
        {
            float yStart = HEADER_HEIGHT + TAB_BAR_HEIGHT;
            var infoRect = new Rect(4, yStart + 2, position.width - 8, INFO_PANEL_HEIGHT - 4);

            EditorGUI.DrawRect(infoRect, HelperTheme.Surface1);
            EditorGUI.DrawRect(new Rect(infoRect.x, infoRect.y, 3, infoRect.height), HelperTheme.Primary);

            var activeGO = Selection.activeGameObject;
            string objName = activeGO != null ? activeGO.name : "No Selection";
            string objPath = activeGO != null ? GetGameObjectPath(activeGO) : "";

            GUILayout.BeginArea(new Rect(infoRect.x + 12, infoRect.y + 4, infoRect.width - 24, infoRect.height - 8));

            // First row: name + path
            using (new GUILayout.HorizontalScope())
            {
                _objNameStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
                _objNameStyle.normal.textColor = activeGO != null ? HelperTheme.Accent : HelperTheme.TextLow;
                GUILayout.Label(objName, _objNameStyle, GUILayout.MaxWidth(200));

                if (activeGO != null && !string.IsNullOrEmpty(objPath))
                {
                    _objPathStyle ??= new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = HelperTheme.TextLow }
                    };
                    GUILayout.Label(objPath, _objPathStyle);
                }

                GUILayout.FlexibleSpace();
            }

            // Second row: stats + tag/layer
            using (new GUILayout.HorizontalScope())
            {
                HelperUIComponents.DrawStatBadge("\u25B3 Tri", _polyCount.ToString("N0"), HelperTheme.Secondary, 65, 28);
                GUILayout.Space(4);
                HelperUIComponents.DrawStatBadge("\u229E Child", _childCount, HelperTheme.ModeTag, 65, 28);
                GUILayout.Space(4);
                HelperUIComponents.DrawStatBadge("\u229E All", _allChildCount, HelperTheme.ModeViewer, 60, 28);

                GUILayout.FlexibleSpace();

                if (activeGO != null)
                {
                    _tagLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = HelperTheme.TextMedium }
                    };

                    GUILayout.Label("Tag:", _tagLabelStyle);
                    activeGO.tag = EditorGUILayout.TagField(activeGO.tag, GUILayout.Width(80));

                    GUILayout.Space(4);

                    _layerLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = HelperTheme.TextMedium }
                    };

                    GUILayout.Label("Layer:", _layerLabelStyle);
                    activeGO.layer = EditorGUILayout.LayerField(activeGO.layer, GUILayout.Width(80));
                }
            }

            GUILayout.EndArea();
        }

        private void DrawMainContent()
        {
            float yStart = HEADER_HEIGHT + TAB_BAR_HEIGHT + (_showInfoPanel ? INFO_PANEL_HEIGHT : 0);
            float contentHeight = position.height - yStart;

            var contentRect = new Rect(0, yStart, position.width, contentHeight);

            GUILayout.BeginArea(contentRect);

            using (var scroll = new GUILayout.ScrollViewScope(_contentScrollView))
            {
                _contentScrollView = scroll.scrollPosition;
                GUILayout.Space(8);

                var currentTab = _tabs.ElementAtOrDefault(_selectedTabIndex);
                currentTab?.OnGUI?.Invoke();

                GUILayout.Space(8);
            }

            GUILayout.EndArea();
        }

        #endregion

        #region Section State Persistence

        private readonly Dictionary<string, bool> _sectionStates = new Dictionary<string, bool>();

        private bool GetSectionState(string tabName, string sectionName, bool defaultExpanded = true)
        {
            string key = $"{PREFS_PREFIX}{tabName}_{sectionName}";

            if (!_sectionStates.TryGetValue(key, out bool state))
            {
                state = EditorPrefs.GetBool(key, defaultExpanded);
                _sectionStates[key] = state;
            }

            return state;
        }

        private void SetSectionState(string tabName, string sectionName, bool expanded)
        {
            string key = $"{PREFS_PREFIX}{tabName}_{sectionName}";
            _sectionStates[key] = expanded;
            EditorPrefs.SetBool(key, expanded);
        }

        /// <summary>
        /// Draw a collapsible section with persisted state
        /// </summary>
        private void DrawPersistedSection(string tabName, string sectionName, Color accentColor, Action drawContent)
        {
            bool expanded = GetSectionState(tabName, sectionName);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                using (new GUILayout.VerticalScope())
                {
                    bool newExpanded = expanded;
                    HelperUIComponents.DrawCollapsibleSection(sectionName, ref newExpanded, accentColor, drawContent);

                    if (newExpanded != expanded)
                    {
                        SetSectionState(tabName, sectionName, newExpanded);
                    }
                }

                GUILayout.Space(8);
            }
            GUILayout.Space(4);
        }

        #endregion

        #region Utility Methods

        private int GetPoly()
        {
            if (Selection.activeGameObject == null) return 0;
            int totalTris = 0;
            foreach (var mf in Selection.activeGameObject.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                // Use GetIndexCount per sub-mesh to avoid GC from mesh.triangles
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    totalTris += (int)mesh.GetIndexCount(sub) / 3;
            }
            return totalTris;
        }

        private string GetAllChildCount(string format = "")
        {
            if (Selection.activeGameObject == null) return "0";
            var count = Selection.activeGameObject.GetComponentsInChildren<Transform>().Length - 1;
            return string.IsNullOrEmpty(format) ? count.ToString() : count.ToString(format);
        }

        private string GetChildCount(string format = "")
        {
            if (Selection.activeGameObject == null) return "0";
            var count = Selection.activeGameObject.transform.childCount;
            return string.IsNullOrEmpty(format) ? count.ToString() : count.ToString(format);
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;
            return str.Substring(0, maxLength - 3) + "...";
        }

        private static string GetGameObjectPath(GameObject go)
        {
            if (go == null) return "";

            var path = new StringBuilder();
            var current = go.transform.parent;

            while (current != null)
            {
                if (path.Length > 0) path.Insert(0, "/");
                path.Insert(0, current.name);
                current = current.parent;
            }

            return path.Length > 0 ? path.ToString() + "/" : "";
        }

        private List<List<Component>> GetComponents(GameObject target, out bool hasNull)
        {
            var components = new List<List<Component>>();
            var childs = target.GetComponentsInChildren(typeof(Component), true);
            hasNull = false;

            foreach (var item in childs)
            {
                if (item != null)
                {
                    var existing = components.FirstOrDefault(c => c.Count > 0 && c[0].GetType() == item.GetType());
                    if (existing != null)
                        existing.Add(item);
                    else
                        components.Add(new List<Component> { item });
                }
                else hasNull = true;
            }

            return components;
        }

        private static readonly string[] DefaultTags = { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };

        private List<string> GetAllTags()
        {
            var tags = DefaultTags.ToList();
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
            var prop = tagManager.FindProperty("tags");

            for (int i = 0; i < prop.arraySize; i++)
            {
                var tag = prop.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(tag) && !tags.Contains(tag))
                    tags.Add(tag);
            }

            return tags;
        }

        private List<string> GetAllLayers()
        {
            var layers = new List<string>();
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
            var prop = tagManager.FindProperty("layers");

            for (int i = 0; i < prop.arraySize; i++)
            {
                var layer = prop.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(layer))
                    layers.Add(layer);
            }

            return layers;
        }

        /// <summary>
        /// Collect all debug info into a single string for clipboard copy
        /// </summary>
        private string CollectAllDebugInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Helper Debug Info ===");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // FPS
            var fpsData = HelperSceneCache.FPS;
            sb.AppendLine("[Frame Rate]");
            sb.AppendLine($"Current FPS: {fpsData.CurrentFPS:F1}");
            sb.AppendLine($"Average FPS: {fpsData.AverageFPS:F1}");
            sb.AppendLine($"Min/Max: {fpsData.MinFPS:F0} / {fpsData.MaxFPS:F0}");
            sb.AppendLine($"Frame Time: {fpsData.FrameTime:F2} ms");
            sb.AppendLine();

            // Application
            sb.AppendLine("[Application]");
            sb.AppendLine($"Play Mode: {(Application.isPlaying ? "Playing" : "Stopped")}");
            sb.AppendLine($"Time Scale: {Time.timeScale:F2}");
            sb.AppendLine($"Frame Count: {Time.frameCount:N0}");
            sb.AppendLine();

            // Memory
            var totalMem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            var monoMem = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            var gfxMem = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);
            sb.AppendLine("[Memory]");
            sb.AppendLine($"Total Allocated: {totalMem:F1} MB");
            sb.AppendLine($"Mono Used: {monoMem:F1} MB");
            sb.AppendLine($"Graphics: {gfxMem:F1} MB");
            sb.AppendLine();

            // Scene
            var scene = HelperSceneCache.Scene;
            sb.AppendLine("[Scene]");
            sb.AppendLine($"Total Objects: {scene.TotalObjects:N0}");
            sb.AppendLine($"Active: {scene.ActiveObjects:N0}");
            sb.AppendLine($"Inactive: {scene.InactiveObjects:N0}");
            sb.AppendLine($"MonoBehaviours: {scene.MonoBehaviours:N0}");
            sb.AppendLine();

            // System
            sb.AppendLine("[System]");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"VRAM: {SystemInfo.graphicsMemorySize / 1024f:F1} GB");
            sb.AppendLine($"RAM: {SystemInfo.systemMemorySize / 1024f:F1} GB");

            return sb.ToString();
        }

        /// <summary>
        /// Collect all performance info into a single string for clipboard copy
        /// </summary>
        private string CollectAllPerfInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Helper Performance Info ===");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var geo = HelperSceneCache.Geometry;
            sb.AppendLine("[Geometry]");
            sb.AppendLine($"Total Triangles: {geo.TotalTriangles:N0}");
            sb.AppendLine($"Total Vertices: {geo.TotalVertices:N0}");
            sb.AppendLine($"Mesh Filters: {geo.MeshFilterCount:N0}");
            sb.AppendLine($"Skinned Meshes: {geo.SkinnedMeshCount:N0}");
            sb.AppendLine();

            var render = HelperSceneCache.Rendering;
            sb.AppendLine("[Rendering]");
            sb.AppendLine($"Cameras: {render.CameraCount}");
            sb.AppendLine($"Renderers: {render.RendererCount:N0}");
            sb.AppendLine($"Lights: {render.LightCount:N0}");
            sb.AppendLine($"Unique Materials: {render.UniqueMaterialCount:N0}");
            sb.AppendLine($"Unique Shaders: {render.UniqueShaderCount:N0}");
            sb.AppendLine();

            var physics = HelperSceneCache.Physics;
            sb.AppendLine("[Physics]");
            sb.AppendLine($"Rigidbodies: {physics.RigidbodyCount:N0}");
            sb.AppendLine($"Colliders: {physics.ColliderCount:N0}");
            sb.AppendLine();

            var ui = HelperSceneCache.UI;
            sb.AppendLine("[UI]");
            sb.AppendLine($"Canvases: {ui.CanvasCount}");
            sb.AppendLine($"TMP Texts: {ui.TMPTextCount:N0}");
            sb.AppendLine($"Images: {ui.ImageCount:N0}");
            sb.AppendLine($"Buttons: {ui.ButtonCount:N0}");

            return sb.ToString();
        }

        #endregion
    }
}
#endif
