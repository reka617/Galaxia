using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
   [SerializeField] private GameObject spawnObject;

   private void OnDestroy()
   {
      Instantiate(spawnObject, transform.position, Quaternion.identity);
   }
}
