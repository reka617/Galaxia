using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Ref")] 
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Setting")] 
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _turnRate = 30f;

    private Vector2 previousMovementInput;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.MoveEvent -= HandleMove;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        float zRotation = previousMovementInput.x * -(_turnRate) * Time.deltaTime;
        _bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        _rigidbody.velocity = (Vector2)_bodyTransform.up * previousMovementInput.y * _moveSpeed;
    }
}
