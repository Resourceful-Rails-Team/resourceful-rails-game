using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Rails {
	public struct PlayerInfo {
		public string name;
		public Color color;
		public int money;
		public int train;
		public int majorcities;

		public PlayerInfo(string name, Color color) {
			this.name = name;
			this.color = color;
			money = 0;
			train = 0;
			majorcities = 0;
		}
	}

	public class GameManager : MonoBehaviour {

		public const int MAXPLAYERS = 2;
		public const int MAXPHASES = 2;
		public const int UPGRADE = 20;
		public const int MAJORCITIES = 6;
		public const int MONEY = 250;
		public const int MONEY_START = 50;

		#region Data Structures

		public GameObject playerInfo;
		public GameObject[] uiPanels = new GameObject[MAXPHASES];

		PlayerInfo[] players = new PlayerInfo[MAXPLAYERS];
		Stack<Node> path = new Stack<Node>();
		int currentPlayer = 0;
		int currentPhase = 0;
		#endregion

		#region Unity Events

		private void Awake() {
			// set singleton reference on awake
			_singleton = this;

			for (int p = 0; p < MAXPLAYERS; p++) {
				players[p] = new PlayerInfo("Player " + p, Color.white);
				players[p].money = 50;
				players[p].train = 0;
				players[p].majorcities = 0;
			}
			uiPanels[currentPhase].SetActive(true);
			UpdatePlayerInfo();
		}
		#endregion

		#region Singleton

		private static GameManager _singleton = null;

		/// <summary>
		/// Manager singleton
		/// </summary>
		public static GameManager Singleton {
			get {
				if (_singleton)
					return _singleton;

				_singleton = FindObjectOfType<GameManager>();
				if (_singleton)
					return _singleton;

				GameObject go = new GameObject("GameManager");
				return go.AddComponent<GameManager>();
			}
		}

		#endregion

		#region Player Actions

		// Discards the player's hand.
		public void DiscardHand() {
			// TODO: removing and refilling player's hand
			// Ends the turn.
			IncrementPlayer();
			return;
		}
		// Moves the train to final node in path.
		public void MoveTrain() {
			// TODO: Move train to last pushed node. 
			// Moving only updates the phase.
			UpdatePhase();
			return;
		}

		// Upgrades the player's train.
		public void UpgradeTrain(int choice) {
			ref PlayerInfo player = ref players[currentPlayer];

			// If player doesn't have enough money, don't upgrade
			if (player.money < UPGRADE) {
				// TODO: Activate failure UI message here.
				return;
			}

			// Deduct value from player's money stock and change train value.
			player.money -= UPGRADE;
			player.train = choice;
			Debug.Log(currentPlayer + " $" + players[currentPlayer].money);

			// Ends the turn and changes phase.
			IncrementPlayer();
			UpdatePhase();
			return;
		}
		// Builds the track between the nodes in path.
		public void BuildTrack() {
			// TODO: Build track between all nodes in stack.
			// Ends the turn and changes phase.
			IncrementPlayer();
			UpdatePhase();
			return;
		}

		#region Path Methods
		// Adds nodes to a path stack
		// Used for building and movement
		public void PushNode(Node node) {
			path.Push(node);
		}
		public Node PopNode() {
			return path.Pop();
		}
		public Node PeekNode() {
			return path.Peek();
		}
		#endregion
		#endregion

		#region Private Methods
		// Changes the current player
		private int IncrementPlayer() {
			currentPlayer += 1;
			if (currentPlayer >= MAXPLAYERS)
				currentPlayer = 0;
			UpdatePlayerInfo();
			return currentPlayer;
		}
		// Updates name and money amount. Placeholder.
		private void UpdatePlayerInfo() {
			Transform playertext = playerInfo.transform.Find("Player");
			playertext.GetComponent<TMP_Text>().text = "Player #" + (currentPlayer + 1);
			playertext = playerInfo.transform.Find("Money");
			playertext.GetComponent<TMP_Text>().text = "$" + players[currentPlayer].money;
		}
		// Cycles through UI screens
		private int UpdatePhase() {
			uiPanels[currentPhase].SetActive(false);
			currentPhase += 1;
			if (currentPhase >= MAXPHASES)
				currentPhase = 0;
			uiPanels[currentPhase].SetActive(true);
			return currentPhase;
		}
		// Check if the current player has won.
		private bool CheckWin() {
			PlayerInfo player = players[currentPlayer];
			if (player.majorcities < MAJORCITIES ||
				player.money < MONEY) {
				return false;
			}
			return true;
		}
		#endregion
	}
}