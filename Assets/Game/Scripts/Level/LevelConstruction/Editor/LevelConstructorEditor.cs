using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class LevelConstructorEditor : EditorWindow
{
    [SerializeField] private LevelDataFile levelData;
    [SerializeField] private Transform root;
    [SerializeField] private Transform levelBounds;
    [SerializeField] private PlayerChunks player;
    [SerializeField] private EnvironmentStyler environmentStyler;

    [MenuItem("Tools/Level Constructor")]
    public static void ShowWindow()
    {
        GetWindow<LevelConstructorEditor>("Level Constructor");
    }

    private void OnGUI()
    {
        levelData = (LevelDataFile)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelDataFile), false);
        root = (Transform)EditorGUILayout.ObjectField("Root", root, typeof(Transform), true);
        levelBounds = (Transform)EditorGUILayout.ObjectField("Level Bounds", levelBounds, typeof(Transform), true);
        player = (PlayerChunks)EditorGUILayout.ObjectField("Player Transform", player, typeof(PlayerChunks), true);
        environmentStyler = (EnvironmentStyler)EditorGUILayout.ObjectField("Environment style", environmentStyler, typeof(Transform), true);

        var player_ = FindFirstObjectByType<PlayerChunks>(FindObjectsInactive.Include);
        if (player_ != null)
            player = player_;
        
        var environmentStyler_ = FindFirstObjectByType<EnvironmentStyler>(FindObjectsInactive.Include);
        if (environmentStyler_ != null)
            environmentStyler = environmentStyler_;
        
        var root_ = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(o => o.name == "LEVEL ROOT");
        if (root_ != null)
            root = root_;
        
        var levelBounds_ = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(o => o.name == "LEVEL BOUNDS");
        if (levelBounds_ != null)
            levelBounds = levelBounds_;
        
        if (levelData == null || root == null || player == null || levelBounds == null || environmentStyler == null)
        {
            EditorGUILayout.HelpBox("Assign LevelData, Environment Styler, Level Bounds, Player and Root first.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Save Level"))
        {
            if (EditorUtility.DisplayDialog("Save Level",
                    "Are you sure you want to save the level? This will overwrite the current LevelData.",
                    "Yes", "No"))
            {
                SaveLevel();
            }
        }

        if (GUILayout.Button("Load Level"))
        {
            LoadLevel();
        }

        if (GUILayout.Button("Clear Objects"))
        {
            if (EditorUtility.DisplayDialog("Clear Objects",
                    "Are you sure you want to clear all objects under the root? This cannot be undone.",
                    "Yes", "No"))
            {
                ClearChildren(root);
            }
        }
    }

    private void SaveLevel()
    {
        var objectGroups = new Dictionary<string, List<Transform>>();

        foreach (Transform child in root)
        {
#if UNITY_EDITOR
            var go = child.gameObject;
            var guid = GetAssetGUID(go);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"GameObject '{go.name}' is not Addressable.");
                continue;
            }

            if (!objectGroups.ContainsKey(guid))
                objectGroups[guid] = new List<Transform>();

            objectGroups[guid].Add(child);
#endif
        }

        var levelObjects = new List<LevelObject>();
        foreach (var kvp in objectGroups)
        {
            bool[] controllables = new bool[kvp.Value.Count];
            bool[] statics = new bool[kvp.Value.Count];
            bool[] staticsUntilTouch = new bool[kvp.Value.Count];
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                if (kvp.Value[i].TryGetComponent(out IControllableObject ctl))
                {
                    controllables[i] = ctl.IsControllable;
                    statics[i] = ctl.IsStatic;
                    staticsUntilTouch[i] = ctl.IsStaticUntilTouch;
                }
            }
            
            var levelObject = new LevelObject
            {
                prefabReference = new AssetReference(kvp.Key),
                isControllable = controllables,
                isStatic = statics,
                isStaticUntilTouch = staticsUntilTouch,
                coordinates = new TransformCoordinates[kvp.Value.Count]
            };

            for (int i = 0; i < kvp.Value.Count; i++)
            {
                var t = kvp.Value[i];
                levelObject.coordinates[i] = new TransformCoordinates
                {
                    position = t.position,
                    rotation = t.rotation,
                    scale = t.localScale
                };
            }

            levelObjects.Add(levelObject);
        }

        levelData.levelObjects = levelObjects.ToArray();
        levelData.environmentStyle = environmentStyler.currentStyle;
        levelData.playerStartPosition = player.transform.position;

        levelData.handsItems = new [] { (int)player.HandsItems[0].itemType, (int)player.HandsItems[1].itemType };
        
        TransformCoordinates[] bounds = new TransformCoordinates[levelBounds.childCount];
        for (int i = 0; i < bounds.Length; i++)
        {
            bounds[i] = new TransformCoordinates
            {
                position = levelBounds.GetChild(i).position,
                rotation = levelBounds.GetChild(i).rotation,
                scale = levelBounds.GetChild(i).localScale
            };
        }
        
        levelData.bounds = bounds;
        
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        Debug.Log("Level saved!");
    }

    private void LoadLevel()
    {
        ClearChildren(root);

        player.transform.position = levelData.playerStartPosition;

        if (levelData.handsItems != null && levelData.handsItems.Length > 0)
        {
            player.HandsItems[0].ChooseItem(levelData.handsItems[0]);
            player.HandsItems[1].ChooseItem(levelData.handsItems[1]);
        }
        
        var bounds = levelData.bounds;
        for (int i = 0; i < bounds.Length; i++)
        {
            levelBounds.GetChild(i).position = bounds[i].position;
            levelBounds.GetChild(i).rotation = bounds[i].rotation;
            levelBounds.GetChild(i).localScale = bounds[i].scale;
        }
        
        environmentStyler.SetStyle(levelData.environmentStyle);
        
        foreach (var obj in levelData.levelObjects)
        {
            bool[] controllables = obj.isControllable;
            bool[] statics = obj.isStatic;
            bool[] staticsUntilTouch = obj.isStaticUntilTouch;
            int index = 0;
#if UNITY_EDITOR
            foreach (var coord in obj.coordinates)
            {
                string guid = obj.prefabReference.AssetGUID;
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    Debug.LogWarning($"Missing prefab for GUID: {guid}");
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
                
                var controllable = instance.GetComponent<IControllableObject>();
                if (controllable != null)
                    controllable.SetUp(controllables[index], statics[index], staticsUntilTouch[index]);
                
                instance.transform.position = coord.position;
                instance.transform.rotation = coord.rotation;
                instance.transform.localScale = coord.scale;
                index++;
            }
        }

        MakeSceneDirty();
#endif
        Debug.Log("Level loaded!");
    }

    private static void MakeSceneDirty()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.IsValid())
            EditorSceneManager.MarkSceneDirty(scene);
    }

    private void ClearChildren(Transform parent)
    {
#if UNITY_EDITOR

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
        
        MakeSceneDirty();
#endif
    }

#if UNITY_EDITOR
    private string GetAssetGUID(GameObject go)
    {
        string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
        return AssetDatabase.AssetPathToGUID(path);
    }
#endif
}
