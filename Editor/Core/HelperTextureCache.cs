#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Texture cache system to avoid creating duplicate textures for the same color.
    /// Improves performance by reusing previously created 1x1 pixel textures.
    /// </summary>
    public static class HelperTextureCache
    {
        private static readonly Dictionary<int, Texture2D> _cache = new Dictionary<int, Texture2D>();

        /// <summary>
        /// Get or create a 1x1 texture with the specified color.
        /// Textures are cached by integer-based color hash to avoid float precision issues.
        /// </summary>
        public static Texture2D Get(Color color)
        {
            int hash = GetColorHash(color);

            if (!_cache.TryGetValue(hash, out var tex) || tex == null)
            {
                tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, color);
                tex.Apply();
                tex.hideFlags = HideFlags.DontSave;
                _cache[hash] = tex;
            }

            return tex;
        }

        /// <summary>
        /// Clear the texture cache. Call this when domain reloads.
        /// </summary>
        public static void Clear()
        {
            foreach (var tex in _cache.Values)
            {
                if (tex != null)
                {
                    Object.DestroyImmediate(tex);
                }
            }
            _cache.Clear();
        }

        /// <summary>
        /// Get the current cache size.
        /// </summary>
        public static int CacheCount => _cache.Count;

        /// <summary>
        /// Compute a deterministic hash from color components converted to 0-255 integers.
        /// Avoids float precision issues with Color.GetHashCode().
        /// </summary>
        private static int GetColorHash(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            int a = Mathf.RoundToInt(color.a * 255);
            return (r << 24) | (g << 16) | (b << 8) | a;
        }
    }
}
#endif
