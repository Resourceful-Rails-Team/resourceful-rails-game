using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public int MoneyStart = 50;
        /// <summary>
        /// The max amount of money that can be spent building.
        /// </summary>
        public int MaxBuild = 20;
        /// <summary>
        /// The cost to for players to upgrade their train.
        /// </summary>
        public int TrainUpgrade = 20;
        /// <summary>
        /// The number of major cities that must be connected to win.
        /// </summary>
        public int WinMajorCities = 6;
        /// <summary>
        /// The amount of money needed to win.
        /// </summary>
        public int WinMoney = 250;
        /// <summary>
        /// The Cost for a player to use another player's track
        /// </summary>
        public int AltTrackCost = 4;
        public int RiverCrossCost;

        public TrainSpecs[] TrainSpecs;
        public NodeCost[] NodeCosts;
        public int MajorCityBuildsPerTurn;

        public int GetNodeCost(NodeType nodeType)
            => NodeCosts.FirstOrDefault(nc => nc.NodeType == nodeType)?.Cost ?? 0;
    }
}
