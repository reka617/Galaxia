using Unity.Netcode;
using UnityEngine;

public class ConnetctedToServer : MonoBehaviour
{
   public void ConnectedToServer()
   {
      NetworkManager.Singleton.StartClient();
   }
}
