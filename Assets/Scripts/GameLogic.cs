using Rails.Collections;
using Rails.Data;
using Rails.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails {
    public static class GameLogic {
        // Updates current player through the intial build turns.
        public static void BuildTurn(ref int currentPlayer, ref int currentPhase, int maxPlayers) {
            // Phase -2, build turns, normal player order.
            // Phase -1, build turns, reverse player order.
            // Phase 0, normal turns, place trains.
            if (currentPhase == -2) {
                if (currentPlayer == maxPlayers - 1) {
                    currentPhase += 1;
								}
                else
                    IncrementPlayer(ref currentPlayer, maxPlayers);
            }
            else if (currentPhase == -1) {
                if (currentPlayer == 0) {
                    currentPhase = 1;
								}
                else
                    DecrementPlayer(ref currentPlayer, maxPlayers);
						}
            //if (currentPlayer == maxPlayers - 1 || currentPlayer == 0)
            //    currentPhase += 1;
            Debug.Log($"phase: {currentPhase}");
            return;
        }
        // Builds the track.
        public static void BuildTrack(TrackGraph<int> Tracks, int player, List<Route> routes, Color playerColor) {
            foreach (Route route in routes) {
                GameGraphics.CommitPotentialTrack(route, playerColor);

                for (int i = 0; i < route.Distance; ++i)
                {
                    if (!Tracks.TryGetEdgeValue(route.Nodes[i], route.Nodes[i+1], out var e) || e != Manager.MajorCityIndex)
                        Tracks[route.Nodes[i], route.Nodes[i + 1]] = player;
                }
            }
            return;
        }
        // Upgrades the train to new if possible.
        public static bool UpgradeTrain(ref int trainStyle, ref int money, int trainNew, int trainUpgrade) {
            // If player doesn't have enough money, don't upgrade
            if (money < trainUpgrade) {
                // TODO: Activate failure UI message here.
                return false;
            }
            // Deduct value from player's money stock and change train value.
            money -= trainUpgrade;
            trainStyle = trainNew;
            return true;
        }
        // Changes the current player
        public static int IncrementPlayer(ref int currentPlayer, int maxPlayers) {
            currentPlayer += 1;
            if (currentPlayer >= maxPlayers)
                currentPlayer = 0;
            return currentPlayer;
        }
        // Changes players for switchback start.
        public static int DecrementPlayer(ref int currentPlayer, int maxPlayers) {
            currentPlayer -= 1;
            if (currentPlayer < 0)
                currentPlayer = maxPlayers;
            return currentPlayer;
        }

        // Cycles through UI screens
        public static int UpdatePhase(GameObject[] PhasePanels, ref int currentPhase, int phases) {
            PhasePanels[currentPhase].SetActive(false);
            currentPhase += 1;
            if (currentPhase >= phases)
                currentPhase = 0;
            PhasePanels[currentPhase].SetActive(true);
            return currentPhase;
        }
        // Check if the current player has won.
        public static bool CheckWin(int majorCities, int winMajorCities, int money, int winMoney) {
            if (majorCities >= winMajorCities &&
              money >= winMoney) {
                return true;
            }
            return false;
        }

    }
}