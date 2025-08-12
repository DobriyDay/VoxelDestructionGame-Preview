using UnityEngine;

public abstract class BaseWindow : MonoBehaviour
{
    public bool WindowIsActive { get; private set; }
    
    public virtual void Show()
    {
        WindowIsActive = true;
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        WindowIsActive = false;
        gameObject.SetActive(false);
    }
}