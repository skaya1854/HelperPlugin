#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HelperPlugin
{
    /// <summary>
    /// Cached scene analysis system that avoids repeated FindObjectsByType calls.
    /// Auto-refreshes at a configurable interval via EditorApplication.update.
    /// </summary>
    public static class HelperSceneCache
    {
        // --- Stat structs (PascalCase public fields per C# convention) ---

        [Serializable]
        public readonly struct SceneStats
        {
            public readonly int TotalObjects;
            public readonly int ActiveObjects;
            public readonly int InactiveObjects;
            public readonly int MonoBehaviours;

            public SceneStats(int total, int active, int inactive, int mono)
            {
                TotalObjects = total;
                ActiveObjects = active;
                InactiveObjects = inactive;
                MonoBehaviours = mono;
            }
        }

        [Serializable]
        public readonly struct GeometryStats
        {
            public readonly long TotalTriangles;
            public readonly int MeshFilterCount;
            public readonly int SkinnedMeshCount;
            public readonly long TotalVertices;

            public GeometryStats(long triangles, int filters, int skinned, long verts)
            {
                TotalTriangles = triangles;
                MeshFilterCount = filters;
                SkinnedMeshCount = skinned;
                TotalVertices = verts;
            }
        }

        [Serializable]
        public readonly struct RenderingStats
        {
            public readonly int CameraCount;
            public readonly int LightCount;
            public readonly int RendererCount;
            public readonly int UniqueMaterialCount;
            public readonly int UniqueShaderCount;
            public readonly int MaterialInstanceCount;

            public RenderingStats(int cam, int lit, int rend, int uMat, int uShd, int matInst)
            {
                CameraCount = cam;
                LightCount = lit;
                RendererCount = rend;
                UniqueMaterialCount = uMat;
                UniqueShaderCount = uShd;
                MaterialInstanceCount = matInst;
            }
        }

        [Serializable]
        public readonly struct PhysicsStats
        {
            public readonly int RigidbodyCount;
            public readonly int ColliderCount;

            public PhysicsStats(int rb, int col)
            {
                RigidbodyCount = rb;
                ColliderCount = col;
            }
        }

        [Serializable]
        public readonly struct UIStats
        {
            public readonly int CanvasCount;
            public readonly int TMPTextCount;
            public readonly int ImageCount;
            public readonly int ButtonCount;
            public readonly int GraphicRaycasterCount;

            public UIStats(int canvas, int tmp, int img, int btn, int raycaster)
            {
                CanvasCount = canvas;
                TMPTextCount = tmp;
                ImageCount = img;
                ButtonCount = btn;
                GraphicRaycasterCount = raycaster;
            }
        }

        [Serializable]
        public readonly struct AnimationStats
        {
            public readonly int AnimatorCount;
            public readonly int ActiveAnimatorCount;
            public readonly int LegacyAnimationCount;

            public AnimationStats(int anim, int active, int legacy)
            {
                AnimatorCount = anim;
                ActiveAnimatorCount = active;
                LegacyAnimationCount = legacy;
            }
        }

        [Serializable]
        public readonly struct ParticleStats
        {
            public readonly int SystemCount;
            public readonly int PlayingCount;
            public readonly int TotalParticleCount;

            public ParticleStats(int systems, int playing, int total)
            {
                SystemCount = systems;
                PlayingCount = playing;
                TotalParticleCount = total;
            }
        }

        [Serializable]
        public readonly struct AudioStats
        {
            public readonly int ListenerCount;
            public readonly int SourceCount;
            public readonly int PlayingCount;

            public AudioStats(int listeners, int sources, int playing)
            {
                ListenerCount = listeners;
                SourceCount = sources;
                PlayingCount = playing;
            }
        }

        [Serializable]
        public readonly struct EnvironmentStats
        {
            public readonly int TerrainCount;
            public readonly int ReflectionProbeCount;
            public readonly int LightProbeGroupCount;
            public readonly int LODGroupCount;
            public readonly int OcclusionAreaCount;
            public readonly int OcclusionPortalCount;

            public EnvironmentStats(int ter, int refProbe, int lpg, int lod, int occArea, int occPortal)
            {
                TerrainCount = ter;
                ReflectionProbeCount = refProbe;
                LightProbeGroupCount = lpg;
                LODGroupCount = lod;
                OcclusionAreaCount = occArea;
                OcclusionPortalCount = occPortal;
            }
        }

        [Serializable]
        public struct FPSData
        {
            public float CurrentFPS;
            public float AverageFPS;
            public float MinFPS;
            public float MaxFPS;
            public float FrameTime;
            public float[] History;
        }

        // --- Cached data ---

        private static SceneStats _sceneStats;
        private static GeometryStats _geometryStats;
        private static RenderingStats _renderingStats;
        private static PhysicsStats _physicsStats;
        private static UIStats _uiStats;
        private static AnimationStats _animationStats;
        private static ParticleStats _particleStats;
        private static AudioStats _audioStats;
        private static EnvironmentStats _environmentStats;
        private static FPSData _fpsData;

        private static double _lastRefreshTime;
        private static bool _isDirty;
        private static bool _initialized;

        private const int FPS_HISTORY_SIZE = 120;
        private static int _fpsHistoryIndex;

        // --- Public properties ---

        /// <summary>Refresh interval in seconds. Default: 1.0</summary>
        public static float RefreshInterval { get; set; } = 1.0f;

        public static SceneStats Scene => EnsureAndReturn(ref _sceneStats);
        public static GeometryStats Geometry => EnsureAndReturn(ref _geometryStats);
        public static RenderingStats Rendering => EnsureAndReturn(ref _renderingStats);
        public static PhysicsStats Physics => EnsureAndReturn(ref _physicsStats);
        public static UIStats UI => EnsureAndReturn(ref _uiStats);
        public static AnimationStats Animation => EnsureAndReturn(ref _animationStats);
        public static ParticleStats Particles => EnsureAndReturn(ref _particleStats);
        public static AudioStats Audio => EnsureAndReturn(ref _audioStats);
        public static EnvironmentStats Environment => EnsureAndReturn(ref _environmentStats);
        public static FPSData FPS => EnsureAndReturn(ref _fpsData);

        /// <summary>True if data changed since last IsDirty check. Resets on read.</summary>
        public static bool IsDirty
        {
            get
            {
                bool val = _isDirty;
                _isDirty = false;
                return val;
            }
        }

        // --- Lifecycle ---

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _initialized = false;
            _lastRefreshTime = 0;
            _fpsData.History = new float[FPS_HISTORY_SIZE];
            _fpsHistoryIndex = 0;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;

            // Update FPS every frame
            UpdateFPS(now);

            // Refresh scene stats at interval
            if (now - _lastRefreshTime >= RefreshInterval)
            {
                Refresh();
            }
        }

        // --- Public methods ---

        /// <summary>
        /// Force an immediate refresh of all cached scene data.
        /// </summary>
        public static void Refresh()
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            _initialized = true;

            CollectSceneStats();
            CollectGeometryStats();
            CollectRenderingStats();
            CollectPhysicsStats();
            CollectUIStats();
            CollectAnimationStats();
            CollectParticleStats();
            CollectAudioStats();
            CollectEnvironmentStats();

            _isDirty = true;
        }

        /// <summary>
        /// Get all components on a GameObject grouped by type.
        /// Uses a single GetComponents call to avoid O(n^2) overhead.
        /// </summary>
        public static Dictionary<Type, List<Component>> GetComponentGroups(GameObject target)
        {
            var groups = new Dictionary<Type, List<Component>>();
            if (target == null) return groups;

            // Single GetComponents call - O(n)
            var allComponents = target.GetComponents<Component>();
            foreach (var comp in allComponents)
            {
                if (comp == null) continue; // Missing script
                var type = comp.GetType();
                if (!groups.TryGetValue(type, out var list))
                {
                    list = new List<Component>();
                    groups[type] = list;
                }
                list.Add(comp);
            }

            return groups;
        }

        // --- Private helpers ---

        private static ref T EnsureAndReturn<T>(ref T field)
        {
            if (!_initialized) Refresh();
            return ref field;
        }

        private static void UpdateFPS(double now)
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            float fps = 1f / dt;
            _fpsData.CurrentFPS = fps;
            _fpsData.FrameTime = dt * 1000f;

            if (_fpsData.History == null)
                _fpsData.History = new float[FPS_HISTORY_SIZE];

            _fpsData.History[_fpsHistoryIndex % FPS_HISTORY_SIZE] = fps;
            _fpsHistoryIndex++;

            int count = Mathf.Min(_fpsHistoryIndex, FPS_HISTORY_SIZE);
            float sum = 0f;
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int i = 0; i < count; i++)
            {
                float v = _fpsData.History[i];
                sum += v;
                if (v < min) min = v;
                if (v > max) max = v;
            }

            _fpsData.AverageFPS = sum / count;
            _fpsData.MinFPS = min;
            _fpsData.MaxFPS = max;
        }

        // --- Collection methods ---

        private static void CollectSceneStats()
        {
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int total = allTransforms.Length;
            int active = 0;
            int inactive = 0;

            foreach (var t in allTransforms)
            {
                if (t.gameObject.activeInHierarchy)
                    active++;
                else
                    inactive++;
            }

            var monos = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _sceneStats = new SceneStats(total, active, inactive, monos.Length);
        }

        private static void CollectGeometryStats()
        {
            var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var skinnedRenderers = Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            long totalTris = 0;
            long totalVerts = 0;

            // Use GetIndexCount to avoid allocating the full triangles array
            foreach (var mf in meshFilters)
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    totalTris += (long)mesh.GetIndexCount(sub) / 3;
                totalVerts += mesh.vertexCount;
            }

            foreach (var smr in skinnedRenderers)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null) continue;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    totalTris += (long)mesh.GetIndexCount(sub) / 3;
                totalVerts += mesh.vertexCount;
            }

            _geometryStats = new GeometryStats(totalTris, meshFilters.Length, skinnedRenderers.Length, totalVerts);
        }

        private static void CollectRenderingStats()
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            var uniqueMats = new HashSet<Material>();
            var uniqueShaders = new HashSet<Shader>();
            int matInstanceCount = 0;

            foreach (var rend in renderers)
            {
                var mats = rend.sharedMaterials;
                foreach (var mat in mats)
                {
                    if (mat == null) continue;
                    matInstanceCount++;
                    uniqueMats.Add(mat);
                    if (mat.shader != null)
                        uniqueShaders.Add(mat.shader);
                }
            }

            _renderingStats = new RenderingStats(
                cameras.Length, lights.Length, renderers.Length,
                uniqueMats.Count, uniqueShaders.Count, matInstanceCount
            );
        }

        private static void CollectPhysicsStats()
        {
            var rbs = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var cols = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _physicsStats = new PhysicsStats(rbs.Length, cols.Length);
        }

        private static void CollectUIStats()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var tmpTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var raycasters = Object.FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            _uiStats = new UIStats(canvases.Length, tmpTexts.Length, images.Length, buttons.Length, raycasters.Length);
        }

        private static void CollectAnimationStats()
        {
            var animators = Object.FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int activeCount = 0;
            foreach (var a in animators)
            {
                if (a.enabled && a.gameObject.activeInHierarchy)
                    activeCount++;
            }

            var legacy = Object.FindObjectsByType<Animation>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _animationStats = new AnimationStats(animators.Length, activeCount, legacy.Length);
        }

        private static void CollectParticleStats()
        {
            var systems = Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int playing = 0;
            int totalCount = 0;

            foreach (var ps in systems)
            {
                if (ps.isPlaying)
                    playing++;
                totalCount += ps.particleCount;
            }

            _particleStats = new ParticleStats(systems.Length, playing, totalCount);
        }

        private static void CollectAudioStats()
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int playing = 0;

            foreach (var src in sources)
            {
                if (src.isPlaying)
                    playing++;
            }

            _audioStats = new AudioStats(listeners.Length, sources.Length, playing);
        }

        private static void CollectEnvironmentStats()
        {
            var terrains = Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var refProbes = Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var lpGroups = Object.FindObjectsByType<LightProbeGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var lodGroups = Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var occAreas = Object.FindObjectsByType<OcclusionArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var occPortals = Object.FindObjectsByType<OcclusionPortal>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            _environmentStats = new EnvironmentStats(
                terrains.Length, refProbes.Length, lpGroups.Length,
                lodGroups.Length, occAreas.Length, occPortals.Length
            );
        }
    }
}
#endif
