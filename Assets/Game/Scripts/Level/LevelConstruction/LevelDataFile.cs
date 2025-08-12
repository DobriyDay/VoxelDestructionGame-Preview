using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class TransformCoordinates
{
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
    public Vector3 scale = Vector3.one;
}

[System.Serializable]
public class LevelObject
{
    public AssetReference prefabReference;
    public bool[] isControllable;
    public bool[] isStatic;
    public bool[] isStaticUntilTouch;
    public TransformCoordinates[] coordinates;
}

[CreateAssetMenu(fileName = "LevelDataFile", menuName = "Data/New Level", order = 0)]
public class LevelDataFile : ScriptableObject
{
    public bool IsRewardedLevel = false;
    
    [Header("Level Data")]
    public Vector3 playerStartPosition;
    public int[] handsItems;
    public LevelObject[] levelObjects;
    
    public TransformCoordinates[] bounds;
    public EnvironmentStyle environmentStyle = EnvironmentStyle.Style1;
}