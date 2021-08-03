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

        private void Start() => _manager = Manager.Singleton;

        void Update()
        {
            if (GameInput.MouseNodeId.InBounds && _manager.MapData.Nodes[GameInput.MouseNodeId.GetSingleId()].Type != NodeType.Water)
            {
                transform.position = Vector3.Slerp(
                    transform.position,
                    Utilities.GetPosition(GameInput.MouseNodeId),
                    _speed * Time.deltaTime
                );
            }
        }
    }
}
