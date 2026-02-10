#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace HelperPlugin
{
    /// <summary>
    /// Reusable UI components for Helper-style EditorWindows.
    /// Provides Material Design inspired controls with consistent styling.
    /// All GUIStyles are cached to avoid GC pressure.
    /// </summary>
    public static class HelperUIComponents
    {
        #region Cached Styles

        private static GUIStyle _headerStyle;
        private static GUIStyle _subHeaderStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _badgeStyle;
        private static GUIStyle _chipStyle;
        private static GUIStyle _searchFieldStyle;
        private static GUIStyle _panelHeaderStyle;
        private static GUIStyle _sectionArrowStyle;
        private static GUIStyle _sectionTitleStyle;
        private static GUIStyle _emptyTitleStyle;
        private static GUIStyle _emptySubtitleStyle;
        private static GUIStyle _buttonTextStyle;
        private static GUIStyle _buttonAtTextStyle;
        private static GUIStyle _tabLabelActiveStyle;
        private static GUIStyle _tabLabelInactiveStyle;
        private static GUIStyle _statLabelStyle;
        private static GUIStyle _statValueStyle;
        private static GUIStyle _chipCountStyle;
        private static GUIStyle _infoLabelStyle;
        private static GUIStyle _infoValueStyle;
        private static GUIStyle _componentNameStyle;
        private static GUIStyle _copyButtonStyle;
        private static GUIStyle _searchClearStyle;
        private static GUIStyle _warningBoxStyle;
        private static GUIStyle _infoBoxStyle;
        private static GUIStyle _statusDotStyle;

        public static GUIStyle HeaderStyle => _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            normal = { textColor = HelperTheme.TextHigh }
        };

        public static GUIStyle SubHeaderStyle => _subHeaderStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            normal = { textColor = HelperTheme.TextMedium }
        };

        public static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = HelperTheme.TextMedium }
        };

        public static GUIStyle ButtonStyle => _buttonStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal
        };

        public static GUIStyle BadgeStyle => _badgeStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = HelperTheme.TextHigh }
        };

        public static GUIStyle ChipStyle => _chipStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            normal = { textColor = HelperTheme.TextHigh }
        };

        public static GUIStyle SearchFieldStyle => _searchFieldStyle ??= new GUIStyle(EditorStyles.toolbarSearchField);

        private static GUIStyle PanelHeaderStyle => _panelHeaderStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = HelperTheme.TextHigh }
        };

        private static GUIStyle SectionArrowStyle => _sectionArrowStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = HelperTheme.TextMedium }
        };

        private static GUIStyle SectionTitleStyle => _sectionTitleStyle ??= new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = HelperTheme.TextHigh }
        };

        private static GUIStyle EmptyTitleStyle => _emptyTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = HelperTheme.TextMedium }
        };

        private static GUIStyle EmptySubtitleStyle => _emptySubtitleStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = HelperTheme.TextLow }
        };

        private static GUIStyle ButtonTextStyle => _buttonTextStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal,
            normal = { textColor = HelperTheme.TextHigh }
        };

        private static GUIStyle ButtonAtTextStyle => _buttonAtTextStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 11,
            normal = { textColor = HelperTheme.TextHigh }
        };

        private static GUIStyle StatLabelStyle => _statLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9,
            normal = { textColor = HelperTheme.TextLow }
        };

        private static GUIStyle ChipCountStyle => _chipCountStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = HelperTheme.TextHigh }
        };

        private static GUIStyle InfoLabelStyle => _infoLabelStyle ??= new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = HelperTheme.TextMedium }
        };

        private static GUIStyle CopyBtnStyle => _copyButtonStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9,
            normal = { textColor = HelperTheme.TextLow }
        };

        private static GUIStyle SearchClearStyle => _searchClearStyle ??= new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 10,
            fontStyle = FontStyle.Bold
        };

        private static GUIStyle StatusDotStyle => _statusDotStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = HelperTheme.TextHigh }
        };

        #endregion

        #region Panel Components

        /// <summary>
        /// Draw a panel header with accent color
        /// </summary>
        public static void DrawPanelHeader(string title, Color accentColor, float height = 28f)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height), GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(rect, HelperTheme.Surface1);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), accentColor);

            var textRect = new Rect(rect.x + 12, rect.y, rect.width - 12, rect.height);
            GUI.Label(textRect, title, PanelHeaderStyle);
        }

        /// <summary>
        /// Draw a section with collapsible header
        /// </summary>
        public static bool DrawSectionHeader(string title, bool isExpanded, Color accentColor)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(24), GUILayout.ExpandWidth(true));

            bool isHover = rect.Contains(Event.current.mousePosition);
            EditorGUI.DrawRect(rect, isHover ? HelperTheme.Surface2 : HelperTheme.Surface1);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), accentColor);

            // Fold arrow
            var arrowRect = new Rect(rect.x + 8, rect.y + 4, 16, 16);
            GUI.Label(arrowRect, isExpanded ? "\u25BC" : "\u25B6", SectionArrowStyle);

            // Title
            var textRect = new Rect(rect.x + 24, rect.y, rect.width - 24, rect.height);
            GUI.Label(textRect, title, SectionTitleStyle);

            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                return !isExpanded;

            return isExpanded;
        }

        /// <summary>
        /// Draw a divider line
        /// </summary>
        public static void DrawDivider(float leftPadding = 8f, float rightPadding = 8f)
        {
            GUILayout.Space(4);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(leftPadding);
                var rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, HelperTheme.Border);
                GUILayout.Space(rightPadding);
            }
            GUILayout.Space(4);
        }

        /// <summary>
        /// Draw empty state message
        /// </summary>
        public static void DrawEmptyState(string title, string subtitle)
        {
            GUILayout.FlexibleSpace();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(title, EmptyTitleStyle);
                    GUILayout.Label(subtitle, EmptySubtitleStyle);
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
        }

        #endregion

        #region Button Components

        /// <summary>
        /// Draw a styled button with hover effects and soft corners
        /// </summary>
        public static bool DrawButton(string text, Color color, float width = 0, float height = 24)
        {
            var options = width > 0
                ? new[] { GUILayout.Width(width), GUILayout.Height(height) }
                : new[] { GUILayout.Height(height), GUILayout.ExpandWidth(true) };

            var rect = GUILayoutUtility.GetRect(GUIContent.none, ButtonStyle, options);

            bool isHover = rect.Contains(Event.current.mousePosition);
            bool isPressed = isHover && Event.current.type == EventType.MouseDown;

            Color bgColor = isPressed ? HelperTheme.Darken(color) : (isHover ? HelperTheme.Lighten(color) : color);

            DrawSoftCornerButton(rect, bgColor, isHover);
            GUI.Label(rect, text, ButtonTextStyle);

            if (isHover)
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }

            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        /// <summary>
        /// Draw a button at specific rect position
        /// </summary>
        public static bool DrawButtonAt(Rect rect, string text, Color color)
        {
            bool isHover = rect.Contains(Event.current.mousePosition);
            bool isPressed = isHover && Event.current.type == EventType.MouseDown;
            Color bgColor = isPressed ? HelperTheme.Darken(color) : (isHover ? HelperTheme.Lighten(color) : color);

            DrawSoftCornerButton(rect, bgColor, isHover);
            GUI.Label(rect, text, ButtonAtTextStyle);

            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        /// <summary>
        /// Draw a button with soft corner effect (simulated rounded corners)
        /// </summary>
        private static void DrawSoftCornerButton(Rect rect, Color bgColor, bool isHover)
        {
            // Main body (slightly inset for corner effect)
            var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
            EditorGUI.DrawRect(innerRect, bgColor);

            // Corner fills with slightly darker color for depth
            var cornerColor = HelperTheme.WithAlpha(bgColor, 0.85f);

            // Top edge
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y, rect.width - 4, 1), cornerColor);
            // Bottom edge
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.yMax - 1, rect.width - 4, 1), cornerColor);
            // Left edge
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2, 1, rect.height - 4), cornerColor);
            // Right edge
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y + 2, 1, rect.height - 4), cornerColor);

            // Highlight on top for 3D effect
            var highlightColor = HelperTheme.WithAlpha(Color.white, isHover ? 0.15f : 0.08f);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + 1, rect.width - 4, 1), highlightColor);

            // Shadow on bottom for depth
            var shadowColor = HelperTheme.WithAlpha(Color.black, 0.15f);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.yMax - 2, rect.width - 4, 1), shadowColor);
        }

        /// <summary>
        /// Draw a mode tab button
        /// </summary>
        public static bool DrawModeTab(string name, bool isActive, Color accentColor, float width = 70)
        {
            var rect = GUILayoutUtility.GetRect(width, 26);

            bool isHover = rect.Contains(Event.current.mousePosition);
            Color bgColor = isActive ? HelperTheme.Surface2 : (isHover ? HelperTheme.Surface1 : HelperTheme.Surface0);

            EditorGUI.DrawRect(rect, bgColor);

            if (isActive)
            {
                // 3px active tab indicator
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 3, rect.width, 3), accentColor);
            }

            var style = isActive ? GetTabStyle(true) : GetTabStyle(false);
            GUI.Label(rect, name, style);

            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        /// <summary>
        /// Draw a mode tab button with count badge
        /// </summary>
        public static bool DrawModeTabWithBadge(string name, bool isActive, Color accentColor, int count, float width = 70)
        {
            var rect = GUILayoutUtility.GetRect(width, 26);

            bool isHover = rect.Contains(Event.current.mousePosition);
            Color bgColor = isActive ? HelperTheme.Surface2 : (isHover ? HelperTheme.Surface1 : HelperTheme.Surface0);

            EditorGUI.DrawRect(rect, bgColor);

            if (isActive)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 3, rect.width, 3), accentColor);
            }

            // Tab name with badge count
            var style = isActive ? GetTabStyle(true) : GetTabStyle(false);
            string label = count > 0 ? $"{name} ({count})" : name;
            GUI.Label(rect, label, style);

            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        private static GUIStyle GetTabStyle(bool isActive)
        {
            if (isActive)
            {
                return _tabLabelActiveStyle ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = HelperTheme.TextHigh }
                };
            }

            return _tabLabelInactiveStyle ??= new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                normal = { textColor = HelperTheme.TextMedium }
            };
        }

        #endregion

        #region Badge & Chip Components

        /// <summary>
        /// Draw a colored badge with number
        /// </summary>
        public static void DrawBadge(int number, Color bgColor, float size = 20)
        {
            var rect = GUILayoutUtility.GetRect(size, size);
            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, number.ToString(), BadgeStyle);
        }

        /// <summary>
        /// Draw a count chip
        /// </summary>
        public static void DrawCountChip(int count, Color bgColor, Color textColor, float width = 45, float height = 24)
        {
            var rect = GUILayoutUtility.GetRect(width, height);

            var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
            EditorGUI.DrawRect(innerRect, bgColor);

            var edgeColor = HelperTheme.WithAlpha(bgColor, 0.85f);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y, rect.width - 4, 1), edgeColor);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.yMax - 1, rect.width - 4, 1), edgeColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2, 1, rect.height - 4), edgeColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y + 2, 1, rect.height - 4), edgeColor);

            ChipCountStyle.normal.textColor = textColor;
            GUI.Label(rect, count.ToString("N0"), ChipCountStyle);
        }

        /// <summary>
        /// Draw a stat badge with label and value
        /// </summary>
        public static void DrawStatBadge(string label, string value, Color accentColor, float width = 60, float height = 32)
        {
            var rect = GUILayoutUtility.GetRect(width, height);

            var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
            EditorGUI.DrawRect(innerRect, HelperTheme.Surface2);

            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y, rect.width - 4, 2), accentColor);

            var edgeColor = HelperTheme.WithAlpha(HelperTheme.Surface2, 0.85f);
            EditorGUI.DrawRect(new Rect(rect.x + 2, rect.yMax - 1, rect.width - 4, 1), edgeColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 2, 1, rect.height - 4), edgeColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y + 2, 1, rect.height - 4), edgeColor);

            GUI.Label(new Rect(rect.x, rect.y + 4, rect.width, 12), label, StatLabelStyle);

            _statValueStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
            _statValueStyle.normal.textColor = accentColor;
            GUI.Label(new Rect(rect.x, rect.y + 14, rect.width, 16), value, _statValueStyle);
        }

        #endregion

        #region Row Components

        /// <summary>
        /// Draw a debug/info row with label and value
        /// </summary>
        public static void DrawInfoRow(string label, string value, Color valueColor, string tooltip = "", float labelWidth = 120)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);

                var labelContent = string.IsNullOrEmpty(tooltip)
                    ? new GUIContent(label)
                    : new GUIContent(label, tooltip);

                GUILayout.Label(labelContent, InfoLabelStyle, GUILayout.Width(labelWidth));

                _infoValueStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
                _infoValueStyle.normal.textColor = valueColor;
                GUILayout.Label(value, _infoValueStyle);
            }
        }

        /// <summary>
        /// Draw an info row with a copy button at the end
        /// </summary>
        public static void DrawInfoRowWithCopy(string label, string value, Color valueColor, string tooltip = "", float labelWidth = 120)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);

                var labelContent = string.IsNullOrEmpty(tooltip)
                    ? new GUIContent(label)
                    : new GUIContent(label, tooltip);

                GUILayout.Label(labelContent, InfoLabelStyle, GUILayout.Width(labelWidth));

                _infoValueStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
                _infoValueStyle.normal.textColor = valueColor;
                GUILayout.Label(value, _infoValueStyle);

                GUILayout.FlexibleSpace();

                DrawCopyButton($"{label}: {value}", 18);
            }
        }

        /// <summary>
        /// Draw a component row with type info
        /// </summary>
        public static void DrawComponentRow(string typeName, int totalCount, int activeCount, int inactiveCount,
            Action onSelectActive, Action onSelectInactive)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                var typeColor = HelperTheme.GetTypeColor(typeName);

                var nameRect = GUILayoutUtility.GetRect(180, 26);
                EditorGUI.DrawRect(nameRect, HelperTheme.Surface1);
                EditorGUI.DrawRect(new Rect(nameRect.x, nameRect.y, 3, nameRect.height), typeColor);

                _componentNameStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
                _componentNameStyle.normal.textColor = typeColor;
                GUI.Label(new Rect(nameRect.x + 8, nameRect.y, nameRect.width - 8, nameRect.height), typeName, _componentNameStyle);

                GUILayout.Space(4);

                DrawCountChip(totalCount, HelperTheme.Surface2, HelperTheme.TextHigh);
                GUILayout.Space(4);

                if (DrawButton(activeCount.ToString(), HelperTheme.Secondary, 45, 24))
                {
                    onSelectActive?.Invoke();
                }

                GUILayout.Space(2);

                if (DrawButton(inactiveCount.ToString(), HelperTheme.TextLow, 45, 24))
                {
                    onSelectInactive?.Invoke();
                }

                GUILayout.Space(8);
            }
            GUILayout.Space(2);
        }

        #endregion

        #region New Components

        /// <summary>
        /// Draw a horizontal progress bar
        /// </summary>
        public static void DrawProgressBar(float value01, Color barColor, float height = 16)
        {
            value01 = Mathf.Clamp01(value01);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height), GUILayout.ExpandWidth(true));

            // Background
            EditorGUI.DrawRect(rect, HelperTheme.Surface1);

            // Fill bar
            if (value01 > 0)
            {
                var fillRect = new Rect(rect.x, rect.y, rect.width * value01, rect.height);
                EditorGUI.DrawRect(fillRect, barColor);
            }

            // Percentage text
            string pct = (value01 * 100f).ToString("F0") + "%";
            GUI.Label(rect, pct, BadgeStyle);
        }

        /// <summary>
        /// Draw a mini FPS line chart using column-based EditorGUI.DrawRect
        /// </summary>
        public static void DrawFPSGraph(float[] history, float width, float height, Color lineColor)
        {
            if (history == null || history.Length == 0) return;

            var rect = GUILayoutUtility.GetRect(width, height);

            // Background
            EditorGUI.DrawRect(rect, HelperTheme.Surface1);

            // Grid lines at 30 and 60 FPS markers
            float maxFps = 120f;
            float y30 = rect.yMax - (30f / maxFps) * rect.height;
            float y60 = rect.yMax - (60f / maxFps) * rect.height;

            var gridColor = HelperTheme.WithAlpha(HelperTheme.Border, 0.5f);
            EditorGUI.DrawRect(new Rect(rect.x, y30, rect.width, 1), gridColor);
            EditorGUI.DrawRect(new Rect(rect.x, y60, rect.width, 1), gridColor);

            // Grid labels
            GUI.Label(new Rect(rect.x + 2, y30 - 7, 30, 14), "30", CopyBtnStyle);
            GUI.Label(new Rect(rect.x + 2, y60 - 7, 30, 14), "60", CopyBtnStyle);

            // Draw columns for each history entry
            int count = history.Length;
            float colWidth = Mathf.Max(1f, rect.width / count);

            for (int i = 0; i < count; i++)
            {
                float fps = Mathf.Clamp(history[i], 0, maxFps);
                float barHeight = (fps / maxFps) * rect.height;
                float x = rect.x + i * colWidth;
                float y = rect.yMax - barHeight;

                // Color based on FPS value
                Color col;
                if (fps >= 60f) col = HelperTheme.StateActive;
                else if (fps >= 30f) col = HelperTheme.Warning;
                else col = HelperTheme.Error;

                EditorGUI.DrawRect(new Rect(x, y, Mathf.Max(colWidth - 1, 1), barHeight), HelperTheme.WithAlpha(col, 0.7f));
            }
        }

        /// <summary>
        /// Draw a styled search field with clear button. Returns true if text changed.
        /// </summary>
        public static bool DrawSearchField(ref string searchText, float width = 200)
        {
            bool changed = false;

            using (new GUILayout.HorizontalScope())
            {
                string prev = searchText;
                searchText = EditorGUILayout.TextField(searchText, SearchFieldStyle, GUILayout.Width(width), GUILayout.Height(20));

                if (prev != searchText)
                    changed = true;

                if (!string.IsNullOrEmpty(searchText))
                {
                    if (GUILayout.Button("X", SearchClearStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        searchText = "";
                        GUI.FocusControl(null);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// Draw a collapsible section with header, fold arrow, and content action.
        /// State is stored per-section in EditorPrefs.
        /// </summary>
        public static bool DrawCollapsibleSection(string title, ref bool isExpanded, Color accentColor, Action drawContent)
        {
            isExpanded = DrawSectionHeader(title, isExpanded, accentColor);

            if (isExpanded)
            {
                GUILayout.Space(2);
                drawContent?.Invoke();
                GUILayout.Space(4);
            }

            return isExpanded;
        }

        /// <summary>
        /// Draw a small copy-to-clipboard button
        /// </summary>
        public static void DrawCopyButton(string textToCopy, float size = 20)
        {
            var rect = GUILayoutUtility.GetRect(size, size);
            bool isHover = rect.Contains(Event.current.mousePosition);

            EditorGUI.DrawRect(rect, isHover ? HelperTheme.Surface3 : HelperTheme.Surface2);

            GUI.Label(rect, "\u2398", CopyBtnStyle);

            if (isHover) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                EditorGUIUtility.systemCopyBuffer = textToCopy;
            }
        }

        /// <summary>
        /// Draw a dot + label indicator for boolean status
        /// </summary>
        public static void DrawStatusIndicator(string label, bool isActive, Color activeColor)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(16);

                // Dot
                var dotRect = GUILayoutUtility.GetRect(8, 8, GUILayout.Width(8));
                dotRect.y += 4;
                EditorGUI.DrawRect(dotRect, isActive ? activeColor : HelperTheme.TextLow);

                GUILayout.Space(6);

                StatusDotStyle.normal.textColor = isActive ? HelperTheme.TextHigh : HelperTheme.TextLow;
                GUILayout.Label(label, StatusDotStyle);
            }
        }

        /// <summary>
        /// Draw a warning box with theme styling
        /// </summary>
        public static void DrawWarningBox(string message)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(28), GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, HelperTheme.WithAlpha(HelperTheme.Warning, 0.12f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), HelperTheme.Warning);

                _warningBoxStyle ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                    normal = { textColor = HelperTheme.Warning }
                };

                GUI.Label(new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height), "\u26A0 " + message, _warningBoxStyle);

                GUILayout.Space(8);
            }
        }

        /// <summary>
        /// Draw an info box with theme styling
        /// </summary>
        public static void DrawInfoBox(string message)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(8);

                var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(28), GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, HelperTheme.WithAlpha(HelperTheme.Info, 0.12f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), HelperTheme.Info);

                _infoBoxStyle ??= new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 11,
                    normal = { textColor = HelperTheme.Info }
                };

                GUI.Label(new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height), "\u2139 " + message, _infoBoxStyle);

                GUILayout.Space(8);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clear cached styles (call on domain reload)
        /// </summary>
        public static void ClearCachedStyles()
        {
            _headerStyle = null;
            _subHeaderStyle = null;
            _labelStyle = null;
            _buttonStyle = null;
            _badgeStyle = null;
            _chipStyle = null;
            _searchFieldStyle = null;
            _panelHeaderStyle = null;
            _sectionArrowStyle = null;
            _sectionTitleStyle = null;
            _emptyTitleStyle = null;
            _emptySubtitleStyle = null;
            _buttonTextStyle = null;
            _buttonAtTextStyle = null;
            _tabLabelActiveStyle = null;
            _tabLabelInactiveStyle = null;
            _statLabelStyle = null;
            _statValueStyle = null;
            _chipCountStyle = null;
            _infoLabelStyle = null;
            _infoValueStyle = null;
            _componentNameStyle = null;
            _copyButtonStyle = null;
            _searchClearStyle = null;
            _warningBoxStyle = null;
            _infoBoxStyle = null;
            _statusDotStyle = null;
        }

        #endregion
    }
}
#endif
