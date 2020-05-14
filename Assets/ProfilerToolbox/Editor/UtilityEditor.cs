namespace ProfilerToolbox 
{
    using UnityEngine;
    using UnityEngine.Assertions;

    using UnityEditor;
    using System;
    using System.Collections.Generic;

    public static class UtilityEditor
    {
        static Dictionary<string, GUIContent> s_GUIContentCache;

        /// <summary>
        /// Gets a <see cref="GUIContent"/> for the given label and tooltip. These are recycled
        /// internally and help reduce the garbage collector pressure in the editor.
        /// </summary>
        /// <param name="textAndTooltip">The label and tooltip separated by a <c>|</c>
        /// character</param>
        /// <returns>A recycled <see cref="GUIContent"/></returns>
        public static GUIContent GetContent(string textAndTooltip)
        {
            if (string.IsNullOrEmpty(textAndTooltip))
                return GUIContent.none;

            GUIContent content;

            if (!s_GUIContentCache.TryGetValue(textAndTooltip, out content))
            {
                var s = textAndTooltip.Split('|');
                content = new GUIContent(s[0]);

                if (s.Length > 1 && !string.IsNullOrEmpty(s[1]))
                    content.tooltip = s[1];

                s_GUIContentCache.Add(textAndTooltip, content);
            }

            return content;
        }


        /// <summary>
        /// Draws a UI box with a description and a "Fix Me" button next to it.
        /// </summary>
        /// <param name="text">The description</param>
        /// <param name="action">The action to execute when the button is clicked</param>
        public static void DrawFixMeBox(string text, Action action)
        {
            Assert.IsNotNull(action);

            EditorGUILayout.HelpBox(text, MessageType.Warning);

            GUILayout.Space(-32);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Fix", GUILayout.Width(60)))
                    action();

                GUILayout.Space(8);
            }
            GUILayout.Space(11);
        }

        /// <summary>
        /// Draws a horizontal split line.
        /// </summary>
        public static void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);

            // Splitter rect should be full-width
            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, Styling.splitter);
        }

        /// <summary>
        /// Draws a header label.
        /// </summary>
        /// <param name="title">The label to display as a header</param>
        public static void DrawHeader(string title)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            //labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

            // Title
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
        }

        internal static bool DrawHeader(string title, bool state)
        {
            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

            // Title
            EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

            // Foldout
            state = GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);

            var e = Event.current;
            if (e.type == EventType.MouseDown && backgroundRect.Contains(e.mousePosition) && e.button == 0)
            {
                state = !state;
                e.Use();
            }

            return state;
        }

        //internal static bool DrawHeader(string title, SerializedProperty group, SerializedProperty activeField, EffectSetting target, Action resetAction, Action removeAction)
        //{
        //    Assert.IsNotNull(group);
        //    Assert.IsNotNull(activeField);
        //    Assert.IsNotNull(target);

        //    var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

        //    var labelRect = backgroundRect;
        //    labelRect.xMin += 32f;
        //    labelRect.xMax -= 20f;

        //    var foldoutRect = backgroundRect;
        //    foldoutRect.y += 1f;
        //    foldoutRect.width = 13f;
        //    foldoutRect.height = 13f;

        //    var toggleRect = backgroundRect;
        //    toggleRect.x += 16f;
        //    toggleRect.y += 2f;
        //    toggleRect.width = 13f;
        //    toggleRect.height = 13f;

        //    var menuIcon = Styling.paneOptionsIcon;
        //    var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);

        //    // Background rect should be full-width
        //    backgroundRect.xMin = 0f;
        //    backgroundRect.width += 4f;

        //    // Background
        //    EditorGUI.DrawRect(backgroundRect, Styling.headerBackground);

        //    // Title
        //    using (new EditorGUI.DisabledScope(!activeField.boolValue))
        //        EditorGUI.LabelField(labelRect, GetContent(title), EditorStyles.boldLabel);

        //    // foldout
        //    group.serializedObject.Update();
        //    group.isExpanded = GUI.Toggle(foldoutRect, group.isExpanded, GUIContent.none, EditorStyles.foldout);
        //    group.serializedObject.ApplyModifiedProperties();

        //    // Active checkbox
        //    activeField.serializedObject.Update();
        //    activeField.boolValue = GUI.Toggle(toggleRect, activeField.boolValue, GUIContent.none, Styling.smallTickbox);
        //    activeField.serializedObject.ApplyModifiedProperties();

        //    // Dropdown menu icon
        //    GUI.DrawTexture(menuRect, menuIcon);

        //    // Handle events
        //    var e = Event.current;

        //    if (e.type == EventType.MouseDown)
        //    {
        //        if (menuRect.Contains(e.mousePosition))
        //        {
        //            ShowHeaderContextMenu(new Vector2(menuRect.x, menuRect.yMax), target, resetAction, removeAction);
        //            e.Use();
        //        }
        //        else if (labelRect.Contains(e.mousePosition))
        //        {
        //            if (e.button == 0)
        //                group.isExpanded = !group.isExpanded;
        //            else
        //                ShowHeaderContextMenu(e.mousePosition, target, resetAction, removeAction);

        //            e.Use();
        //        }
        //    }

        //    return group.isExpanded;
        //}

        //static void ShowHeaderContextMenu(Vector2 position, EffectSetting target, Action resetAction, Action removeAction)
        //{
        //    Assert.IsNotNull(resetAction);
        //    Assert.IsNotNull(removeAction);

        //    var menu = new GenericMenu();
        //    menu.AddItem(GetContent("Reset"), false, () => resetAction());
        //    menu.AddItem(GetContent("Remove"), false, () => removeAction());
        //    menu.AddSeparator(string.Empty);
        //    menu.AddItem(GetContent("Copy Settings"), false, () => CopySettings(target));

        //    if (CanPaste(target))
        //        menu.AddItem(GetContent("Paste Settings"), false, () => PasteSettings(target));
        //    else
        //        menu.AddDisabledItem(GetContent("Paste Settings"));

        //    menu.DropDown(new Rect(position, Vector2.zero));
        //}
    }
}
