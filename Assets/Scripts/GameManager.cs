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

		public PlayerInfo(string name, Color color, int money, int train) {
			this.name = name;
			this.color = color;
			this.money = money;
			this.train = train;
			majorcities = 0;
		}
	}

	public class GameManager : MonoBehaviour {

		#region Public Data
		public int MaxPlayers = 6;
		// The amount of money each player starts with.
		public int MoneyStart = 50;
		// The max amount of money that can be spent building.
		public int MaxBuild = 20;
		// The cost to for players to upgrade their train.
		public int TrainUpgrade = 20;
		// The number of major cities that must be connected to win.
		public int Win_MajorCities = 6;
		// The amount of money needed to win.
		public int Win_Money = 250;

		// The trains that players can use.
		public Rails.ScriptableObjects.TrainData[] trainData;
		// 
		public GameObject PlayerInfoPanel;
		public GameObject[] PhasePanels;
		#endregion

		#region Private Data
		PlayerInfo[] players;
		Stack<Node> path = new Stack<Node>();
		int phases;
		int currentPlayer = 0;
		int currentPhase = 0;
		#endregion

		#region Unity Events

		private void Awake() {
			// set singleton reference on awake
			_singleton = this;
			phases = PhasePanels.Length;
			// Initiate all player info.
			players = new PlayerInfo[MaxPlayers];
			for (int p = 0; p < MaxPlayers; p++)
				players[p] = new PlayerInfo("Player " + p, Color.white, MoneyStart, 0);
			// Deactivate all panels just in case.
			for (int u = 0; u < phases; u++)
				PhasePanels[u].SetActive(false);
			// Activate first turn panel.
			PhasePanels[currentPhase].SetActive(true);
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

		// Moves the train to final node in path.
		public void MoveTrain() {
			// TODO: Move train to last pushed node. 
			// Moving only updates the phase.
			UpdatePhase();
			return;
		}
		// Discards the player's hand.
		public void DiscardHand() {
			// TODO: removing and refilling player's hand
			// Ends the turn.
			IncrementPlayer();
			return;
		}

		// Builds the track between the nodes in path.
		public void BuildTrack() {
			// TODO: Build track between all nodes in stack.
			// Ends the turn and changes phase.
			BuildTrack_();
			IncrementPlayer();
			UpdatePhase();
			return;
		}
		// Upgrades the player's train.
		public void UpgradeTrain(int choice) {
			UpgradeTrain_(choice);
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
		// Private method for building.
		private void BuildTrack_() {

		}
		// Private method for upgrading.
		private void UpgradeTrain_(int choice) {
			ref PlayerInfo player = ref players[currentPlayer];

			// If player doesn't have enough money, don't upgrade
			if (player.money < TrainUpgrade) {
				// TODO: Activate failure UI message here.
				return;
			}

			// Deduct value from player's money stock and change train value.
			player.money -= TrainUpgrade;
			player.train = choice;
			Debug.Log(currentPlayer + " $" + players[currentPlayer].money);
		}

		// Changes the current player
		private int IncrementPlayer() {
			currentPlayer += 1;
			if (currentPlayer >= MaxPlayers)
				currentPlayer = 0;
			UpdatePlayerInfo();
			return currentPlayer;
		}
		// Updates name and money amount. Placeholder.
		private void UpdatePlayerInfo() {
			Transform playertext = PlayerInfoPanel.transform.Find("Player");
			playertext.GetComponent<TMP_Text>().text = "Player #" + (currentPlayer + 1);
			playertext = PlayerInfoPanel.transform.Find("Money");
			playertext.GetComponent<TMP_Text>().text = "$" + players[currentPlayer].money;
		}
		// Cycles through UI screens
		private int UpdatePhase() {
			PhasePanels[currentPhase].SetActive(false);
			currentPhase += 1;
			if (currentPhase >= phases)
				currentPhase = 0;
			PhasePanels[currentPhase].SetActive(true);
			return currentPhase;
		}
		// Check if the current player has won.
		private bool CheckWin() {
			PlayerInfo player = players[currentPlayer];
			if (player.majorcities < Win_MajorCities ||
				player.money < Win_Money) {
				return false;
			}
			return true;
		}
		#endregion
	}
}