using Rails.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Rails.Controls
{
    public class GameInput : MonoBehaviour
    {
        public enum Context
        {
            Game,
            Popup,
            AllPlayers,
        }

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

        private static bool UsingGamepad => PlayerInput.all[0].currentControlScheme == "Gamepad";
        private static bool _rotateTriggered = false;
        private static Camera _mainCamera;

        #region Input Global Fields 
        public static Context CurrentContext { get; set; } = GameInput.Context.Game;
        public static Vector2 MoveInput { get; private set; }
        public static Vector2 RotateInput { get; private set; }
        public static float ZoomInput { get; private set; }
        public static bool SelectPressed { get; private set; }
        public static bool SelectJustPressed { get; private set; }
        public static NodeId MouseNodeId { get; private set; }
        public static bool DeleteJustPressed { get; private set; }
        public static bool EnterJustPressed { get; private set; }
        public static bool ToggleAllPlayersJustPressed { get; private set; }

        #endregion

        #region Input Events

        private void OnMove(InputValue value)
        {
            if (CurrentContext == Context.Game)
                MoveInput = value.Get<Vector2>();
        }
        private void OnZoom(InputValue value)
        {
            if (CurrentContext == Context.Game)
                ZoomInput = Mathf.Clamp(value.Get<float>(), -1, 1);
        }
        private void OnRotateTriggered(InputValue value)
        {
            if (CurrentContext == Context.Game)
                _rotateTriggered = value.isPressed;
        }
        private void OnRotate(InputValue value)
        {
            if (_rotateTriggered || UsingGamepad)
                RotateInput = value.Get<Vector2>() * new Vector2(1.0f, -1.0f);
        }
        private void OnSelect(InputValue value)
        {
            if (CurrentContext == Context.Game && !IsPointerOverUI())
            {
                SelectPressed = value.isPressed;
                SelectJustPressed = SelectPressed;
            }
        }
        private void OnDelete(InputValue value)
        {
            if(CurrentContext == Context.Game)
                DeleteJustPressed = value.isPressed;
        }            
        private void OnEnter(InputValue value)
        {
            if(CurrentContext == Context.Game)
                EnterJustPressed = value.isPressed;
        }

        private void OnToggleAllPlayersPanel(InputValue value)
        {
            if (CurrentContext == Context.Game || CurrentContext == Context.AllPlayers)
                ToggleAllPlayersJustPressed = value.isPressed;
        }
        
        #endregion

        private void Update()
        {
            Plane plane = new Plane(Vector3.up, 0.0f);
            Ray ray = UsingGamepad ?
                _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f)) :
                _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (IsPointerOverUI() || CurrentContext != Context.Game)
                MouseNodeId = new NodeId(-1, -1);
            else if (plane.Raycast(ray, out float enter))
                MouseNodeId = Utilities.GetNodeId(ray.GetPoint(enter));
        }

        private void LateUpdate()
        {
            SelectJustPressed = false;
            DeleteJustPressed = false;
            EnterJustPressed = false;
            ToggleAllPlayersJustPressed = false;
            if (!UsingGamepad) 
                RotateInput = Vector2.zero;
        }

        private bool IsPointerOverUI()
        {
            var pixelPos = UsingGamepad ?
                (Vector2)_mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)) :
                Mouse.current.position.ReadValue();

            return Utilities.IsPointerOverGameObject(pixelPos);
        }
    }
}
