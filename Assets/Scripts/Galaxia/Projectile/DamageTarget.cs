using Unity.Netcode;
using UnityEngine;

public class DamageTarget : NetworkBehaviour
{
   [SerializeField] private int damageValue = 3;

   // 데미지를 주는 오브젝트의 클라이언트 아이디를 저장하는 변수
   private ulong ownerClientId;

   //데미지를 주는 오브젝트의 클라이언트 아이디를 설정
   public void SetOwner(ulong ownerClientId)
   {
      this.ownerClientId = ownerClientId;
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.attachedRigidbody == null) return;

      if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObject))
      {
         if (ownerClientId == netObject.OwnerClientId)
         {
            return;
         }
      }

      if (other.attachedRigidbody.TryGetComponent<Health>(out Health health))
      {
         health.TakeDamage(damageValue);
      }
   }
}
