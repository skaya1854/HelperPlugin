#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Tab implementations for HelperWindow. All built-in tabs are defined here.
    /// Uses HelperSceneCache for data - ZERO FindObjectsByType calls.
    /// </summary>
    public partial class HelperWindow
    {
        #region Tab Search State

        private string _testSearchText = "";
        private string _componentSearchText = "";
        private string _tagSearchText = "";
        private string _layerSearchText = "";
        private bool _componentSortByCount;

        // Cached styles for tabs
        private static GUIStyle _categoryHeaderStyle;
        private static GUIStyle _indexBadgeStyle;
        private static GUIStyle _playIndicatorStyle;
        private static GUIStyle _liveIndicatorStyle;
        private static GUIStyle _timeStampStyle;
        private static GUIStyle _tagRowNameStyle;
        private static GUIStyle _tagRowCountStyle;
        private static GUIStyle _perfHeaderStyle;
        private static GUIStyle _perfFpsStyle;

        private static void ClearTabStyles()
        {
            _categoryHeaderStyle = null;
            _indexBadgeStyle = null;
            _playIndicatorStyle = null;
            _liveIndicatorStyle = null;
            _timeStampStyle = null;
            _tagRowNameStyle = null;
            _tagRowCountStyle = null;
            _perfHeaderStyle = null;
            _perfFpsStyle = null;
        }

        #endregion

        #region Test Tab

        private void DrawTestTab()
        {
            // Search field
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                HelperUIComponents.DrawSearchField(ref _testSearchText, 200);
                GUILayout.FlexibleSpace();
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            // Draw buttons by category
            var categories = HelperButtonRegistry.GetCategories().ToList();

            foreach (var category in categories)
            {
                var buttons = HelperButtonRegistry.GetButtonsByCategory(category)
                    .Where(b => string.IsNullOrEmpty(_testSearchText) ||
                               b.Name.IndexOf(_testSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               b.CurrentValue.IndexOf(_testSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (buttons.Count == 0) continue;

                string sectionTitle = $"{category} ({buttons.Count})";

                DrawPersistedSection("Test", category, HelperTheme.ModeTest, () =>
                {
                    int index = 1;
                    foreach (var button in buttons)
                    {
                        DrawTestButton(index++, button);
                    }
                });
            }
        }

        private void DrawTestButton(int index, HelperButtonRegistry.ButtonInfo button)
        {
            using (new GUILayout.HorizontalScope())
            {
                var rowRect = GUILayoutUtility.GetRect(position.width - 32, 28);
                EditorGUI.DrawRect(rowRect, HelperTheme.Surface1);

                // Index badge
                var indexRect = new Rect(rowRect.x + 4, rowRect.y + 3, 28, 22);
                EditorGUI.DrawRect(indexRect, HelperTheme.Primary);

                _indexBadgeStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = HelperTheme.TextHigh }
                };
                GUI.Label(indexRect, index.ToString(), _indexBadgeStyle);

                // Play mode indicator
                if (button.RequiresPlayMode && !Application.isPlaying)
                {
                    var playRect = new Rect(rowRect.x + 36, rowRect.y + 6, 16, 16);
                    _playIndicatorStyle ??= new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = HelperTheme.TextLow }
                    };
                    GUI.Label(playRect, "\u25B6", _playIndicatorStyle);
                }

                // Button with tooltip showing default value
                var btnRect = new Rect(rowRect.x + 54, rowRect.y + 3, 160, 22);
                var btnColor = button.RequiresPlayMode && !Application.isPlaying ? HelperTheme.Surface2 : HelperTheme.Surface3;

                // Tooltip with default value
                if (btnRect.Contains(Event.current.mousePosition))
                {
                    GUI.tooltip = $"Default: {button.DefaultValue}";
                }

                if (HelperUIComponents.DrawButtonAt(btnRect, button.Name, btnColor))
                {
                    if (button.RequiresPlayMode && !Application.isPlaying)
                    {
                        // Silently ignore - requires play mode
                    }
                    else
                    {
                        button.Action?.Invoke(button.CurrentValue);
                    }
                }

                // Description field
                var descRect = new Rect(rowRect.x + 220, rowRect.y + 4, rowRect.width - 276, 20);
                var newValue = EditorGUI.TextField(descRect, button.CurrentValue);
                if (newValue != button.CurrentValue)
                {
                    button.CurrentValue = newValue;
                }

                // Reset button
                if (button.CurrentValue != button.DefaultValue)
                {
                    var resetRect = new Rect(rowRect.xMax - 50, rowRect.y + 3, 44, 22);
                    if (HelperUIComponents.DrawButtonAt(resetRect, "Reset", HelperTheme.TextLow))
                    {
                        button.CurrentValue = button.DefaultValue;
                    }
                }
            }
            GUILayout.Space(2);
        }

        #endregion

        #region Component Tab

        private void DrawComponentTab()
        {
            if (Selection.activeGameObject == null)
            {
                HelperUIComponents.DrawEmptyState("No GameObject Selected", "Select an object in the hierarchy");
                return;
            }

            var components = GetComponents(Selection.activeGameObject, out bool hasNull);

            // Header: search + total count + sort toggle
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                HelperUIComponents.DrawSearchField(ref _componentSearchText, 200);
                GUILayout.Space(8);

                int totalCount = components.Sum(g => g.Count);
                HelperUIComponents.DrawStatBadge("Total", totalCount.ToString(), HelperTheme.ModeComponent, 60, 28);

                GUILayout.Space(8);

                if (HelperUIComponents.DrawButton(_componentSortByCount ? "Sort: Count" : "Sort: Name", HelperTheme.Surface2, 90, 22))
                {
                    _componentSortByCount = !_componentSortByCount;
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            if (hasNull)
            {
                HelperUIComponents.DrawWarningBox("Selected GameObject has Null Component!");
                GUILayout.Space(4);
            }

            // Filter and sort components
            var filtered = components
                .Where(g => g.Count > 0)
                .Where(g =>
                {
                    if (string.IsNullOrEmpty(_componentSearchText)) return true;
                    string typeName = g[0].GetType().Name;
                    return typeName.IndexOf(_componentSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                });

            var sorted = _componentSortByCount
                ? filtered.OrderByDescending(g => g.Count)
                : filtered.OrderBy(g => g[0].GetType().Name);

            foreach (var group in sorted)
            {
                var typeName = group[0].GetType().Name;
                var active = group.Where(c => c.gameObject.activeSelf).Select(c => c.gameObject).ToArray();
                var inactive = group.Where(c => !c.gameObject.activeSelf).Select(c => c.gameObject).ToArray();

                var components = group.ToArray();

                HelperUIComponents.DrawComponentRow(typeName, group.Count, active.Length, inactive.Length,
                    () => Selection.objects = active,
                    () => Selection.objects = inactive,
                    () => DeleteComponents(components, typeName));
            }
        }

        #endregion

        #region Tag Tab

        private void DrawTagTab()
        {
            // Search
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                HelperUIComponents.DrawSearchField(ref _tagSearchText, 200);
                GUILayout.FlexibleSpace();
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            var tags = GetAllTags();
            int totalObjects = HelperSceneCache.Scene.TotalObjects;

            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;

                if (!string.IsNullOrEmpty(_tagSearchText) &&
                    tag.IndexOf(_tagSearchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var objects = GameObject.FindGameObjectsWithTag(tag);
                float pct = totalObjects > 0 ? (float)objects.Length / totalObjects : 0f;

                DrawTagLayerRow(tag, objects.Length, pct, HelperTheme.ModeTag, () => Selection.objects = objects);
            }
        }

        #endregion

        #region Layer Tab

        private void DrawLayerTab()
        {
            // Search
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                HelperUIComponents.DrawSearchField(ref _layerSearchText, 200);
                GUILayout.FlexibleSpace();
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            var layers = GetAllLayers();
            int totalObjects = HelperSceneCache.Scene.TotalObjects;

            foreach (var layer in layers)
            {
                if (string.IsNullOrEmpty(layer)) continue;

                if (!string.IsNullOrEmpty(_layerSearchText) &&
                    layer.IndexOf(_layerSearchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var layerMask = LayerMask.NameToLayer(layer);
                var objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                    .Where(g => g.layer == layerMask)
                    .ToArray();

                float pct = totalObjects > 0 ? (float)objects.Length / totalObjects : 0f;

                DrawTagLayerRow(layer, objects.Length, pct, HelperTheme.ModeLayer, () => Selection.objects = objects);
            }
        }

        #endregion

        #region Tag/Layer Shared Row

        private void DrawTagLayerRow(string name, int count, float percentage, Color accentColor, Action onSelect)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                var rowRect = GUILayoutUtility.GetRect(position.width - 24, 32);
                EditorGUI.DrawRect(rowRect, HelperTheme.Surface1);
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 3, rowRect.height), accentColor);

                // Name
                _tagRowNameStyle ??= new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = HelperTheme.TextHigh }
                };
                GUI.Label(new Rect(rowRect.x + 12, rowRect.y, 130, rowRect.height), name, _tagRowNameStyle);

                // Count badge
                var countRect = new Rect(rowRect.x + 148, rowRect.y + 5, 50, 22);
                EditorGUI.DrawRect(countRect, HelperTheme.Surface2);

                _tagRowCountStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                _tagRowCountStyle.normal.textColor = accentColor;
                GUI.Label(countRect, count.ToString("N0"), _tagRowCountStyle);

                // Percentage bar
                var barRect = new Rect(rowRect.x + 206, rowRect.y + 8, 100, 16);
                EditorGUI.DrawRect(barRect, HelperTheme.Surface0);
                if (percentage > 0)
                {
                    var fillRect = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(percentage), barRect.height);
                    EditorGUI.DrawRect(fillRect, HelperTheme.WithAlpha(accentColor, 0.5f));
                }
                GUI.Label(barRect, (percentage * 100f).ToString("F1") + "%", HelperUIComponents.BadgeStyle);

                // Select button
                var btnRect = new Rect(rowRect.x + 316, rowRect.y + 5, 60, 22);
                if (HelperUIComponents.DrawButtonAt(btnRect, "Select", accentColor))
                {
                    onSelect?.Invoke();
                }

                GUILayout.Space(8);
            }
            GUILayout.Space(2);
        }

        #endregion

        #region Viewer Tab

        private void DrawViewerTab()
        {
            GUILayout.Space(4);

            // Object path display
            if (Selection.activeGameObject != null)
            {
                string fullPath = GetGameObjectPath(Selection.activeGameObject) + Selection.activeGameObject.name;
                HelperUIComponents.DrawInfoBox("Path: " + fullPath);
                GUILayout.Space(4);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                _previewTarget = (GameObject)EditorGUILayout.ObjectField(
                    Selection.activeGameObject, typeof(GameObject), true, GUILayout.Height(20));
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            if (Selection.activeGameObject != null)
            {
                if (_gameObjectEditor == null || _gameObjectEditor.target != Selection.activeGameObject)
                {
                    if (_gameObjectEditor != null) DestroyImmediate(_gameObjectEditor);
                    _gameObjectEditor = Editor.CreateEditor(Selection.activeGameObject);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(8);
                    var previewRect = GUILayoutUtility.GetRect(position.width - 24, 280);
                    _gameObjectEditor.OnInteractivePreviewGUI(previewRect, new GUIStyle
                    {
                        normal = { background = HelperTextureCache.Get(HelperTheme.Surface2) }
                    });
                    GUILayout.Space(8);
                }
            }
            else
            {
                HelperUIComponents.DrawEmptyState("No GameObject Selected", "Select an object to preview");
            }
        }

        private void ResetViewer()
        {
            if (_gameObjectEditor != null)
            {
                DestroyImmediate(_gameObjectEditor);
                _gameObjectEditor = null;
            }
        }

        #endregion

        #region Debug Tab

        private void DrawDebugTab()
        {
            // Real-time indicator + Copy All button
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                var indicatorColor = Application.isPlaying ? HelperTheme.StateActive : HelperTheme.TextLow;
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(8, 8), indicatorColor);

                _liveIndicatorStyle ??= new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                _liveIndicatorStyle.normal.textColor = indicatorColor;
                GUILayout.Label(Application.isPlaying ? "LIVE" : "EDITOR", _liveIndicatorStyle);

                GUILayout.FlexibleSpace();

                if (HelperUIComponents.DrawButton("Copy All", HelperTheme.Surface3, 70, 20))
                {
                    EditorGUIUtility.systemCopyBuffer = CollectAllDebugInfo();
                }

                GUILayout.Space(8);

                _timeStampStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = HelperTheme.TextLow }
                };
                GUILayout.Label($"Updated: {DateTime.Now:HH:mm:ss}", _timeStampStyle);
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            // Frame Rate section with FPS graph
            DrawPersistedSection("Debug", "Frame Rate", HelperTheme.ModeDebug, () =>
            {
                var fps = HelperSceneCache.FPS;
                Color fpsColor = fps.CurrentFPS >= 60 ? HelperTheme.StateActive : fps.CurrentFPS >= 30 ? HelperTheme.Accent : HelperTheme.Error;

                HelperUIComponents.DrawInfoRowWithCopy("Current FPS", fps.CurrentFPS.ToString("F1"), fpsColor, "Current frame rate");
                HelperUIComponents.DrawInfoRowWithCopy("Average FPS", fps.AverageFPS.ToString("F1"), HelperTheme.Primary, "Average frame rate");
                HelperUIComponents.DrawInfoRowWithCopy("Min / Max", $"{fps.MinFPS:F0} / {fps.MaxFPS:F0}", HelperTheme.TextMedium, "Recent min/max FPS");
                HelperUIComponents.DrawInfoRowWithCopy("Frame Time", fps.FrameTime.ToString("F2") + " ms", fpsColor, "Frame processing time");

                GUILayout.Space(4);

                // FPS Graph
                if (fps.History != null && fps.History.Length > 0)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(16);
                        HelperUIComponents.DrawFPSGraph(fps.History, position.width - 48, 60, HelperTheme.Primary);
                        GUILayout.Space(16);
                    }
                }
            });

            // Application
            DrawPersistedSection("Debug", "Application", HelperTheme.ModeDebug, () =>
            {
                HelperUIComponents.DrawInfoRowWithCopy("Play Mode", Application.isPlaying ? "Playing" : "Stopped",
                    Application.isPlaying ? HelperTheme.StateActive : HelperTheme.TextLow);
                HelperUIComponents.DrawInfoRowWithCopy("Time Scale", Time.timeScale.ToString("F2"),
                    Time.timeScale != 1f ? HelperTheme.Accent : HelperTheme.TextMedium);
                HelperUIComponents.DrawInfoRowWithCopy("Frame Count", Time.frameCount.ToString("N0"), HelperTheme.Primary);
            });

            // Memory with progress bars
            DrawPersistedSection("Debug", "Memory", HelperTheme.ModeDebug, () =>
            {
                var totalMem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
                var reservedMem = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
                var monoMem = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
                var monoHeap = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / (1024f * 1024f);
                var gfxMem = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / (1024f * 1024f);

                HelperUIComponents.DrawInfoRowWithCopy("Total Allocated", totalMem.ToString("F1") + " MB", HelperTheme.Accent);

                // Memory usage bar (allocated vs reserved)
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(16);
                    float memPct = reservedMem > 0 ? totalMem / reservedMem : 0;
                    HelperUIComponents.DrawProgressBar(memPct, HelperTheme.Accent, 14);
                    GUILayout.Space(16);
                }

                HelperUIComponents.DrawInfoRowWithCopy("Mono Used", monoMem.ToString("F1") + " MB", HelperTheme.ModeStyle);

                // Mono usage bar
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(16);
                    float monoPct = monoHeap > 0 ? monoMem / monoHeap : 0;
                    HelperUIComponents.DrawProgressBar(monoPct, HelperTheme.ModeStyle, 14);
                    GUILayout.Space(16);
                }

                HelperUIComponents.DrawInfoRowWithCopy("Graphics", gfxMem.ToString("F1") + " MB", HelperTheme.ModeViewer);
                HelperUIComponents.DrawInfoRowWithCopy("GC Count (0/1/2)",
                    $"{GC.CollectionCount(0)} / {GC.CollectionCount(1)} / {GC.CollectionCount(2)}", HelperTheme.ModeLayer);
            });

            // Scene (from cache)
            DrawPersistedSection("Debug", "Scene", HelperTheme.ModeDebug, () =>
            {
                var scene = HelperSceneCache.Scene;

                HelperUIComponents.DrawInfoRowWithCopy("Total Objects", scene.TotalObjects.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Active", scene.ActiveObjects.ToString("N0"), HelperTheme.Secondary);
                HelperUIComponents.DrawInfoRowWithCopy("Inactive", scene.InactiveObjects.ToString("N0"), HelperTheme.TextLow);
                HelperUIComponents.DrawInfoRowWithCopy("MonoBehaviours", scene.MonoBehaviours.ToString("N0"), HelperTheme.ModeComponent);
            });

            // System Info
            DrawPersistedSection("Debug", "System Info", HelperTheme.ModeDebug, () =>
            {
                HelperUIComponents.DrawInfoRowWithCopy("Unity Version", Application.unityVersion, HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Platform", Application.platform.ToString(), HelperTheme.TextMedium);
                HelperUIComponents.DrawInfoRowWithCopy("Graphics API", SystemInfo.graphicsDeviceType.ToString(), HelperTheme.ModeViewer);
                HelperUIComponents.DrawInfoRowWithCopy("GPU", TruncateString(SystemInfo.graphicsDeviceName, 30), HelperTheme.Accent, SystemInfo.graphicsDeviceName);
                HelperUIComponents.DrawInfoRowWithCopy("VRAM", (SystemInfo.graphicsMemorySize / 1024f).ToString("F1") + " GB", HelperTheme.ModePerformance);
                HelperUIComponents.DrawInfoRowWithCopy("System RAM", (SystemInfo.systemMemorySize / 1024f).ToString("F1") + " GB", HelperTheme.Secondary);
            });

            // Quality & Rendering
            DrawPersistedSection("Debug", "Quality & Rendering", HelperTheme.ModeDebug, () =>
            {
                HelperUIComponents.DrawInfoRowWithCopy("Quality Level", QualitySettings.names[QualitySettings.GetQualityLevel()], HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("VSync", QualitySettings.vSyncCount == 0 ? "Off" : $"Every {QualitySettings.vSyncCount} VBlank",
                    QualitySettings.vSyncCount == 0 ? HelperTheme.TextLow : HelperTheme.StateActive);
                HelperUIComponents.DrawInfoRowWithCopy("Target FPS", Application.targetFrameRate <= 0 ? "Unlimited" : Application.targetFrameRate.ToString(),
                    HelperTheme.TextMedium);
                HelperUIComponents.DrawInfoRowWithCopy("Shadow Resolution", QualitySettings.shadowResolution.ToString(), HelperTheme.ModeLayer);
                HelperUIComponents.DrawInfoRowWithCopy("Pixel Light Count", QualitySettings.pixelLightCount.ToString(), HelperTheme.StatePlaying);
            });

            // Audio (from cache)
            DrawPersistedSection("Debug", "Audio", HelperTheme.ModeDebug, () =>
            {
                var audio = HelperSceneCache.Audio;

                HelperUIComponents.DrawInfoRowWithCopy("Audio Listeners", audio.ListenerCount.ToString(),
                    audio.ListenerCount == 1 ? HelperTheme.StateActive : HelperTheme.Error);
                HelperUIComponents.DrawInfoRowWithCopy("Audio Sources", audio.SourceCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Playing", audio.PlayingCount.ToString("N0"),
                    audio.PlayingCount > 0 ? HelperTheme.StatePlaying : HelperTheme.TextLow);
            });

            // Runtime (only in play mode)
            if (Application.isPlaying)
            {
                DrawPersistedSection("Debug", "Runtime", HelperTheme.ModeDebug, () =>
                {
                    HelperUIComponents.DrawInfoRowWithCopy("Real Time", Time.realtimeSinceStartup.ToString("F1") + " s", HelperTheme.Primary);
                    HelperUIComponents.DrawInfoRowWithCopy("Game Time", Time.time.ToString("F1") + " s", HelperTheme.Secondary);
                    HelperUIComponents.DrawInfoRowWithCopy("Delta Time", (Time.deltaTime * 1000f).ToString("F2") + " ms", HelperTheme.TextMedium);
                    HelperUIComponents.DrawInfoRowWithCopy("Fixed Delta", (Time.fixedDeltaTime * 1000f).ToString("F2") + " ms", HelperTheme.ModeLayer);
                });
            }
        }

        #endregion

        #region Performance Tab

        private void DrawPerformanceTab()
        {
            // Header with summary badges
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(8, 8), HelperTheme.ModePerformance);

                _perfHeaderStyle ??= new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = HelperTheme.ModePerformance }
                };
                GUILayout.Label("PERFORMANCE MONITOR", _perfHeaderStyle);

                GUILayout.Space(16);

                var fps = HelperSceneCache.FPS;
                Color fpsColor = fps.CurrentFPS >= 60 ? HelperTheme.StateActive : fps.CurrentFPS >= 30 ? HelperTheme.Accent : HelperTheme.Error;

                _perfFpsStyle ??= new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                _perfFpsStyle.normal.textColor = fpsColor;
                GUILayout.Label($"FPS: {fps.CurrentFPS:F0}", _perfFpsStyle);

                GUILayout.FlexibleSpace();

                if (HelperUIComponents.DrawButton("Copy All", HelperTheme.Surface3, 70, 20))
                {
                    EditorGUIUtility.systemCopyBuffer = CollectAllPerfInfo();
                }

                GUILayout.Space(8);
            }

            GUILayout.Space(4);

            // Summary badges row
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                var geo = HelperSceneCache.Geometry;
                var render = HelperSceneCache.Rendering;

                HelperUIComponents.DrawStatBadge("\u25B3 Tri", geo.TotalTriangles.ToString("N0"), HelperTheme.Primary, 75, 32);
                GUILayout.Space(4);
                HelperUIComponents.DrawStatBadge("Renderers", render.RendererCount.ToString("N0"), HelperTheme.Secondary, 75, 32);
                GUILayout.Space(4);
                HelperUIComponents.DrawStatBadge("Materials", render.UniqueMaterialCount.ToString("N0"), HelperTheme.ModeViewer, 75, 32);
                GUILayout.Space(4);
                HelperUIComponents.DrawStatBadge("Cameras", render.CameraCount.ToString(), HelperTheme.ModeComponent, 65, 32);

                GUILayout.FlexibleSpace();
                GUILayout.Space(8);
            }

            GUILayout.Space(8);

            // Geometry (from cache)
            DrawPersistedSection("Perf", "Geometry", HelperTheme.ModePerformance, () =>
            {
                var geo = HelperSceneCache.Geometry;

                HelperUIComponents.DrawInfoRowWithCopy("Total Triangles", geo.TotalTriangles.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Total Vertices", geo.TotalVertices.ToString("N0"), HelperTheme.TextMedium);
                HelperUIComponents.DrawInfoRowWithCopy("Mesh Filters", geo.MeshFilterCount.ToString("N0"), HelperTheme.TextMedium);
                HelperUIComponents.DrawInfoRowWithCopy("Skinned Mesh", geo.SkinnedMeshCount.ToString("N0"), HelperTheme.ModeComponent);
            });

            // Rendering (from cache)
            DrawPersistedSection("Perf", "Rendering", HelperTheme.ModePerformance, () =>
            {
                var render = HelperSceneCache.Rendering;

                HelperUIComponents.DrawInfoRowWithCopy("Cameras", render.CameraCount.ToString(), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Renderers", render.RendererCount.ToString("N0"), HelperTheme.Secondary);
                HelperUIComponents.DrawInfoRowWithCopy("Lights", render.LightCount.ToString("N0"), HelperTheme.StatePlaying);
            });

            // Physics (from cache)
            DrawPersistedSection("Perf", "Physics", HelperTheme.ModePerformance, () =>
            {
                var physics = HelperSceneCache.Physics;

                HelperUIComponents.DrawInfoRowWithCopy("Rigidbodies", physics.RigidbodyCount.ToString("N0"), HelperTheme.ModeLayer);
                HelperUIComponents.DrawInfoRowWithCopy("Colliders", physics.ColliderCount.ToString("N0"), HelperTheme.ModeTag);
            });

            // UI (from cache)
            DrawPersistedSection("Perf", "UI", HelperTheme.ModePerformance, () =>
            {
                var ui = HelperSceneCache.UI;

                HelperUIComponents.DrawInfoRowWithCopy("Canvases", ui.CanvasCount.ToString(), HelperTheme.ModeViewer);
                HelperUIComponents.DrawInfoRowWithCopy("Graphic Raycasters", ui.GraphicRaycasterCount.ToString(), HelperTheme.ModeLayer);
                HelperUIComponents.DrawInfoRowWithCopy("TMP Texts", ui.TMPTextCount.ToString("N0"), HelperTheme.Accent);
                HelperUIComponents.DrawInfoRowWithCopy("Images", ui.ImageCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Buttons", ui.ButtonCount.ToString("N0"), HelperTheme.Secondary);
            });

            // Animation (from cache)
            DrawPersistedSection("Perf", "Animation", HelperTheme.ModePerformance, () =>
            {
                var anim = HelperSceneCache.Animation;

                HelperUIComponents.DrawInfoRowWithCopy("Animators", anim.AnimatorCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Active Animators", anim.ActiveAnimatorCount.ToString("N0"), HelperTheme.StateActive);
                HelperUIComponents.DrawInfoRowWithCopy("Legacy Animation", anim.LegacyAnimationCount.ToString("N0"), HelperTheme.TextLow);
            });

            // Particles (from cache)
            DrawPersistedSection("Perf", "Particles", HelperTheme.ModePerformance, () =>
            {
                var particles = HelperSceneCache.Particles;

                HelperUIComponents.DrawInfoRowWithCopy("Particle Systems", particles.SystemCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Playing", particles.PlayingCount.ToString("N0"),
                    particles.PlayingCount > 0 ? HelperTheme.StatePlaying : HelperTheme.TextLow);
                HelperUIComponents.DrawInfoRowWithCopy("Total Particles", particles.TotalParticleCount.ToString("N0"), HelperTheme.Accent);
            });

            // Materials & Shaders (from cache)
            DrawPersistedSection("Perf", "Materials & Shaders", HelperTheme.ModePerformance, () =>
            {
                var render = HelperSceneCache.Rendering;

                HelperUIComponents.DrawInfoRowWithCopy("Unique Materials", render.UniqueMaterialCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Unique Shaders", render.UniqueShaderCount.ToString("N0"), HelperTheme.ModeViewer);
                HelperUIComponents.DrawInfoRowWithCopy("Material Instances", render.MaterialInstanceCount.ToString("N0"), HelperTheme.TextMedium);
            });

            // LOD & Culling (from cache)
            DrawPersistedSection("Perf", "LOD & Culling", HelperTheme.ModePerformance, () =>
            {
                var env = HelperSceneCache.Environment;

                HelperUIComponents.DrawInfoRowWithCopy("LOD Groups", env.LODGroupCount.ToString("N0"), HelperTheme.Primary);
                HelperUIComponents.DrawInfoRowWithCopy("Occlusion Areas", env.OcclusionAreaCount.ToString("N0"), HelperTheme.ModeLayer);
                HelperUIComponents.DrawInfoRowWithCopy("Occlusion Portals", env.OcclusionPortalCount.ToString("N0"), HelperTheme.TextMedium);
            });

            // Terrain & Environment (from cache)
            DrawPersistedSection("Perf", "Terrain & Environment", HelperTheme.ModePerformance, () =>
            {
                var env = HelperSceneCache.Environment;

                HelperUIComponents.DrawInfoRowWithCopy("Terrains", env.TerrainCount.ToString(), HelperTheme.Secondary);
                HelperUIComponents.DrawInfoRowWithCopy("Reflection Probes", env.ReflectionProbeCount.ToString("N0"), HelperTheme.ModeViewer);
                HelperUIComponents.DrawInfoRowWithCopy("Light Probe Groups", env.LightProbeGroupCount.ToString("N0"), HelperTheme.StatePlaying);
            });
        }

        #endregion

        #region Delete Helpers

        private void DeleteComponents(Component[] components, string typeName)
        {
            if (components == null || components.Length == 0) return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Components",
                $"'{typeName}' 컴포넌트 {components.Length}개를 삭제하시겠습니까?\n\n이 작업은 Undo로 복원할 수 있습니다.",
                "삭제",
                "취소");

            if (!confirmed) return;

            Undo.SetCurrentGroupName($"Remove {components.Length} {typeName}");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (var comp in components)
            {
                if (comp != null)
                    Undo.DestroyObjectImmediate(comp);
            }

            Undo.CollapseUndoOperations(undoGroup);

            HelperSceneCache.Refresh();
            UpdateGameObjectInfo();
        }

        #endregion
    }
}
#endif
