﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;


[Serializable]
internal class ToolbarSceneSelection : BaseToolbarElement
{
    public class GroupData
    {
        public string Path;
        public string Prefix;
        public List<string> Group;
    }

    public List<GroupData> GroupDataList = new List<GroupData>
    {
        new GroupData{Path = "Assets/Plugins", Prefix = "Plugin", Group = new List<string>{ } },
        new GroupData{Path = "Assets/3rd", Prefix = "3rd", Group = new List<string>{ } },
        new GroupData{Path = "Assets/Scenes", Prefix = "Scenes", Group = new List<string>{ "map", "game" } },
        new GroupData{Path = "", Prefix = "Other", Group = new List<string>{ } },
    };

    public override string NameInList => "[Dropdown] Scene selection";

    [SerializeField] bool showSceneFolder = true;

    SceneData[] scenesPopupDisplay;
    string[] scenesPath;
    string[] scenesBuildPath;
    int selectedSceneIndex;

    List<SceneData> toDisplay = new List<SceneData>();
    string[] sceneGuids;
    Scene activeScene;
    int usedIds;
    string name;
    GUIContent content;
    bool isPlaceSeparator;

    public override void Init()
    {
        RefreshScenesList();
        EditorSceneManager.sceneOpened -= HandleSceneOpened;
        EditorSceneManager.sceneOpened += HandleSceneOpened;
    }

    protected override void OnDrawInList(Rect position)
    {
        position.width = 200.0f;
        showSceneFolder = EditorGUI.Toggle(position, "Group by folders", showSceneFolder);
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
        DrawSceneDropdown();
        EditorGUI.EndDisabledGroup();
    }

    private void DrawSceneDropdown()
    {
        selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, scenesPopupDisplay.Select(e => e.popupDisplay).ToArray(), GUILayout.Width(WidthInToolbar));

        if (GUI.changed && 0 <= selectedSceneIndex && selectedSceneIndex < scenesPopupDisplay.Length)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                foreach (var scenePath in scenesPath)
                {
                    if ((scenePath) == scenesPopupDisplay[selectedSceneIndex].path)
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        break;
                    }
                }
            }
        }

    }

    void RefreshScenesList()
    {
        InitScenesData();

        //Scenes in build settings
        for (int i = 0; i < scenesBuildPath.Length; ++i)
        {
            AddScene(scenesBuildPath[i]);
        }

        //Scenes on Assets/Scenes/
        isPlaceSeparator = false;
        for (int i = 0; i < scenesPath.Length; ++i)
        {
            if (scenesPath[i].Contains("Assets/Scenes"))
            {
                PlaceSeperatorIfNeeded();
                AddScene(scenesPath[i]);
            }
        }

        //All other scenes.
        isPlaceSeparator = false;
        for (int i = 0; i < scenesPath.Length; ++i)
        {
            PlaceSeperatorIfNeeded();
            AddScene(scenesPath[i]);
        }

        scenesPopupDisplay = toDisplay.ToArray();
    }

    void AddScene(string path)
    {
        if (!path.Contains(".unity"))
            path += ".unity";

        if (toDisplay.Find(data => path == data.path) != null)
            return;

        if (showSceneFolder)
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(path));
            name = $"{folderName}/{GetSceneName(path)}";
        }
        else
        {
            name = GetSceneName(path);
        }

        if (scenesBuildPath.Contains(path))
            content = new GUIContent(name, EditorGUIUtility.Load("BuildSettings.Editor.Small") as Texture, "Open scene");
        else
            content = new GUIContent(name, "Open scene");

        toDisplay.Add(new SceneData()
        {
            path = path,
            popupDisplay = content,
        });

        if (selectedSceneIndex == -1 && Path.GetFileNameWithoutExtension(path) == activeScene.name)
            selectedSceneIndex = usedIds;
        ++usedIds;
    }

    void PlaceSeperatorIfNeeded()
    {
        if (!isPlaceSeparator)
        {
            isPlaceSeparator = true;
            PlaceSeperator();
        }
    }

    void PlaceSeperator()
    {
        toDisplay.Add(new SceneData()
        {
            path = "\0",
            popupDisplay = new GUIContent("\0"),
        });
        ++usedIds;
    }

    void HandleSceneOpened(Scene scene, OpenSceneMode mode)
    {
        RefreshScenesList();
    }

    string GetSceneName(string path)
    {
        foreach (var groupData in GroupDataList)
        {
            if (!path.Contains(groupData.Path))
                continue;

            string groupName = string.Empty;
            foreach (var group in groupData.Group)
            {
                if (path.Contains(group))
                {
                    groupName = $"/{group}";
                    break;
                }
            }

            return $"{groupData.Prefix}{groupName}/{Path.GetFileNameWithoutExtension(path)}";
        }

        return null;
    }

    void InitScenesData()
    {
        toDisplay.Clear();
        selectedSceneIndex = -1;
        scenesBuildPath = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

        sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });
        scenesPath = new string[sceneGuids.Length];
        for (int i = 0; i < scenesPath.Length; ++i)
            scenesPath[i] = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

        activeScene = SceneManager.GetActiveScene();
        usedIds = 0;
    }

    class SceneData
    {
        public string path;
        public GUIContent popupDisplay;
    }
}
