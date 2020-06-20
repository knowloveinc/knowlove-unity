using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Window : MonoBehaviour
{
    public virtual void Show()
    {
        if (!this.gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        if(this.gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
