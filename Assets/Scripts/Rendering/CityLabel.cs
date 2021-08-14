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
        //public Canvas Base;
        public TMP_Text Name;
        public List<Image> GoodsIcons;

        public void Set(Vector3 position, City city)
        {
            transform.position = position;
            Name.text = city.Name;
            GoodsIcons = new List<Image>();
            foreach (Vector2Int g in city.Goods)
            {
                Good good;
                Image icon;

                good = Manager.Singleton.MapData.Goods[g.x];
                icon = null;

                GoodsIcons.Add(icon);
            }
        }
    }
}