using Rails.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class GameHUDManager : MonoBehaviour
    {
        [Header("Basic")]
        public TMPro.TMP_Text PlayerNameText;
        public TMPro.TMP_Text PlayerMoneyText;
        public TMPro.TMP_Text PlayerCitiesText;

        [Header("Train")]
        public TMPro.TMP_Text TrainNameText;
        public Image TrainIconImage;
        public TMPro.TMP_Text TrainUpgradeText;
        public Image[] TrainUpgradeImages;

        [Header("Goods")]
        public IconValueItem[] Goods;

        [Header("Cards")]
        public CardItem[] Cards;

        private void Start()
        {
            var manager = Manager.Singleton;
            if (!manager)
                return;

            manager.OnPlayerInfoUpdate += Manager_OnPlayerInfoUpdate;
            Manager_OnPlayerInfoUpdate(manager);
        }

        private void Manager_OnPlayerInfoUpdate(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            // update basic info
            this.PlayerNameText.text = currentPlayer.name;
            this.PlayerMoneyText.text = $"{currentPlayer.money}";
            this.PlayerCitiesText.text = $"{currentPlayer.majorCities}";

            // update train
            this.TrainNameText.text = currentPlayer.trainStyle.ToString();

            // update demand cards
            for (int i = 0; i < Cards.Length; ++i)
            {
                if (i < currentPlayer.demandCards.Count)
                {
                    Cards[i].gameObject.SetActive(true);
                    for (int d = 0; d < currentPlayer.demandCards[i].Length; ++d)
                        Cards[i].SetDemand(d, currentPlayer.demandCards[i][d]);
                }
                else
                {
                    Cards[i].gameObject.SetActive(false);
                }
            }

            // update goods
            SetGoods(currentPlayer.goodsCarried);
        }

        public void SetGoods(IEnumerable<Good> values)
        {
            var valuesArr = values.ToArray();
            for (int i = 0; i < Goods.Length; ++i)
            {
                var good = Goods[i];

                if (i < valuesArr.Length)
                {
                    good.Value = valuesArr[i].Name;
                    good.Sprite = valuesArr[i].Icon;
                }
                else
                {
                    good.Sprite = null;
                    good.Value = null;
                    good.Disabled = true;
                }
            }
        }
    }
}
