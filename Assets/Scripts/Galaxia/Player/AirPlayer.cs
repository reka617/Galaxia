using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class AirPlayer : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName;

    public static event Action<AirPlayer> OnPlayerSpawned;
    public static event Action<AirPlayer> OnPlayerDespawned; 
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public ItemWallet Wallet { get; private set; }
    public override void OnNetworkSpawn()
    {
        // if (IsServer)
        // {
        //     UserData userData =
        //         HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
        //     
        //     PlayerName.Value = userData.userName;
        //     Debug.Log(PlayerName.Value);
        //     
        //     OnPlayerSpawned?.Invoke(this);
        // }
        
        if (IsServer)
        {
            // Null 체크 추가
            if (HostSingleton.Instance != null && 
                HostSingleton.Instance.HostGameManager != null && 
                HostSingleton.Instance.HostGameManager.NetworkServer != null)
            {
                UserData userData =
                    HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            
                // userData도 null 체크
                if (userData != null)
                {
                    PlayerName.Value = userData.userName;
                    Debug.Log(PlayerName.Value);
                }
                else
                {
                    // userData가 null인 경우 대체 이름 사용
                    PlayerName.Value = $"Player {OwnerClientId}";
                    Debug.Log($"Using fallback name for player {OwnerClientId}");
                }
            }
            else
            {
                // 싱글톤 객체가 null인 경우 대체 이름 사용
                PlayerName.Value = $"Player {OwnerClientId}";
                Debug.Log($"HostSingleton not available, using fallback name for player {OwnerClientId}");
            }
        
            // 플레이어 스폰 이벤트는 항상 호출
            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            //우선순위 설정
            cinemachineCamera.Priority = ownerPriority;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
        
    }
}
