using System;
using UnityEngine;

public class RespawnGold : Item
{
    public event Action<RespawnGold> OnGoldCollected;

    private Vector3 prevPosition;

    private void Update()
    {
        if (prevPosition != transform.position)
        {
            Show(true);
        }

        prevPosition = transform.position;
    }

    public override int CollectItem()
    {
        if (!IsServer)
        {
            Show(false);
            return 0;
        }

        if (alreadyCollected)
            return 0;

        alreadyCollected = true;
        
        OnGoldCollected?.Invoke(this);

        return goldValue;
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
    
}
