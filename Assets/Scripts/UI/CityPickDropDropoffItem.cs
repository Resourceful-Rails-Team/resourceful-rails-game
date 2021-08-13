using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class CityPickDropDropoffItem : MonoBehaviour
    {
        public Toggle Toggle;
        public Image IconImage;
        public TMPro.TMP_Text IconTooltipText;
        public TMPro.TMP_Text CardNumberText;
        public TMPro.TMP_Text MoneyText;

        public string Money { get => MoneyText.text; set => MoneyText.text = value; }
        public int CardNumber { set => CardNumberText.text = value.ToString(); }
        public Sprite Icon { get => IconImage.overrideSprite; set => IconImage.overrideSprite = value; }
        public string IconTooltip { get => IconTooltipText.text; set => IconTooltipText.text = value; }
    }
}
