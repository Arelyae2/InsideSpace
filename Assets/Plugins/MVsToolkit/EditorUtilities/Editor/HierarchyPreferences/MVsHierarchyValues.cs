using System.IO;
using UnityEngine;

namespace MVsToolkit.Preferences
{
    public class MVsHierarchyValues
    {
        // ===Hierarchy===
        public bool DrawFolderIcon = true;
        public bool DrawFirstComponentIcon = true;
        public bool DrawComponentsIcon = true;

        public bool DrawZebraMod = true;
        public bool DrawChildLines = true;

        //---

        public string ZebraModBlackColor = "#353535";
        public string ZebraModWhiteColor = "#BFBFBF";
        public string PrefabColor = "#8CC7FF";
        public string MissingPrefabColor = "#FF6767";

        private static readonly string FilePath =
            Path.Combine("ProjectSettings", "MVsToolkit_Preferences_Hierarchy.json");

        public void Save()
        {
            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(FilePath, json);
        }

        public MVsHierarchyValues Load()
        {
            string json;

            if (!File.Exists(FilePath))
            {
                json = JsonUtility.ToJson(new(), true);
                File.WriteAllText(FilePath, json);

                Debug.Log("[MV's Toolkit] No preferences file found, creating new save");
                return new MVsHierarchyValues();
            }

            json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<MVsHierarchyValues>(json);
        }
    }
}