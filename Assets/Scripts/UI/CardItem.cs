/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    /// <summary>
    /// UI interop class that links the CardItem prefab's components into one managed object.
    /// </summary>
    public class CardItem : MonoBehaviour
    {
        public TMPro.TMP_Text[] IconTooltipTexts;
        public TMPro.TMP_Text[] CityNameTexts;
        public TMPro.TMP_Text[] PriceTexts;
        public Image[] IconImages;

        /// <summary>
        /// Sets the given card to the given demand
        /// </summary>
        public void SetDemand(int index, Rails.Data.Demand demand)
        {
            if (index < 0 || index >= 3)
                return;

            CityNameTexts[index].text = demand.City.Name;
            PriceTexts[index].text = $"${demand.Reward}";
            IconImages[index].overrideSprite = demand.Good.Icon;
            IconTooltipTexts[index].text = demand.Good.Name;
        }
    }
}
