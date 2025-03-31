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

    [SerializeField] private Texture2D crosshair;
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
            UserData userData = null;
            if (IsHost)
            {
                userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                userData = ServerSingleton.Instance.ServerGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }

            PlayerName.Value = userData.userName;
            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            //우선순위 설정
            cinemachineCamera.Priority = ownerPriority;
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width/2, crosshair.height/2), CursorMode.Auto);
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
