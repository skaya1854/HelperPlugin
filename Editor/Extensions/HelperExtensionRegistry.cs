#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Registry for Helper tab extensions. Discovers and manages IHelperExtension implementations.
    /// </summary>
    public static class HelperExtensionRegistry
    {
        private static List<IHelperExtension> _extensions = new List<IHelperExtension>();
        private static bool _initialized = false;

        /// <summary>
        /// All registered extensions sorted by order
        /// </summary>
        public static IReadOnlyList<IHelperExtension> Extensions
        {
            get
            {
                EnsureInitialized();
                return _extensions;
            }
        }

        /// <summary>
        /// Register an extension instance
        /// </summary>
        public static void Register(IHelperExtension extension)
        {
            EnsureInitialized();

            if (_extensions.Any(e => e.GetType() == extension.GetType()))
            {
                return; // Already registered
            }

            _extensions.Add(extension);
            _extensions = _extensions.OrderBy(e => e.Order).ToList();
        }

        /// <summary>
        /// Unregister an extension
        /// </summary>
        public static void Unregister<T>() where T : IHelperExtension
        {
            _extensions.RemoveAll(e => e is T);
        }

        /// <summary>
        /// Force re-discovery of extensions
        /// </summary>
        public static void Refresh()
        {
            _extensions.Clear();
            _initialized = false;
            EnsureInitialized();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _initialized = false;
            _extensions.Clear();
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            DiscoverExtensions();
        }

        private static void DiscoverExtensions()
        {
            var extensionType = typeof(IHelperExtension);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => extensionType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        try
                        {
                            var instance = (IHelperExtension)Activator.CreateInstance(type);
                            _extensions.Add(instance);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[HelperPlugin] Failed to create extension {type.Name}: {ex.Message}");
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Only log in verbose mode or when types were partially loaded
                    if (ex.Types?.Any(t => t != null) == true)
                        Debug.LogWarning($"[HelperPlugin] Partial load for assembly {assembly.GetName().Name}");
                }
            }

            _extensions = _extensions.OrderBy(e => e.Order).ToList();
        }
    }
}
#endif
