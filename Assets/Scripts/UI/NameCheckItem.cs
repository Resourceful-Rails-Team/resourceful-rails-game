using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the NameCheckItem prefab's components into one managed object.
    /// </summary>
    public class NameCheckItem : MonoBehaviour
    {
        public TMPro.TMP_Text NameText;
        public Image CheckImage;

        /// <summary>
        /// The item's name text.
        /// </summary>
        public string Name { get => NameText.text; set => NameText.text = value; }

        /// <summary>
        /// The item's check state.
        /// </summary>
        public bool Checked { get => CheckImage.enabled; set => CheckImage.enabled = value; }
    }
}
