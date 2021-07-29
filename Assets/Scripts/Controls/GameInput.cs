using UnityEngine;
using UnityEngine.InputSystem;

namespace Rails.Controls
{
    public class GameInput : MonoBehaviour
    {
        #region Singleton
        private static GameInput _singleton;
        private void Awake()
        {
            if (_singleton != null)
                Debug.LogError("Error: more than one GameInput Monobehaviour found. Please have only one per Scene.");
            _singleton = this;
            _mainCamera = Camera.main;
        }
        #endregion

        public static Vector2 MoveInput { get; private set; }
        public static Vector2 RotateInput { get; private set; }
        public static float ZoomInput { get; private set; }
        public static bool SelectPressed { get; private set; }
        public static bool SelectJustPressed { get; private set; }
        public static NodeId MouseNodeId { get; private set; }

        private static bool _rotateTriggered = false;
        private static Camera _mainCamera;

        #region Input Events
        private void OnMove(InputValue value) => MoveInput = value.Get<Vector2>();
        private void OnZoom(InputValue value) => ZoomInput = Mathf.Clamp(value.Get<float>(), -1, 1);
        private void OnRotateTriggered(InputValue value) => _rotateTriggered = value.isPressed;
        private void OnRotate(InputValue value)
        {
            if (_rotateTriggered)
                RotateInput = value.Get<Vector2>() * new Vector2(1.0f, -1.0f);
        }
        private void OnSelect(InputValue value)
        {
            SelectPressed = value.isPressed;
            SelectJustPressed = SelectPressed;
        }
        #endregion

        private void Update()
        {
            Plane plane = new Plane(Vector3.up, 0.0f);
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (plane.Raycast(ray, out float enter))
                MouseNodeId = Manager.Singleton.GetNodeId(ray.GetPoint(enter));
        }
        private void LateUpdate() => SelectJustPressed = false;
    }
}
