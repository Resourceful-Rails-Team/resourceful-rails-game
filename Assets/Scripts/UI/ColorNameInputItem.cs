/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the ColorNameInputItem prefab's components into one managed object.
    /// </summary>
    public class ColorNameInputItem : MonoBehaviour
    {
        public Image ColorImage;
        public TMPro.TMP_Text NameText;
        public TMPro.TMP_InputField ValueInput;

        /// <summary>
        /// The item's box's color.
        /// </summary>
        public Color Color { get => ColorImage.color; set => ColorImage.color = value; }

        /// <summary>
        /// The item's rendered name text.
        /// </summary>
        public string Name { get => NameText.text; set => NameText.text = value; }

        /// <summary>
        /// The item's rendered value text.
        /// </summary>
        public string Value { get => ValueInput.text; set => ValueInput.text = value; }

        public event EventHandler<string> OnValueChanged;

        private void Start()
        {
            // add on value changed event listener
            ValueInput.onValueChanged.AddListener((v) => OnValueChanged?.Invoke(this, v));
        }
    }
}
