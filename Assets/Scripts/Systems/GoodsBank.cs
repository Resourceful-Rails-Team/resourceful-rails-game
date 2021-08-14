/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rails.ScriptableObjects;
using Rails.Data;

namespace Rails.Systems
{
    public static class GoodsBank
    {
        #region Properties
        private static Manager manager = Manager.Singleton;
        private static MapData mapData = manager.MapData;
        private static int goodsCount = manager.MapData.Goods.Count;
        private static int[] tokens = new int[goodsCount];

        #endregion

        #region Public
        // Initializer
        public static void Initialize()
        {
            tokens = new int[mapData.Goods.Count];
            for (int i = 0; i < mapData.Goods.Count; i++)
            {
                tokens[i] = 3;
            }
        }

        // Returns the number of goods left of that type in the bank.
        public static int GetGoodQuantity(Good good)
        {
            int index = mapData.Goods.IndexOf(good);
            return tokens[index];
        }
        // Picks up a good from a city.
        public static bool GoodPickup(Good good, List<Good> goodsCarried, int trainType)
        {
            bool success = false;
            // Check to make sure that good is in that city.

            int index = mapData.Goods.IndexOf(good);

            // Make sure there are enough goods left and that the train isn't full.
            if (tokens[index] != 0 &&
                goodsCarried.Count <
                manager.Rules.TrainSpecs[trainType].goodsTotal)
            {
                --tokens[index];
                goodsCarried.Add(mapData.Goods[index]);
                success = true;
            }
            return success;
        }
        // Drops off a good in any city.
        public static bool GoodDropoff(int index, List<Good> goodsCarried)
        {
            bool success = false;
            // Make sure train has goods to drop off and that index is in range.
            if (goodsCarried.Count > 0 &&
                index >= 0 && index < goodsCarried.Count)
            {
                int i = mapData.Goods.IndexOf(goodsCarried[index]);
                ++tokens[i];
                goodsCarried.RemoveAt(index);
                success = true;
            }
            return success;
        }
        #endregion

        #region Private

        #endregion
    }
}