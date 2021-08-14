/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the EndGamePlayerItem prefab's components into one managed object.
    /// </summary>
    public class EndGamePlayerItem : MonoBehaviour
    {
        public TMPro.TMP_Text NameText;
        public TMPro.TMP_Text MoneyText;
        public TMPro.TMP_Text CitiesText;

        /// <summary>
        /// The item's name text.
        /// </summary>
        public string Name { get => NameText.text; set => NameText.text = value; }

        /// <summary>
        /// Sets the amount of money the player earned.
        /// </summary>
        public int Money { set => MoneyText.text = "$" + value.ToString(); }

        /// <summary>
        /// Sets the number of cities the player earned.
        /// </summary>
        public int Cities { set => CitiesText.text = value.ToString(); }
    }
}
