/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Data;
using Rails.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rails
{
    public class MainMenu : MonoBehaviour
    {
        #region Properties


        [Header("References")]
        public GameObject MainRoot;
        public GameObject CreateRoot;

        [Header("Create Game")]
        public Color[] PlayerColors;
        public int MaxPlayers = 6;

        [Header("Create Game UI References")]
        public TMPro.TMP_InputField PlayerCountInput;
        public Transform PlayerInputItemRoot;
        public Toggle PlayerRandomizeOrderToggle;

        [Header("Create Game Prefab References")]
        public ColorNameInputItem PlayerInputPrefab;

        #endregion

        #region Private Vars


        /// <summary>
        /// Number of players in create game screen.
        /// </summary>
        private int _numberOfPlayers = 1;

        /// <summary>
        /// State of randomize toggle on create game screen.
        /// </summary>
        private bool _randomizeOrder = false;

        /// <summary>
        /// Array of start player infos created by the create game screen.
        /// </summary>
        private StartPlayerInfo[] _players;

        /// <summary>
        /// Array of UI input items per player created by the create game screen.
        /// </summary>
        private ColorNameInputItem[] _playerInputs;

        #endregion

        #region Unity Events

        private void Start()
        {
            // Initialize player count, randomize toggle
            PlayerCountInput.text = _numberOfPlayers.ToString();
            PlayerRandomizeOrderToggle.isOn = _randomizeOrder;

            // Initialize player info
            CreateGame_FixPlayerInfo();
        }

        #endregion

        #region Create Game


        /// <summary>
        /// Changes to main title screen.
        /// </summary>
        public void Create_GotoTitle()
        {
            MainRoot.SetActive(true);
            CreateRoot.SetActive(false);
        }

        /// <summary>
        /// Increments the number of players.
        /// </summary>
        public void CreateGame_PlayerCountInc()
        {
            if (_numberOfPlayers < MaxPlayers)
                ++_numberOfPlayers;

            CreateGame_FixPlayerInfo();
            PlayerCountInput.text = _numberOfPlayers.ToString();
        }

        /// <summary>
        /// Decrements the number of players.
        /// </summary>
        public void CreateGame_PlayerCountDec()
        {
            if (_numberOfPlayers > 1)
                --_numberOfPlayers;

            CreateGame_FixPlayerInfo();
            PlayerCountInput.text = _numberOfPlayers.ToString();
        }

        /// <summary>
        /// Converts the number value in text to an int and stores it as the number of players.
        /// Resets back to last valid value on failure.
        /// </summary>
        public void CreateGame_PlayerCountChanged(string text)
        {
            if (int.TryParse(text, out var value))
            {
                if (value != _numberOfPlayers)
                {
                    _numberOfPlayers = value;
                    CreateGame_FixPlayerInfo();
                }
            }
            else
            {
                PlayerCountInput.text = _numberOfPlayers.ToString();
            }
        }

        /// <summary>
        /// Sets whether to randomize the player order on start or not.
        /// </summary>
        public void CreateGame_SetRandomizeOrder(bool value)
        {
            _randomizeOrder = value;
        }

        /// <summary>
        /// Creates the game.
        /// </summary>
        public void CreateGame()
        {
            var startGameRules = FindObjectOfType<GameStartRules>();
            if (!startGameRules)
            {
                // create new start game rules
                var go = new GameObject("start rules");
                startGameRules = go.AddComponent<GameStartRules>();

                // prevent unloading on scene change
                DontDestroyOnLoad(go);
            }

            //  initialize start game rules
            startGameRules.Players = _players;

            // randomize
            if (_randomizeOrder)
                startGameRules.Players = startGameRules.Players.OrderBy(x => Guid.NewGuid()).ToArray();

            // load scene
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Triggered when the user changes the name of a player via a ColorNameInputItem.
        /// </summary>
        private void CreateGame_PlayerNameChanged(object sender, string name)
        {
            if (sender is ColorNameInputItem inputItem)
            {
                var playerIndex = Array.IndexOf(_playerInputs, inputItem);
                if (playerIndex >= 0)
                {
                    _players[playerIndex].Name = name;
                }
            }
        }

        /// <summary>
        /// Ensures that the size of the _players array matches the number of players set.
        /// And that the data and references are valid.
        /// </summary>
        private void CreateGame_FixPlayerInfo()
        {
            if (_players == null)
                _players = new StartPlayerInfo[_numberOfPlayers];
            if (_playerInputs == null)
                _playerInputs = new ColorNameInputItem[_numberOfPlayers];
            if (_players.Length != _numberOfPlayers)
                Array.Resize(ref _players, _numberOfPlayers);

            // expand array
            if (_playerInputs.Length < _numberOfPlayers)
                Array.Resize(ref _playerInputs, _numberOfPlayers);

            // validate player
            for (int i = 0; i < _playerInputs.Length; ++i)
            {
                var playerInput = _playerInputs[i];
                if (i < _numberOfPlayers)
                {
                    var player = _players[i];

                    // validate player name
                    if (player.Name == null)
                        player.Name = $"Player {i + 1}";
                    // validate player color
                    if (player.Color == Color.clear)
                        player.Color = PlayerColors[i];

                    _players[i] = player;

                    // ensure input exists
                    if (playerInput == null)
                    {
                        // instantiate and initialize
                        _playerInputs[i] = playerInput = Instantiate(PlayerInputPrefab);
                        playerInput.transform.SetParent(PlayerInputItemRoot, false);
                        playerInput.transform.SetSiblingIndex(i);
                        playerInput.Color = player.Color;
                        playerInput.Name = $"Player {i + 1}:";
                        playerInput.Value = player.Name;
                        playerInput.OnValueChanged += CreateGame_PlayerNameChanged;
                    }
                }
                else
                {
                    // destroy
                    if (playerInput)
                    {
                        Destroy(playerInput.gameObject);
                        _playerInputs[i] = null;
                    }
                }
            }

            // truncate array
            if (_playerInputs.Length > _numberOfPlayers)
                Array.Resize(ref _playerInputs, _numberOfPlayers);
        }

        #endregion

        #region Title Screen

        /// <summary>
        /// Changes to create game screen.
        /// </summary>
        public void Title_GotoCreate()
        {
            MainRoot.SetActive(false);
            CreateRoot.SetActive(true);
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void Title_Exit()
        {
            Application.Quit();
        }

        #endregion

    }
}
