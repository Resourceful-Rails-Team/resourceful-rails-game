using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the NameValueItem prefab's components into one managed object.
    /// </summary>
    public class NameValueItem : MonoBehaviour
    {
        public TMPro.TMP_Text NameText;
        public TMPro.TMP_Text ValueText;

        /// <summary>
        /// The item's name text.
        /// </summary>
        public string Name { get => NameText.text; set => NameText.text = value; }

        /// <summary>
        /// The item's value text.
        /// </summary>
        public string Value { get => ValueText.text; set => ValueText.text = value; }
    }
}
