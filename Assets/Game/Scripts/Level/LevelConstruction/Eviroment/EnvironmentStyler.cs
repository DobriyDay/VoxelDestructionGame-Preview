using UnityEngine;

public enum EnvironmentStyle
{
    Style1 = 0,
    Style2 = 1,
    Style3 = 2,
    Style4 = 3,
}

[System.Serializable]
public class EnvironmentStyleObjects
{
    public EnvironmentStyle style = EnvironmentStyle.Style1;
    public Sprite bgSprite;
}

public class EnvironmentStyler : MonoBehaviour
{
    [Header("Current Style")]
    public EnvironmentStyle currentStyle;

    [Header("Styles settings")]
    [SerializeField] private EnvironmentStyleObjects[] environmentStyleObjects;
    [SerializeField] private SpriteRenderer[] bgRenderer;

    public void SetStyle(EnvironmentStyle style)
    {
        foreach (var styleObject in environmentStyleObjects)
        {
            if (styleObject.style == style)
            {
                foreach (var renderer in bgRenderer)
                    renderer.sprite = styleObject.bgSprite;

                return;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetStyle(currentStyle);
    }
#endif
}


