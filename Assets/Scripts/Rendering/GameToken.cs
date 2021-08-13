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
        // The detail renderers - can change color
        private MeshRenderer[] _detailRenderers;
        // The node renderers - these represent Clear and Mountain nodes
        // which are always black
        private MeshRenderer[] _nodeRenderers;
        // The animator for the Token
        private Animator _animator;
        private TMPro.TMP_Text _text;

        private void Awake()
        {
            // Assign the detail renderers to all non-clear / mountain nodes
            _detailRenderers = GetComponentsInChildren<MeshRenderer>()
                .Where(r => 
                    !r.gameObject.name.StartsWith("Clear") && 
                    !r.gameObject.name.StartsWith("Mountain")
                ).ToArray();
            
            // Assign the node renderers to all clear / mountain nodes
            _nodeRenderers = GetComponentsInChildren<MeshRenderer>()
                .Where(r => 
                    r.gameObject.name.StartsWith("Clear") ||
                    r.gameObject.name.StartsWith("Mountain")
                ).ToArray();

            _text = GetComponentInChildren<TMPro.TMP_Text>();
            _animator = GetComponent<Animator>();

            // Setup the primary color - grabbing the color of the material if possible
            PrimaryColor = _detailRenderers?.FirstOrDefault()?.material.color ?? Color.white;     
        }

        private Color _color;

        /// <summary>
        /// The current Color of the GameToken.
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                if (_detailRenderers != null)
                    foreach (var renderer in _detailRenderers)
                    {
                        renderer.material.color = value;
                        if (value.a == 0.0f)
                            renderer.enabled = false;
                    }

                foreach (var renderer in _nodeRenderers)
                    renderer.material.color = Color.black;

                _color = value;
            }
        }

        private Color _primaryColor;
        /// <summary>
        /// The default Color of the GameToken. When ResetColor is called
        /// the GameToken reverts back to this Color.
        /// </summary>
        public Color PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Resets the `GameToken`'s color to its primary color
        /// </summary>
        public void ResetColor() => Color = _primaryColor;

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
