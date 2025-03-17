using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class HealthDisplay : NetworkBehaviour
{
   [Header("References")] 
   [SerializeField] private Health health;
   [SerializeField] private Image hpBar;

   public override void OnNetworkSpawn()
   {
      if (!IsClient) return;

      health.CurrentHealth.OnValueChanged += OnHealthChanged;
      OnHealthChanged(0, health.CurrentHealth.Value);
   }

   public override void OnNetworkDespawn()
   {
      if (!IsClient) return;

      health.CurrentHealth.OnValueChanged -= OnHealthChanged;
   }

   private void OnHealthChanged(int oldHealth, int newHealth)
   {
      hpBar.fillAmount = (float)newHealth / health.MaxHealth;
   }
}
