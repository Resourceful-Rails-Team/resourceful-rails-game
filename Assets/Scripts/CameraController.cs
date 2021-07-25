using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rails
{
    public class CameraController : MonoBehaviour
    {
        public float Speed = 1f;

        private Vector2 _moveInput = Vector2.zero;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var move = (Vector3.right * _moveInput.x * Speed)
                + (Vector3.forward * _moveInput.y * Speed);
            var planarDirection = Quaternion.LookRotation(Vector3.ProjectOnPlane(this.transform.forward, Vector3.up), Vector3.up);

            this.transform.position += planarDirection * move * Time.deltaTime;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }
}
