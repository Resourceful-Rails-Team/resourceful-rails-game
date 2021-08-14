using Assets.Scripts.Data;
using Rails.Data;
using Rails.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class PlayerInfoItem : MonoBehaviour
    {
        [Header("Basic")]
        public TMPro.TMP_Text PlayerNameText;
        public TMPro.TMP_Text PlayerMoneyText;
        public TMPro.TMP_Text PlayerCitiesText;

        [Header("Train")]
        public TMPro.TMP_Text TrainNameText;
        public Image TrainIconImage;
        public TMPro.TMP_Text TrainUpgradeTextUpper;
        public TMPro.TMP_Text TrainUpgradeTextLower;

        [Header("Goods")]
        public IconValueItem[] Goods;

        [Header("Cards")]
        public CardItem[] Cards;

        private void Start() => PathPlanner.OnCurrentCostChange += () => UpdateInfo(Manager.Singleton.Player);

        public void UpdateInfo(PlayerInfo player)
        {
            var manager = Manager.Singleton;

            // update basic info
            this.PlayerNameText.text = player.name;
            this.PlayerMoneyText.text = $"${player.money}" + (PathPlanner.CurrentCost != 0 ? $"<color=red> - {PathPlanner.CurrentCost}</color>" : "");
            this.PlayerCitiesText.text = $"{player.majorCities}";

            // update train
            var trainSpecs = manager.Rules.TrainSpecs[player.trainType];
            this.TrainNameText.text = player.trainType.ToString();
            this.TrainUpgradeTextUpper.text = $"{player.movePointsLeft}";
            this.TrainUpgradeTextLower.text = $"{trainSpecs.movePoints}";

            // update demand cards
            for (int i = 0; i < Cards.Length; ++i)
            {
                if (i < player.demandCards.Count)
                {
                    Cards[i].gameObject.SetActive(true);
                    var demandCard = player.demandCards[i];
                    for (int d = 0; d < demandCard.Count(); d++)
                    {
                        Cards[i].SetDemand(d, player.demandCards[i][d]);
                    }
                }
                else
                {
                    Cards[i].gameObject.SetActive(false);
                }
            }

            // update goods
            SetGoods(player.goodsCarried);
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
