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
    /// Registry for Helper buttons. Discovers buttons from attributes and allows runtime registration.
    /// </summary>
    public static class HelperButtonRegistry
    {
        /// <summary>
        /// Represents a registered button
        /// </summary>
        public class ButtonInfo
        {
            public string Name { get; set; }
            public string DefaultValue { get; set; }
            public string Category { get; set; }
            public int Order { get; set; }
            public bool RequiresPlayMode { get; set; }
            public Action<string> Action { get; set; }
            public bool IsRuntimeRegistered { get; set; }

            /// <summary>
            /// Current parameter value (editable in UI)
            /// </summary>
            public string CurrentValue { get; set; }
        }

        private static List<ButtonInfo> _buttons = new List<ButtonInfo>();
        private static bool _initialized = false;

        /// <summary>
        /// All registered buttons
        /// </summary>
        public static IReadOnlyList<ButtonInfo> Buttons
        {
            get
            {
                EnsureInitialized();
                return _buttons;
            }
        }

        /// <summary>
        /// Get buttons by category
        /// </summary>
        public static IEnumerable<ButtonInfo> GetButtonsByCategory(string category)
        {
            EnsureInitialized();
            return _buttons.Where(b => b.Category == category).OrderBy(b => b.Order);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public static IEnumerable<string> GetCategories()
        {
            EnsureInitialized();
            return _buttons.Select(b => b.Category).Distinct().OrderBy(c => c);
        }

        /// <summary>
        /// Register a button at runtime
        /// </summary>
        public static void Register(string name, string defaultValue, Action<string> action,
            string category = "General", int order = 0, bool requiresPlayMode = false)
        {
            EnsureInitialized();

            // Check for duplicate
            var existing = _buttons.FirstOrDefault(b => b.Name == name && b.Category == category);
            if (existing != null)
            {
                existing.Action = action;
                existing.DefaultValue = defaultValue;
                existing.CurrentValue = defaultValue;
                existing.RequiresPlayMode = requiresPlayMode;
                return;
            }

            _buttons.Add(new ButtonInfo
            {
                Name = name,
                DefaultValue = defaultValue,
                CurrentValue = defaultValue,
                Category = category,
                Order = order,
                RequiresPlayMode = requiresPlayMode,
                Action = action,
                IsRuntimeRegistered = true
            });
        }

        /// <summary>
        /// Unregister a runtime button
        /// </summary>
        public static void Unregister(string name, string category = "General")
        {
            _buttons.RemoveAll(b => b.Name == name && b.Category == category && b.IsRuntimeRegistered);
        }

        /// <summary>
        /// Clear all runtime registered buttons
        /// </summary>
        public static void ClearRuntimeButtons()
        {
            _buttons.RemoveAll(b => b.IsRuntimeRegistered);
        }

        /// <summary>
        /// Update button parameter value
        /// </summary>
        public static void SetButtonValue(int index, string value)
        {
            if (index >= 0 && index < _buttons.Count)
            {
                _buttons[index].CurrentValue = value;
            }
        }

        /// <summary>
        /// Force re-discovery of buttons
        /// </summary>
        public static void Refresh()
        {
            var runtimeButtons = _buttons.Where(b => b.IsRuntimeRegistered).ToList();
            _buttons.Clear();
            _initialized = false;
            EnsureInitialized();
            _buttons.AddRange(runtimeButtons);
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            _initialized = false;
            _buttons.Clear();
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            DiscoverAttributeButtons();
        }

        private static void DiscoverAttributeButtons()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        // Check for container attribute
                        var containerAttr = type.GetCustomAttribute<HelperButtonContainerAttribute>();
                        string containerCategory = containerAttr?.Category ?? "General";

                        // Find methods with HelperButton attribute
                        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                        foreach (var method in methods)
                        {
                            var buttonAttr = method.GetCustomAttribute<HelperButtonAttribute>();
                            if (buttonAttr == null) continue;

                            // Validate method signature
                            var parameters = method.GetParameters();
                            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string))
                            {
                                Debug.LogWarning($"[HelperPlugin] Method {type.Name}.{method.Name} has [HelperButton] but wrong signature. Expected: void Method(string param)");
                                continue;
                            }

                            var category = !string.IsNullOrEmpty(buttonAttr.Category) ? buttonAttr.Category : containerCategory;

                            var capturedMethod = method;
                            var capturedName = buttonAttr.Name;
                            _buttons.Add(new ButtonInfo
                            {
                                Name = capturedName,
                                DefaultValue = buttonAttr.DefaultValue,
                                CurrentValue = buttonAttr.DefaultValue,
                                Category = category,
                                Order = buttonAttr.Order,
                                RequiresPlayMode = buttonAttr.RequiresPlayMode,
                                Action = (param) =>
                                {
                                    try { capturedMethod.Invoke(null, new object[] { param }); }
                                    catch (Exception ex) { Debug.LogError($"[HelperPlugin] Button '{capturedName}' error: {ex.InnerException?.Message ?? ex.Message}"); }
                                },
                                IsRuntimeRegistered = false
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[HelperPlugin] Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            // Sort by category, then order
            _buttons = _buttons.OrderBy(b => b.Category).ThenBy(b => b.Order).ToList();
        }
    }
}
#endif
