using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Rendering
{
    public class GameToken : MonoBehaviour
    {
        private MeshRenderer _renderer;
        private Animator _animator;
        private Color _primaryColor;

        private void Awake()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
            _animator = GetComponent<Animator>();
            _primaryColor = _renderer?.material.color ?? Color.white;
        }
        public void SetColor(Color color)
        {
            if (_renderer != null)
                _renderer.material.color = color;
        }

        public void SetPrimaryColor(Color color)
        {
            _primaryColor = color;
            SetColor(_primaryColor);
        }

        public void ResetColor()
        {
            if (_renderer != null)
                _renderer.material.color = _primaryColor;
        }

        public void PlayAnimation(string animName) 
            => _animator?.SetTrigger(animName);
    }
}
