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

            manager.OnTurnEnd += Manager_OnTurnEnd;
            Manager_OnTurnEnd(manager);
        }

        private void Manager_OnTurnEnd(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            // update basic info
            this.PlayerNameText.text = currentPlayer.name;
            this.PlayerMoneyText.text = $"${currentPlayer.money}";
            this.PlayerCitiesText.text = $"{currentPlayer.majorcities}";

            // update train
            this.TrainNameText.text = currentPlayer.trainStyle.ToString();
        }

        public void SetGoods(IEnumerable<string> values)
        {
            var valuesArr = values.ToArray();
            for (int i = 0; i < Goods.Length; ++i)
            {
                var good = Goods[i];

                if (i < valuesArr.Length)
                    good.Value = valuesArr[i];
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
