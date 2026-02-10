#if UNITY_EDITOR
using UnityEngine;

namespace HelperPlugin
{
    /// <summary>
    /// Material Design inspired theme system for Helper EditorWindow.
    /// Provides consistent styling across all Helper components.
    /// </summary>
    public static class HelperTheme
    {
        #region Surface Colors (Dark Theme)

        /// <summary>Darkest background - used for main window background</summary>
        public static readonly Color Surface0 = new Color(0.08f, 0.09f, 0.11f);

        /// <summary>Card backgrounds</summary>
        public static readonly Color Surface1 = new Color(0.11f, 0.12f, 0.14f);

        /// <summary>Elevated elements, hover states</summary>
        public static readonly Color Surface2 = new Color(0.14f, 0.15f, 0.18f);

        /// <summary>Highest elevation</summary>
        public static readonly Color Surface3 = new Color(0.17f, 0.19f, 0.22f);

        /// <summary>Borders and dividers</summary>
        public static readonly Color Border = new Color(0.22f, 0.24f, 0.28f);

        #endregion

        #region Brand Colors

        /// <summary>Primary blue - main actions, active states</summary>
        public static readonly Color Primary = new Color(0.38f, 0.62f, 0.98f);

        /// <summary>Darker primary for pressed states</summary>
        public static readonly Color PrimaryDark = new Color(0.25f, 0.47f, 0.85f);

        /// <summary>Secondary green - success, positive actions</summary>
        public static readonly Color Secondary = new Color(0.54f, 0.76f, 0.42f);

        /// <summary>Accent amber - highlights, warnings</summary>
        public static readonly Color Accent = new Color(0.98f, 0.73f, 0.25f);

        /// <summary>Error red - errors, destructive actions</summary>
        public static readonly Color Error = new Color(0.94f, 0.36f, 0.36f);

        #endregion

        #region Text Colors

        /// <summary>High emphasis text (95% brightness)</summary>
        public static readonly Color TextHigh = new Color(0.95f, 0.96f, 0.98f);

        /// <summary>Medium emphasis text (70% brightness)</summary>
        public static readonly Color TextMedium = new Color(0.70f, 0.73f, 0.78f);

        /// <summary>Low emphasis / disabled text (45% brightness)</summary>
        public static readonly Color TextLow = new Color(0.45f, 0.48f, 0.52f);

        #endregion

        #region State Colors

        /// <summary>Active state - green</summary>
        public static readonly Color StateActive = new Color(0.30f, 0.69f, 0.31f);

        /// <summary>Playing/Running state - amber</summary>
        public static readonly Color StatePlaying = new Color(0.98f, 0.76f, 0.18f);

        /// <summary>Finished/Complete state - teal</summary>
        public static readonly Color StateFinished = new Color(0.38f, 0.82f, 0.69f);

        /// <summary>Idle/Inactive state - gray</summary>
        public static readonly Color StateIdle = new Color(0.55f, 0.58f, 0.62f);

        #endregion

        #region Extended Colors

        /// <summary>Gradient start for section headers</summary>
        public static readonly Color GradientStart = new Color(0.12f, 0.14f, 0.18f);

        /// <summary>Gradient end for section headers</summary>
        public static readonly Color GradientEnd = new Color(0.08f, 0.09f, 0.11f);

        /// <summary>Focus/Selected border highlight</summary>
        public static readonly Color FocusBorder = new Color(0.38f, 0.62f, 0.98f, 0.6f);

        /// <summary>Success color - distinct from StateActive</summary>
        public static readonly Color Success = new Color(0.18f, 0.80f, 0.44f);

        /// <summary>Informational color</summary>
        public static readonly Color Info = new Color(0.20f, 0.60f, 0.86f);

        /// <summary>Warning color - distinct from Accent</summary>
        public static readonly Color Warning = new Color(0.95f, 0.61f, 0.07f);

        #endregion

        #region Mode Colors

        /// <summary>Test mode accent - teal</summary>
        public static readonly Color ModeTest = new Color(0.38f, 0.82f, 0.69f);

        /// <summary>Component mode accent - amber</summary>
        public static readonly Color ModeComponent = new Color(0.98f, 0.73f, 0.25f);

        /// <summary>Tag mode accent - green</summary>
        public static readonly Color ModeTag = new Color(0.54f, 0.76f, 0.42f);

        /// <summary>Layer mode accent - red</summary>
        public static readonly Color ModeLayer = new Color(0.94f, 0.36f, 0.36f);

        /// <summary>Viewer mode accent - blue</summary>
        public static readonly Color ModeViewer = new Color(0.38f, 0.62f, 0.98f);

        /// <summary>Style mode accent - purple</summary>
        public static readonly Color ModeStyle = new Color(0.75f, 0.55f, 0.95f);

        /// <summary>Debug mode accent - cyan</summary>
        public static readonly Color ModeDebug = new Color(0.35f, 0.85f, 0.95f);

        /// <summary>Performance mode accent - orange</summary>
        public static readonly Color ModePerformance = new Color(0.98f, 0.55f, 0.25f);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Lighten a color for hover effects
        /// </summary>
        public static Color Lighten(Color color, float amount = 0.12f)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        /// <summary>
        /// Darken a color for pressed effects
        /// </summary>
        public static Color Darken(Color color, float amount = 0.15f)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        /// <summary>
        /// Set alpha of a color
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Generate a stable color from a type name (for type badges)
        /// </summary>
        public static Color GetTypeColor(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return TextMedium;

            int hash = typeName.GetHashCode();
            float hue = Mathf.Abs(hash % 360) / 360f;
            return Color.HSVToRGB(hue, 0.65f, 0.9f);
        }

        #endregion
    }
}
#endif
