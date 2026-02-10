#if UNITY_EDITOR
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Interface for Helper window extensions.
    /// Implement this to add custom tabs/panels to the Helper window.
    /// </summary>
    public interface IHelperExtension
    {
        /// <summary>
        /// Display name shown in the tab bar
        /// </summary>
        string TabName { get; }

        /// <summary>
        /// Tab accent color
        /// </summary>
        Color TabColor { get; }

        /// <summary>
        /// Sort order for tab position (lower = left)
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Called when the tab is selected. Draw your GUI here.
        /// </summary>
        void OnGUI();

        /// <summary>
        /// Called when selection changes in the hierarchy
        /// </summary>
        void OnSelectionChange();

        /// <summary>
        /// Called on editor update tick
        /// </summary>
        void OnEditorUpdate();
    }

    /// <summary>
    /// Base class for Helper extensions with default implementations
    /// </summary>
    public abstract class HelperExtensionBase : IHelperExtension
    {
        public abstract string TabName { get; }
        public virtual Color TabColor => HelperTheme.Primary;
        public virtual int Order => 100;

        public abstract void OnGUI();
        public virtual void OnSelectionChange() { }
        public virtual void OnEditorUpdate() { }
    }
}
#endif
