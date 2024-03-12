using UnityEngine;

public abstract class GameObjectBase
{
    public Transform trans;
    public GameObject gobj;
    private bool IsShow = false; 
    
    public bool GetIsShow()
    {
        return IsShow;
    }

    public void Hide()
    {
        IsShow = false;
        trans.localScale = Vector3.zero;
    }

    public void Show()
    {
        IsShow = true;
        trans.localScale = Vector3.one;
    }
}