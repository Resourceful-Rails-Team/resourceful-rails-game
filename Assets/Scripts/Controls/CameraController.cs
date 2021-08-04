using UnityEngine;
using UnityEngine.InputSystem;

namespace Rails.Controls
{
    public class CameraController : MonoBehaviour
    {
        public Transform targetTransform;
        public float focusDistance = 1f;
        public float moveSpeed = 1f;
        public float rotateSpeed = 1f;
        public float zoomSpeed = 1f;

        public float zoomMinDist = 1f;
        public float zoomMaxDist = 10f;

        public float lerpSpeed = 2.0f;

        private Vector3 focus;

        [SerializeField]
        private Transform _markerTransform;
       
        private void Start()
        {
            focus = transform.position;
            focus.y = 0;
            focus.z += focusDistance;

            transform.LookAt(focus);

            targetTransform.position = transform.position;
            targetTransform.rotation = transform.rotation;
        }
        void Update()
        {
            Move(GameInput.MoveInput);
            Rotate(GameInput.RotateInput);
            Zoom(GameInput.ZoomInput);

            transform.position = Vector3.Slerp(transform.position, targetTransform.position, lerpSpeed * Time.deltaTime);
            transform.rotation = targetTransform.rotation; 
        }
        #region Methods

        // Moves the camera across the X-Z plane.
        private void Move(Vector2 input)
        {
            float distance = Vector3.Distance(targetTransform.position, focus);

            Vector3 move = (Vector3.right * input.x * moveSpeed)
                + (Vector3.forward * input.y * moveSpeed);
            Quaternion planarDirection = Quaternion.LookRotation(
              Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);

            move = planarDirection * move * distance * Time.deltaTime;
            focus += move;
            targetTransform.position += move;

            return;
        }

        // Rotates the camera around the focus point.
        private void Rotate(Vector2 input)
        {
            var targetPos = targetTransform.position;

            // Rotate horizontally along vertical axis.
            float rot = rotateSpeed * input.x * Time.deltaTime;
            targetTransform.RotateAround(focus, Vector3.up, rot);

            // Rotate vertically along side axis.
            float dot = Vector3.Dot(Vector3.down, targetTransform.forward);
            if ((dot < 0.25f && input.y < 0f) || (dot >= 0.99f && input.y > 0f))
                return;

            rot = rotateSpeed * input.y * Time.deltaTime;
            targetTransform.RotateAround(focus, targetTransform.right, rot);

            transform.position += targetTransform.position - targetPos;
            
            return;
        }

        // Zooms the camera towards or away from the focus point.
        private void Zoom(float input)
        {
            float distance = Vector3.Distance(targetTransform.position, focus);
            float zoomDelta = zoomSpeed * input * distance  * Time.deltaTime;

            distance = Mathf.Clamp(distance - zoomDelta, zoomMinDist, zoomMaxDist);
            targetTransform.position = focus - (targetTransform.forward * distance);
        }
        #endregion
    }
}