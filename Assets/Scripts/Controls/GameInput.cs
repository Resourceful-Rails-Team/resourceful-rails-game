using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    #region Singleton
    private static GameInput _singleton;
    private void Awake()
    {
        if(_singleton != null)
            Debug.LogError("Error: more than one GameInput Monobehaviour found. Please have only one per Scene.");
        _singleton = this;
    }
    #endregion

    public static Vector2 MoveInput { get; private set; }
    public static Vector2 RotateInput { get; private set; }
    public static float ZoomInput { get; private set; }
    private static bool _rotateTriggered = false;

    public static bool SelectPressed { get; private set; }
    public static bool SelectJustPressed { get; private set; }

    private void OnMove(InputValue value) => MoveInput = value.Get<Vector2>();
    private void OnZoom(InputValue value) => ZoomInput = Mathf.Clamp(value.Get<float>(), -1, 1);    
    private void OnRotateTriggered(InputValue value) => _rotateTriggered = value.isPressed;
    private void OnRotate(InputValue value)
    {
        if (_rotateTriggered)
            RotateInput = value.Get<Vector2>();
    }
    private void OnSelect(InputValue value)
    {
        SelectPressed = value.isPressed;
        SelectJustPressed = SelectPressed;
    }

    private void LateUpdate()
    {
        ZoomInput = 0.0f;
        SelectJustPressed = false;
    }
}
