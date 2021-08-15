/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

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
        /// The Cost for a player to use another player's track.
        /// </summary>
        public int AltTrackCost = 4;
        /// <summary>
        /// The cost for a player to build across a river.
        /// </summary>
        public int RiverCrossCost = 2;
        /// <summary>
        /// The number of major cities a player can build from in a turn.
        /// </summary>
        public int MajorCityBuildsPerTurn = 2;
        /// <summary>
        /// The stats of the various trains used in the game.
        /// </summary>
        public TrainSpecs[] TrainSpecs;
        /// <summary>
        /// The cost to build to nodes.
        /// </summary>
        public NodeCost[] NodeCosts;
        /// <summary>
        /// The number of cards a player can hold at once.
        /// </summary>
        public int HandSize = 3;

        public int GetNodeCost(NodeType nodeType)
            => NodeCosts.FirstOrDefault(nc => nc.NodeType == nodeType)?.Cost ?? 0;
    }
}
