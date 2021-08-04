using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Rendering
{
    /// <summary>
    /// An editable representation of any token on the game board.
    /// </summary>
    public class GameToken : MonoBehaviour
    {
        private MeshRenderer[] _renderers;
        private Animator _animator;
        private Color _primaryColor;
        private TMPro.TMP_Text _text;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _text = GetComponentInChildren<TMPro.TMP_Text>();
            _animator = GetComponent<Animator>();
            _primaryColor = _renderers?[0].material.color ?? Color.white;
        }

        /// <summary>
        /// Set the GameToken's mesh color (if it has a mesh)
        /// </summary>
        /// <param name="color">The color to set the mesh</param>
        public void SetColor(Color color)
        {
            if (_renderers != null)
                foreach(var renderer in _renderers)
                    renderer.material.color = color;
        }

        /// <summary>
        /// Establishes a primary color for a GameToken.
        /// The token resets to this color when `ResetColor` is called.
        /// </summary>
        /// <param name="color">The color to set the primary color to</param>
        public void SetPrimaryColor(Color color)
        {
            _primaryColor = color;
            ResetColor();
        }
        
        /// <summary>
        /// Resets the `GameToken`'s color to its primary color
        /// </summary>
        public void ResetColor()
        {
            if (_renderers != null)
                foreach (var renderer in _renderers)
                    renderer.material.color = _primaryColor;
        }

        /// <summary>
        /// Plays a given animation on the token (if it has an animator).
        /// </summary>
        /// <param name="animName">The trigger associated with the animation</param>
        public void PlayAnimation(string animName) 
            => _animator?.SetTrigger(animName);

        /// <summary>
        /// Sets the token's text rendered above to the given string.
        /// </summary>
        /// <param name="text">String to set the token text to</param>
        public void SetText(string text)
        {
            if (_text)
                _text.text = text;
        }
    }
}
