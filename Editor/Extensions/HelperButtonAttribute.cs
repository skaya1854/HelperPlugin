#if UNITY_EDITOR
using System;

namespace HelperPlugin
{
    /// <summary>
    /// Attribute to mark a static method as a Helper test button.
    /// Methods must have signature: static void MethodName(string parameter)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HelperButtonAttribute : Attribute
    {
        /// <summary>
        /// Button display name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Default parameter value
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// Category for grouping (optional)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Sort order within category
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Whether this button requires Play Mode
        /// </summary>
        public bool RequiresPlayMode { get; set; }

        public HelperButtonAttribute(string name, string defaultValue = "void")
        {
            Name = name;
            DefaultValue = defaultValue;
            Category = "General";
            Order = 0;
            RequiresPlayMode = false;
        }
    }

    /// <summary>
    /// Attribute to mark a class containing Helper buttons.
    /// All [HelperButton] methods in this class will be discovered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HelperButtonContainerAttribute : Attribute
    {
        /// <summary>
        /// Category name for all buttons in this class
        /// </summary>
        public string Category { get; }

        public HelperButtonContainerAttribute(string category = "General")
        {
            Category = category;
        }
    }
}
#endif
