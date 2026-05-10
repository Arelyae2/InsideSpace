using UnityEditor;
using UnityEngine;

namespace MVsToolkit.Preferences
{
    public static class MVsHierarchyPreferences
    {
        public static MVsHierarchyValues Values;

        static GUIStyle _sectionStyle;
        static GUIStyle _titleStyle;

        [MenuItem("Tools/MVsToolkit/Preferences")]
        public static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences("Preferences/MVs Toolkit/Hierarchy");
        }

        [SettingsProvider]
        public static SettingsProvider CreatePreferencesProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/MVs Toolkit/Hierarchy", SettingsScope.User)
            {
                label = "MVs Hierarchy",

                guiHandler = (searchContext) =>
                {
                    EditorGUI.BeginChangeCheck();
                    DrawHierarchy();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Values.Save();
                    }

                    if (GUILayout.Button("Reset Values"))
                    {
                        bool confirm = EditorUtility.DisplayDialog(
                            "Reset Preferences",
                            "Are you sure you want to reset all MV's Toolkit preferences to their default values?",
                            "Reset",
                            "Cancel"
                        );

                        if (confirm)
                        {
                            Values = new MVsHierarchyValues();
                            Values.Save();
                            Debug.Log("MV's Toolkit preferences reset to default values.");
                        }
                    }
                },

                keywords = new System.Collections.Generic.HashSet<string>(new[] {
                    "mvstoolkit",
                    "toolkit",
                    "toolbox",
                    "hierarchy",
                    "icon" })
            };

            return provider;
        }

        static void DrawHierarchy()
        {
            if (_sectionStyle == null)
            {
                _sectionStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };

                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.UpperLeft
                };
            }

            GUILayout.BeginVertical(_sectionStyle);
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                Values.DrawFolderIcon = EditorGUILayout.Toggle("Draw Folder Icon", Values.DrawFolderIcon);
                Values.DrawFirstComponentIcon = EditorGUILayout.Toggle("Draw First Component Icon", Values.DrawFirstComponentIcon);
                Values.DrawComponentsIcon = EditorGUILayout.Toggle("Draw Components Icon", Values.DrawComponentsIcon);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                Values.DrawZebraMod = EditorGUILayout.Toggle("Draw Zebra Mod", Values.DrawZebraMod);
                Values.DrawChildLines = EditorGUILayout.Toggle("Draw Child Lines", Values.DrawChildLines);
                GUILayout.EndVertical();

                GUILayout.EndVertical();

                GUILayout.BeginVertical(EditorStyles.helpBox);
                Values.ZebraModBlackColor = DrawColorField("Zebra Mod B_Color", Values.ZebraModBlackColor);
                Values.ZebraModWhiteColor = DrawColorField("Zebra Mod W_Color", Values.ZebraModWhiteColor);

                GUILayout.Space(10);
                Values.PrefabColor = DrawColorField("Prefab Color", Values.PrefabColor);
                Values.MissingPrefabColor = DrawColorField("Missing Prefab Color", Values.MissingPrefabColor);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        static string DrawColorField(string label, string hex)
        {
            Color parsed;
            if (!ColorUtility.TryParseHtmlString(hex, out parsed))
                parsed = Color.white;

            Color newColor = EditorGUILayout.ColorField(label, parsed);

            // Retourne un hex propre (#RRGGBB)
            return "#" + ColorUtility.ToHtmlStringRGB(newColor);
        }

        public static MVsHierarchyValues GetValues()
        {
            if(Values == null)
            {
                Values = new MVsHierarchyValues();
                Values.Load();
            }

            return Values;
        }
    }
}