/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Controls;
using Rails.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Rendering
{
    public class MouseMarker : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 15.0f;
        private Manager _manager;
        private MeshRenderer _meshRenderer;

        void Start()
        {
            _manager = Manager.Singleton;
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (GameInput.MouseNodeId.InBounds && _manager.MapData.Nodes[GameInput.MouseNodeId.GetSingleId()].Type != NodeType.Water)
            {
                _meshRenderer.enabled = true;
                transform.position = Vector3.Slerp(
                    transform.position,
                    Utilities.GetPosition(GameInput.MouseNodeId),
                    _speed * Time.deltaTime
                );
            }
            else
            {
                _meshRenderer.enabled = false;
            }
        }
    }
}
