using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
{
    /// <summary>
    /// A collection of rules associated with a game of Resourceful Rails
    /// </summary>
    [Serializable]
    public class GameRules
    {
        /// <summary>
        /// The amount of money each player starts with.
        /// </summary>
        public int MoneyStart = 100;
        /// <summary>
        /// The max amount of money that can be spent building.
        /// </summary>
        public int MaxBuild = 15;
        /// <summary>
        /// The cost to for players to upgrade their train.
        /// </summary>
        public int TrainUpgrade = 15;
        /// <summary>
        /// The number of major cities that must be connected to win.
        /// </summary>
        public int WinMajorCities = 5;
        /// <summary>
        /// The amount of money needed to win.
        /// </summary>
        public int WinMoney = 1000;
        /// <summary>
        /// The Cost for a player to use another player's track
        /// </summary>
        public int AltTrackCost = 15;

        public TrainSpecs BaseTrainSpecs;
        public TrainSpecs FastTrainSpecs;
        public TrainSpecs HeavyTrainSpecs;
        public TrainSpecs SuperTrainSpecs;
    }
}
