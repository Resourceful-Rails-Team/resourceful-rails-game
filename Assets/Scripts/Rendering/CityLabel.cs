using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rails.Data;

namespace Rails.Rendering
{
    public class CityLabel : MonoBehaviour
    {
        public TMP_Text Name;
        public List<Image> GoodsIcons;

        public void Set(Vector3 position, City city)
        {
            transform.position = position;
            Name.text = city.Name;

            int i = 0;
            foreach (Image img in GoodsIcons)
            {
                Good good;
                if (i < city.Goods.Count)
                {
                    good = Manager.Singleton.MapData.Goods[city.Goods[i].x];
                    img.overrideSprite = good.Icon;
                    img.gameObject.SetActive(true);
                }
                else
                    img.gameObject.SetActive(false);
                i++;
            }
        }
    }
}