using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender
{
    public static class ForceReserializeAssetsUtils
    {
        public static void ForceReserializeAllAssets()
        {
            if (!EditorUtility.DisplayDialog("Attention", "Do you want to force reserialize all assets? This can be time heavy operation and result in massive list of changes.", "Ok", "Cancel"))
            {
                return;
            }

            AssetDatabase.ForceReserializeAssets();
        }

        [MenuItem("Assets/Reserialize Assets", true)]
        static public bool ForceReserializeSelectedAssetsValidate() => Selection.assetGUIDs.Length > 0;

        [MenuItem("Assets/Reserialize Assets")]
        public static void ForceReserializeSelectedAssets()
        {
            // 获取当前选中的资源对象数组
            string[] selectedAssetGUIDs = Selection.assetGUIDs;
            if (selectedAssetGUIDs.Length == 0)
            {
                EditorUtility.DisplayDialog("Attention", "No assets are selected.", "Ok");
                return;
            }

            var assetPathSet = new HashSet<string>();
            // 遍历选中的资源对象
            foreach (var selectedAssetGUID in selectedAssetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGUID);
                var selectedObject = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                // 检查是否是文件夹类型资源
                if (selectedObject is DefaultAsset folder)
                {
                    // 获取文件夹下的所有资源路径
                    string[] assetGUIDs = AssetDatabase.FindAssets("", new[] { AssetDatabase.GetAssetPath(folder) });

                    // 遍历文件夹下的所有资源
                    foreach (string assetGUID in assetGUIDs)
                    {
                        assetPathSet.Add(AssetDatabase.GUIDToAssetPath(assetGUID));
                    }
                }
                else
                {
                    assetPathSet.Add(AssetDatabase.GetAssetPath(selectedObject));
                }
            }

            AssetDatabase.ForceReserializeAssets(assetPathSet, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
        }
    }
}