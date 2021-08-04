using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
  public class OrientToCamera : MonoBehaviour
  {
    public bool Orthogonal;

    // Update is called once per frame
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
