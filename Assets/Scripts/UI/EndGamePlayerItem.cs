using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
    public class EndGamePlayerItem : MonoBehaviour
    {
        public TMPro.TMP_Text NameText;
        public TMPro.TMP_Text MoneyText;
        public TMPro.TMP_Text CitiesText;

        public string Name { get => NameText.text; set => NameText.text = value; }
        public int Money { set => MoneyText.text = "$" + value.ToString(); }
        public int Cities { set => CitiesText.text = value.ToString(); }
    }
}
