using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class AirPlayer : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private int ownerPriority = 15;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            //우선순위 설정
            cinemachineCamera.Priority = ownerPriority;
        }
    }
}
