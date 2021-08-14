using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
    /// <summary>
    /// Orients the attached gameobject's transform towards the camera.
    /// </summary>
    public class OrientToCamera : MonoBehaviour
    {
        /// <summary>
        /// Whether or not to face the camera's position or match the camera's direction.
        /// </summary>
        public bool Orthogonal;

        /// <summary>
        /// Called once per frame.
        /// </summary>
        void Update()
        {
            // grab camera
            var camera = Camera.main;
            if (camera)
            {
                // orient in direction of camera if perspective
                // orient to same direction as camera if orthogonal
                if (Orthogonal)
                    this.transform.forward = camera.transform.forward;
                else
                    this.transform.LookAt(camera.transform.position);
            }
        }
    }
}
