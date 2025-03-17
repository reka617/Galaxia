using Unity.Netcode;
using UnityEngine;

public class ItemWallet : NetworkBehaviour
{
    public NetworkVariable<int> golds = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Item>(out Item item)) return;

        int goldValue = item.CollectItem();

        if (!IsServer) return;

        golds.Value += goldValue;
    }
    
    public void SpendGold(int costToFire)
    {
        golds.Value -= costToFire;
    }
}
