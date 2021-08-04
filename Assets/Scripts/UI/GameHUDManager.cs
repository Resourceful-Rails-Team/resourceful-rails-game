using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.UI
{
  public class GameHUDManager : MonoBehaviour
  {
    public TMPro.TMP_Text PlayerNameText;
    public TMPro.TMP_Text PlayerMoneyText;
    public TMPro.TMP_Text PlayerTrainText;

    public NameValueItem GoodPrefab;
    public GameObject GoodsRoot;

    public NameCheckItem CityPrefab;
    public GameObject CitiesRoot;

    private List<NameValueItem> _goods = new List<NameValueItem>();
    private List<NameCheckItem> _cities = new List<NameCheckItem>();

    public void SetGoods(Dictionary<string, string> values)
    {
      // destroy existing goods
      foreach (var good in _goods)
        Destroy(good.gameObject);
      _goods.Clear();

      foreach (var kvp in values)
      {
        // instantiate item
        var good = Instantiate(GoodPrefab);
        good.transform.SetParent(GoodsRoot.transform, false);

        // set name and value
        good.Name = kvp.Key;
        good.Value = kvp.Value;

        // add to collection
        _goods.Add(good);
      }
    }

    public void SetCities(Dictionary<string, bool> values)
    {
      // destroy existing cities
      foreach (var city in _cities)
        Destroy(city.gameObject);
      _cities.Clear();

      foreach (var kvp in values)
      {
        // instantiate item
        var city = Instantiate(CityPrefab);
        city.transform.SetParent(CitiesRoot.transform, false);

        // set name and value
        city.Name = kvp.Key;
        city.Checked = kvp.Value;

        // add to collection
        _cities.Add(city);
      }
    }
  }
}
