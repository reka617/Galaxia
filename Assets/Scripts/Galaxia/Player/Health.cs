using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
   [field: SerializeField] public int MaxHealth { get; private set; } = 100; //최대체력 저장변수

   public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

   private bool isDead;
   public Action<Health> OnDeath;

   public override void OnNetworkSpawn()
   {
      if (!IsServer) return;

      CurrentHealth.Value = MaxHealth;
   }

   public void TakeDamage(int damageV)
   {
      ModifyHealth(-damageV);
   }

   public void RestoreHealth(int healValue)
   {
      ModifyHealth(healValue);
   }

   private void ModifyHealth(int value)
   {
      if (isDead) return;

      int newHealth = CurrentHealth.Value + value;

      CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

      if (CurrentHealth.Value == 0)
      {
         OnDeath?.Invoke(this);
         isDead = true;
      }

   }
}
