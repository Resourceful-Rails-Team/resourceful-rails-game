using Rails.Collections;
using Rails.Data;
using Rails.Rendering;
using Rails.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails
{
    public static class GameLogic
    {
        // Updates current player through the intial build turns.
        public static void BuildTurn(ref int currentPlayer, ref Phase currentPhase, int maxPlayers)
        {
            // Phase.InitBuild:     inital build turns, normal player order.
            if (currentPhase == Phase.InitBuild)
            {
                ++currentPlayer;

                if (currentPlayer == maxPlayers)
                {
                    currentPhase = Phase.InitBuildRev;
                    --currentPlayer;
                }
            }

            // Phase.InitBuildRev:  initial build turns, reverse player order.
            else if (currentPhase == Phase.InitBuildRev)
            {
                --currentPlayer;

                if (currentPlayer == -1)
                {
                    currentPhase = Phase.Build;
                    ++currentPlayer;
                }
            }

            return;
        }

        // Builds the track.
        public static int BuildTrack(TrackGraph<int> Tracks, List<Route> routes,
            int player, Color playerColor, int spendLimit)
        {
            int totalCost = 0;
            foreach (Route route in routes)
            {
                // Check to make sure we're not over the spending limit.
                totalCost += route.Cost;
                if (totalCost > spendLimit)
                    break;

                GameGraphics.CommitPotentialTrack(route, playerColor);

                for (int i = 0; i < route.Distance; ++i)
                {
                    if (!Tracks.TryGetEdgeValue(route.Nodes[i], route.Nodes[i + 1], out var e) || e == -1)
                        Tracks[route.Nodes[i], route.Nodes[i + 1]] = player;
                }
            }
            return totalCost;
        }

        // Upgrades the train to new if possible.
        public static bool UpgradeTrain(ref int trainStyle, ref int money, int trainNew, int trainUpgradeCost)
        {
            // If player doesn't have enough money, don't upgrade
            if (money < trainUpgradeCost)
            {
                return false;
            }
            // Deduct value from player's money stock and change train value.
            money -= trainUpgradeCost;
            trainStyle = trainNew;

            return true;
        }

        // Changes the current player
        public static int IncrementPlayer(ref int currentPlayer, int maxPlayers)
        {
            currentPlayer += 1;
            if (currentPlayer >= maxPlayers)
                currentPlayer = 0;
            return currentPlayer;
        }

        // Changes players for switchback start.
        public static int DecrementPlayer(ref int currentPlayer, int maxPlayers)
        {
            currentPlayer -= 1;
            if (currentPlayer < 0)
                currentPlayer = maxPlayers;
            return currentPlayer;
        }

        // Cycles through UI screens
        public static Phase UpdatePhase(GameObject[] PhasePanels, ref Phase currentPhase)
        {
            PhasePanels[(int)currentPhase].SetActive(false);
            ++currentPhase;
            if (currentPhase >= Phase.MAX)
                currentPhase = Phase.Move;
            PhasePanels[(int)currentPhase].SetActive(true);

            return currentPhase;
        }

        // Check if the current player has won.
        public static bool CheckWin(int majorCities, int winMajorCities, int money, int winMoney)
        {
            if (majorCities >= winMajorCities &&
              money >= winMoney)
            {
                return true;
            }
            return false;
        }
    }
}