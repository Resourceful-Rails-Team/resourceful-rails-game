using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
{
    /// <summary>
    /// A collection of rules associated with a game of Resourceful Rails
    /// </summary>
    [Serializable]
    public class GameRules : MonoBehaviour
    {
        /// <summary>
        /// The amount of money each player starts with.
        /// </summary>
        public int MoneyStart;
        /// <summary>
        /// The max amount of money that can be spent building.
        /// </summary>
        public int MaxBuild;
        /// <summary>
        /// The cost to for players to upgrade their train.
        /// </summary>
        public int TrainUpgrade;
        /// <summary>
        /// The number of major cities that must be connected to win.
        /// </summary>
        public int WinMajorCities;
        /// <summary>
        /// The amount of money needed to win.
        /// </summary>
        public int WinMoney;

        public TrainSpecs BaseTrainSpecs;
        public TrainSpecs FastTrainSpecs;
        public TrainSpecs HeavyTrainSpecs;
        public TrainSpecs SuperTrainSpecs;

        public StartPlayerInfo[] Players;
    }

    [Serializable]
    public struct StartPlayerInfo
    {
        public string Name;
        public Color Color;
    }
}
