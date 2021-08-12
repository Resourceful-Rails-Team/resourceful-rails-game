using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails.Rendering
{
    /// <summary>
    /// An editable representation of any token on the game board.
    /// </summary>
    public class GameToken : MonoBehaviour
    {
        private MeshRenderer[] _detailRenderers;
        private MeshRenderer[] _nodeRenderers;
        private Animator _animator;
        private Color _primaryColor;
        private TMPro.TMP_Text _text;

        private void Awake()
        {
            _detailRenderers = GetComponentsInChildren<MeshRenderer>()
                .Where(r => 
                    !r.gameObject.name.StartsWith("Clear") && 
                    !r.gameObject.name.StartsWith("Mountain")
                ).ToArray();

            _nodeRenderers = GetComponentsInChildren<MeshRenderer>()
                .Where(r => 
                    r.gameObject.name.StartsWith("Clear") ||
                    r.gameObject.name.StartsWith("Mountain")
                ).ToArray();

            _text = GetComponentInChildren<TMPro.TMP_Text>();
            _animator = GetComponent<Animator>();
            _primaryColor = _detailRenderers?.FirstOrDefault()?.material.color ?? Color.white;

            SetColor(_primaryColor);
        }

        /// <summary>
        /// Set the GameToken's mesh color (if it has a mesh)
        /// </summary>
        /// <param name="color">The color to set the mesh</param>
        public void SetColor(Color color)
        {
            if (_detailRenderers != null)
                foreach (var renderer in _detailRenderers)
                {
                    renderer.material.color = color;
                    if (color.a == 0.0f)
                        renderer.enabled = false;
                }

            foreach (var renderer in _nodeRenderers)
                renderer.material.color = Color.black;
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
        public void ResetColor() => SetColor(_primaryColor);

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
