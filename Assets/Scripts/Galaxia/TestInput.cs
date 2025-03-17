using UnityEngine;

public class TestInput : MonoBehaviour
{
    [SerializeField] public InputReader inputReader;
    private void Start()
    {
        inputReader.MoveEvent += HandleMove;
    }

    private void OnDestroy()
    {
        inputReader.MoveEvent -= HandleMove;
        inputReader.DisableAllInputs();
    }


    private void HandleMove(Vector2 movement)
    {
        Debug.Log(movement);
    }
}
