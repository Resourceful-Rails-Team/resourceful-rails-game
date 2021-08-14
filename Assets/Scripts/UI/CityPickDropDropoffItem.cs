using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the CityPickDropDropoffItem prefab's components into one managed object.
    /// </summary>
    public class CityPickDropDropoffItem : MonoBehaviour
    {
        public Toggle Toggle;
        public Image IconImage;
        public TMPro.TMP_Text IconTooltipText;
        public TMPro.TMP_Text CardNumberText;
        public TMPro.TMP_Text MoneyText;

        /// <summary>
        /// Reward displayed on item.
        /// </summary>
        public string Money { get => MoneyText.text; set => MoneyText.text = value; }

        /// <summary>
        /// Sets the card number rendered on item.
        /// </summary>
        public int CardNumber { set => CardNumberText.text = value.ToString(); }

        /// <summary>
        /// Sets the goods icon rendered on item.
        /// </summary>
        public Sprite Icon { get => IconImage.overrideSprite; set => IconImage.overrideSprite = value; }
        
        /// <summary>
        /// Sets the goods icon tooltip text rendered when the user hovers over the goods icon.
        /// </summary>
        public string IconTooltip { get => IconTooltipText.text; set => IconTooltipText.text = value; }
    }
}
