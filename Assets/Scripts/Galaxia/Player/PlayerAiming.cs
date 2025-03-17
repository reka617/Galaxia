using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform weaponTransform;

    private Camera mainCamera;

    private void OnEnable()
    {
        FindCamera();
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        
        // 카메라가 null이면 다시 찾기 시도
        if (mainCamera == null)
        {
            FindCamera();
            // 여전히 null이면 이 프레임은 건너뛰기
            if (mainCamera == null) return;
        }

        Vector2 aimScreenPosition = inputReader.AimPosition;

        Vector3 screenPoint = new Vector3(
            aimScreenPosition.x,
            aimScreenPosition.y,
            -mainCamera.transform.position.z
        );
        
        
        Vector3 aimWorldPosition = mainCamera.ScreenToWorldPoint(screenPoint);

        weaponTransform.up = new Vector2(
            aimWorldPosition.x - weaponTransform.position.x,
            aimWorldPosition.y - weaponTransform.position.y);
    }
    private void FindCamera()
    {
        mainCamera = Camera.main;
        
        // 만약 Camera.main이 없다면 태그로 찾아보기
        if (mainCamera == null)
        {
            var cams = FindObjectsOfType<Camera>();
            if (cams.Length > 0)
            {
                mainCamera = cams[0]; // 첫 번째 카메라 사용
                Debug.Log("메인 카메라를 찾을 수 없어 첫 번째 카메라를 사용합니다.");
            }
            else
            {
                Debug.LogWarning("씬에 카메라가 없습니다!");
            }
        }
    }
}