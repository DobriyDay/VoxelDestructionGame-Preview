using System;
using UnityEngine;

public class HandsItem : MonoBehaviour
{
    public enum ItemType
    {
        None = -1,
        Axe = 0,
        Pickaxe = 1
    }
    
    public ItemType itemType;
    
    [SerializeField] private GameObject[] items;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        ChooseItem();
    }
    #endif

    public void ChooseItem(int itemIndex)
    {
        itemType = (ItemType)itemIndex;
        ChooseItem();
    }
    
    public void ChooseItem()
    {
        for (int i = 0; i < items.Length; i++)
            items[i].gameObject.SetActive(false);
        
        if (itemType != ItemType.None)
            items[(int)itemType].SetActive(true);
    }
}
