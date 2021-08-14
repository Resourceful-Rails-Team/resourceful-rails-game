using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the IconValueItem prefab's components into one managed object.
    /// </summary>
    public class IconValueItem : MonoBehaviour
    {
        public Image IconImage;
        public TMPro.TMP_Text ValueText;

        /// <summary>
        /// The item's icon/sprite.
        /// </summary>
        public Sprite Sprite { get => IconImage.overrideSprite; set => IconImage.overrideSprite = value; }

        /// <summary>
        /// The item's value text.
        /// </summary>
        public string Value { get => ValueText.text; set => ValueText.text = value; }

        /// <summary>
        /// Whether the item is enabled or disabled.
        /// </summary>
        public bool Disabled { get => !IconImage.enabled; set { IconImage.enabled = !value; ValueText.color = value ? Color.grey : _startColor; } }

        /// <summary>
        /// The color of the text at the start.
        /// </summary>
        private Color _startColor;

        /// <summary>
        /// Triggered on start.
        /// </summary>
        public void Start()
        {
            // get the text color at the start
            _startColor = ValueText.color;
        }
    }
}
