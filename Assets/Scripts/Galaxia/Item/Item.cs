using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Item : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    protected int goldValue = 100;
    protected bool alreadyCollected;

    public abstract int CollectItem();

    public void SetValue(int value)
    {
        goldValue = value;
    }

    protected void Show(bool show)
    {
        spriteRenderer.enabled = show;
    }
}
