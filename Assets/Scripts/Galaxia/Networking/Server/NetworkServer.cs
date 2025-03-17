using Unity.Netcode;
using UnityEngine;

public class NetworkServer
{
   private NetworkManager networkManager;

   public NetworkServer (NetworkManager networkManager)
   {
      this.networkManager = networkManager;
      // 연결 승인 콜백 등록
      networkManager.ConnectionApprovalCallback += ApprovalCheck;
   }

   private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
   {
      string payload = System.Text.Encoding.UTF8.GetString(request.Payload); // 요청 데이터를 문자열로 변환
      UserData userData = JsonUtility.FromJson<UserData>(payload); // 요청 데이터를 UserData객체로 변환
      
      Debug.Log("ApprovalCheck: " + userData.userName);

      response.Approved = true; // 연결 승인
      response.CreatePlayerObject = true; // 플레이어 오브젝트 생성, 이부분이 빠질경우 정상적으로 게임씬으로 가더라도 플레이어를 확인 못할수도있음
   }
}
