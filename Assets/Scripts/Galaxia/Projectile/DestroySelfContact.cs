using UnityEngine;

public class DestroySelfContact : MonoBehaviour
{
   private void OnTriggerEnter2D(Collider2D other)
   {
      Destroy(gameObject);
   }
}
