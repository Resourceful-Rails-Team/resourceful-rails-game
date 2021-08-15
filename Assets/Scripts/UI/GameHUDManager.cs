/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Assets.Scripts.Data;
using Rails.Controls;
using Rails.Data;
using Rails.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rails.UI
{
    public class GameHUDManager : MonoBehaviour
    {
        /// <summary>
        /// This class links a given phase to a name and description that is shown
        /// to the user between phases.
        /// </summary>
        [Serializable]
        public class PhaseInfo
        {
            public Phase Phase;
            public string Name;
            public string Description;
        }

        /// <summary>
        /// Enum of all the different help steps.
        /// </summary>
        [Serializable]
        public enum HelpStep
        {
            FirstTwoTurns,
            BuildingTrack,
            Strategy,
            PlacingMoving,
            PickDrop,
            Upgrading,
            Winning
        }

        /// <summary>
        /// This class links a HelpStep to its respective content and NavBar button.
        /// </summary>
        [Serializable]
        public class HelpStepInfo
        {
            public HelpStep Step;
            public GameObject Root;
            public Button NavButton;
        }

        /// <summary>
        /// Static singleton for easy access.
        /// </summary>
        public static GameHUDManager Singleton { get; private set; }

        [Header("Other References")]
        public Transform WorldCanvas;

        [Header("Player")]
        public PlayerInfoItem CurrentPlayerInfo;
        public PlayerInfoItem[] AllPlayerInfos;
        public Transform AllPlayerInfosRoot;

        [Header("Build")]
        public GameObject BuildMarkerPrefab;
        public TrackItem TrackItemPrefab;
        public Transform TracksRoot;
        public Transform BuildInfoPanel;
        public Transform UpgradePanel;

        [Header("Phase Controls")]
        public Button BuildButton;
        public TooltipHandler BuildButtonTooltipHandler;
        public TMPro.TMP_Text BuildButtonTooltip;
        public Button UpgradeButton;
        public TooltipHandler UpgradeButtonTooltipHandler;
        public TMPro.TMP_Text UpgradeButtonTooltip;
        public Button DiscardButton;
        public TooltipHandler DiscardButtonTooltipHandler;

        [Header("Track Select/Delete")]
        public TrackSelectDeleteItem TrackSelectDeleteItemPrefab;
        public Transform TrackSelectPanel;
        public Transform TrackSelectItemsRoot;
        public Transform TrackSelectStartRoot;
        public Transform TrackSelectDeleteButtonRoot;

        [Header("Move")]
        public TrackSelectDeleteItem TrackSelectDeleteItemSmallPrefab;
        public Transform MoveInfoPanel;
        public Transform MoveInfoItemsRoot;

        [Header("Pickup Drop")]
        public Transform CityPickDropPanel;
        public TMPro.TMP_Text CityPickDropNameText;
        public Toggle[] CityPickDropPickupToggles;
        public TMPro.TMP_Text[] CityPickDropPickupTexts;
        public CityPickDropDropoffItem[] CityPickDropDropoffItems;
        public Button CityPickDropContinue;
        public TMPro.TMP_Text CityPickDropContinueTooltipText;
        public TooltipHandler CityPickDropContinueTooltipHandler;

        [Header("End Game")]
        public Transform PlayerWonPanel;
        public Transform PlayerWonPlayersRoot;
        public EndGamePlayerItem EndGamePlayerItemPrefab;
        public TMPro.TMP_Text PlayerWonNameText;
        public TMPro.TMP_Text PlayerWonMoneyText;
        public TMPro.TMP_Text PlayerWonCitiesText;

        [Header("Phase Turn Transition")]
        public Transform PhaseTurnTransitionPanel;
        public TMPro.TMP_Text PhaseTurnTransitionTitleText;
        public TMPro.TMP_Text PhaseTurnTransitionPlayerNameText;
        public TMPro.TMP_Text PhaseTurnTransitionSubheadingText;
        public List<PhaseInfo> PhaseInfos;

        [Header("Help Text")]
        public GameObject HelpPanel;
        public List<HelpStepInfo> HelpStepInfos;

        /// <summary>
        /// Collection of build markers by NodeId that are rendered on the game's WorldCanvas.
        /// </summary>
        private Dictionary<NodeId, BuildMarkerContainer> _buildMarkers = new Dictionary<NodeId, BuildMarkerContainer>();

        /// <summary>
        /// Collection of items rendered in the top left corner for each build track.
        /// </summary>
        private List<TrackItem> _uiBuildTrackItems = new List<TrackItem>();

        /// <summary>
        /// Index of the path to reference in the Track Select Delete panel.
        /// </summary>
        private int _uiTrackSelectPathIndex = -1;

        /// <summary>
        /// Collection of items rendered in the Track Select Delete panel.
        /// Each item represents a node in the user's build path.
        /// </summary>
        private List<TrackSelectDeleteItem> _uiTrackSelectDeleteItems = new List<TrackSelectDeleteItem>();

        /// <summary>
        /// Collection of items rendered in the top left corner for each move node.
        /// </summary>
        private List<TrackSelectDeleteItem> _uiMoveTrackItems = new List<TrackSelectDeleteItem>();

        /// <summary>
        /// The interaction object passed when OnTrainMeetsCityHandler is raised.
        /// </summary>
        private TrainCityInteraction _cityPickDropInteraction;

        /// <summary>
        /// The current Phase Turn Transition panel coroutine that is executing.
        /// </summary>
        private IEnumerator _phaseTurnTransitionCoroutine;

        /// <summary>
        /// This class links a NodeId to a world-space object with a text component.
        /// Used to render a collection of Node names above a given node.
        /// </summary>
        private class BuildMarkerContainer
        {
            public NodeId NodeId { get; set; }
            public GameObject GameObject { get; set; }
            public TMPro.TMP_Text TextComponent { get; set; }

            public List<string> NodeNames { get; } = new List<string>();

            /// <summary>
            /// Add name to the list of names.
            /// </summary>
            public void AddName(string name)
            {
                NodeNames.Add(name);
                UpdateText();
            }

            /// <summary>
            /// Remove name from list of names.
            /// </summary>
            public void RemoveName(string name)
            {
                NodeNames.Remove(name);
                UpdateText();
            }

            /// <summary>
            /// Update the text with the current list of names, comma separated.
            /// </summary>
            public void UpdateText()
            {
                TextComponent.text = string.Join(",", NodeNames);
            }

            /// <summary>
            /// Clear the list of names and text.
            /// </summary>
            public void ClearText()
            {
                NodeNames.Clear();
                UpdateText();
            }
        }

        #region Unity Events

        /// <summary>
        /// Called once at the very start.
        /// </summary>
        private void Awake()
        {
            // Initialize singleton
            Singleton = this;
        }

        /// <summary>
        /// Called on start.
        /// </summary>
        private void Start()
        {
            // grab manager
            var manager = Manager.Singleton;
            if (!manager)
                return;

            // Subscribe to manager events
            manager.OnPlayerInfoUpdate += Manager_OnPlayerInfoUpdate;
            manager.OnPhaseChange += Manager_OnPhaseChange;
            manager.OnTrainMeetsCityHandler += Manager_OnTrainMeetsCity;
            manager.OnGameOver += Manager_OnGameOver;
            manager.OnTurnEnd += Manager_OnTurnEnd;

            // Invoke events on start to initialize state
            Manager_OnPlayerInfoUpdate(manager);
            Manager_OnPhaseChange(manager);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            var manager = Manager.Singleton;
            switch (manager.CurrentPhase)
            {
                case Phase.Build:
                case Phase.InitBuild:
                case Phase.InitBuildRev:
                    {
                        // Refresh build track UI
                        Manager_OnBuildTrackUpdated(Manager.Singleton);

                        // disable/enable upgrade button depending on money
                        if (manager.Rules.TrainUpgrade > manager.Player.money)
                        {
                            UpgradeButton.interactable = false;
                            UpgradeButtonTooltipHandler.enabled = true;
                            UpgradeButtonTooltip.text = $"You need at least ${manager.Rules.TrainUpgrade} to upgrade!";
                        }
                        else
                        {
                            UpgradeButton.interactable = true;
                            UpgradeButtonTooltipHandler.enabled = false;
                        }
                        break;
                    }
                case Phase.Move:
                    {
                        // Refresh move track UI
                        Manager_OnMoveTrackUpdated(Manager.Singleton);

                        // disable/enable discard button if player has moved this turn
                        if (manager.Rules.TrainSpecs[manager.Player.trainType].movePoints > manager.Player.movePointsLeft)
                        {
                            DiscardButton.interactable = false;
                            DiscardButtonTooltipHandler.enabled = true;
                        }
                        else
                        {
                            DiscardButton.interactable = true;
                            DiscardButtonTooltipHandler.enabled = false;
                        }
                        break;
                    }
            }

            // handle toggling of all players panel
            if (GameInput.ToggleAllPlayersJustPressed)
            {
                if (AllPlayerInfosRoot.gameObject.activeSelf)
                    OnHideAllPlayerInfos();
                else
                    OnShowAllPlayerInfos();
            }
        }

        #endregion

        #region City PickDrop

        /// <summary>
        /// Triggered when the current player's train passes through a city.
        /// </summary>
        private void Manager_OnTrainMeetsCity(object sender, TrainCityInteraction e)
        {
            // show panel
            CityPickDropPanel.gameObject.SetActive(true);

            // set input context to popup
            GameInput.CurrentContext = GameInput.Context.Popup;

            // store event args into local variable for later
            _cityPickDropInteraction = e;

            // set city name
            CityPickDropNameText.text = e.City.Name;

            // set pickup goods
            for (int i = 0; i < CityPickDropPickupToggles.Length; ++i)
            {
                // try and get good
                var good = e.Goods.ElementAtOrDefault(i);
                var toggle = CityPickDropPickupToggles[i];

                // if good doesn't exist then hide UI row
                if (good == null)
                {
                    toggle.gameObject.SetActive(false);
                }
                else
                {
                    toggle.gameObject.SetActive(true);
                    toggle.isOn = false;
                    CityPickDropPickupTexts[i].text = good.Name;
                }
            }

            // set dropoff
            for (int i = 0; i < CityPickDropDropoffItems.Length; ++i)
            {
                var item = CityPickDropDropoffItems[i];

                // try and get demand from card
                // since each card cannot have duplicate cities, we just find the first demand with the current city
                var demand = e.Cards.ElementAtOrDefault(i)?.FirstOrDefault(x => x.City == e.City);

                // if no demand then hide UI element
                if (demand == null)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    // show UI element and update info
                    item.gameObject.SetActive(true);
                    item.Money = $"${demand.Reward}";
                    item.CardNumber = i + 1;
                    item.Icon = demand.Good.Icon;
                    item.IconTooltip = demand.Good.Name;
                    item.Toggle.isOn = false;
                }
            }

            // trigger initial validation
            CityPickDrop_Validate();
        }

        /// <summary>
        /// Validates the selected pickup and dropoff goods.
        /// On success, the continue button is enabled.
        /// On failure, the continue button is disabled and a tooltip is enabled with the relevant validation error.
        /// </summary>
        public void CityPickDrop_Validate()
        {
            bool isValid = true;
            string invalidMessage = "";
            var manager = Manager.Singleton;
            var player = manager.Players[_cityPickDropInteraction.PlayerIndex];
            var playerGoodsCarried = player.goodsCarried.ToList();

            // Iterate each dropoff item
            // Simulate dropping off item by removing good from local playerGoodsCarried 
            foreach (var dropoffItem in CityPickDropDropoffItems)
            {
                if (dropoffItem.Toggle.isOn)
                {
                    var good = playerGoodsCarried.FirstOrDefault(x => x.Icon == dropoffItem.Icon);
                    if (good != null)
                    {
                        playerGoodsCarried.Remove(good);
                    }
                    else
                    {
                        isValid = false;
                        invalidMessage = $"You do not have enough {good.Name} to dropoff!";
                    }
                }
            }

            // Iterate each pickup item
            // Simulate picking up an item by adding it to the local playerGoodsCarried
            for (int i = 0; i < _cityPickDropInteraction.Goods.Length; ++i)
            {
                var pickupToggle = CityPickDropPickupToggles[i];
                if (pickupToggle.isOn)
                    playerGoodsCarried.Add(_cityPickDropInteraction.Goods[i]);
            }

            // invalid if goods carried is more than allowed
            if (isValid && playerGoodsCarried.Count > manager.Rules.TrainSpecs[manager.Player.trainType].goodsTotal)
            {
                isValid = false;
                invalidMessage = $"Your train cannot carry {playerGoodsCarried.Count} goods!";
            }

            // update state
            CityPickDropContinue.interactable = isValid;
            CityPickDropContinueTooltipHandler.enabled = !isValid;
            CityPickDropContinueTooltipText.text = invalidMessage;
        }

        /// <summary>
        /// Sends pickup/dropoff results back to Manager and closes the CityPickDrop panel.
        /// </summary>
        public void CityPickDrop_Continue()
        {
            var goods = new List<Good>();
            var cards = new List<DemandCard>();

            // Construct list of cards to use for dropoff
            for (int i = 0; i < _cityPickDropInteraction.Cards.Length; ++i)
            {
                if (CityPickDropDropoffItems[i].Toggle.isOn)
                    cards.Add(_cityPickDropInteraction.Cards[i]);
            }

            // Construct list of goods to pickup from city
            for (int i = 0; i < _cityPickDropInteraction.Goods.Length; ++i)
            {
                if (CityPickDropPickupToggles[i].isOn)
                    goods.Add(_cityPickDropInteraction.Goods[i]);
            }

            // Send to handler
            Manager.Singleton.OnTrainMeetsCityComplete.Invoke(this, new TrainCityInteractionResult()
            {
                ChosenCards = cards.ToArray(),
                Goods = goods.ToArray()
            });

            // close panel and return input state to game
            CityPickDropPanel.gameObject.SetActive(false);
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        #endregion

        #region Build Panel

        /// <summary>
        /// Builds the current track(s).
        /// </summary>
        public void BuildTrack()
        {
            Manager.Singleton.BuildTrack();
        }

        /// <summary>
        /// Ends the current build phase.
        /// </summary>
        public void EndBuild()
        {
            Manager.Singleton.EndBuild();
        }

        /// <summary>
        /// Attempts to open a panel with the upgrade options for the user to select.
        /// </summary>
        public void UpgradeTrain()
        {
            const string button1 = "Button 1";
            const string button2 = "Button 2";

            switch (Manager.Singleton.Player.trainType)
            {
                // Super
                case 3:
                    // Do nothing since player is fully upgraded.
                    return;

                // Fast or Heavy
                case 1:
                case 2:
                    int t1 = 3;
                    // Find the correct buttons.
                    Button b1 = UpgradePanel.transform.Find(button1).GetComponent<Button>();
                    Button b2 = UpgradePanel.transform.Find(button2).GetComponent<Button>();

                    // Remove previous listeners
                    b1.onClick.RemoveAllListeners();
                    b2.onClick.RemoveAllListeners();

                    // Add events to trigger upgrading.
                    b1.onClick.AddListener(delegate { UpgradeTrain(t1); });

                    // Change the text to match the train specs.
                    b1.GetComponentInChildren<TMPro.TMP_Text>().text = Manager.Singleton.Rules.TrainSpecs[t1].ToString();

                    // Set the correct elements to be active.
                    b1.gameObject.SetActive(true);
                    b2.gameObject.SetActive(false);
                    UpgradePanel.gameObject.SetActive(true);
                    break;

                // Standard
                case 0:
                    t1 = 1;
                    int t2 = 2;
                    // Find the correct buttons.
                    b1 = UpgradePanel.transform.Find(button1).GetComponent<Button>();
                    b2 = UpgradePanel.transform.Find(button2).GetComponent<Button>();

                    // Remove previous listeners
                    b1.onClick.RemoveAllListeners();
                    b2.onClick.RemoveAllListeners();

                    // Add events to trigger upgrading.
                    b1.onClick.AddListener(delegate { UpgradeTrain(t1); });
                    b2.onClick.AddListener(delegate { UpgradeTrain(t2); });

                    // Change the text to match the train specs.
                    b1.GetComponentInChildren<TMPro.TMP_Text>().text = Manager.Singleton.Rules.TrainSpecs[t1].ToString();
                    b2.GetComponentInChildren<TMPro.TMP_Text>().text = Manager.Singleton.Rules.TrainSpecs[t2].ToString();

                    // Set the correct elements to be active.
                    b1.gameObject.SetActive(true);
                    b2.gameObject.SetActive(true);
                    UpgradePanel.gameObject.SetActive(true);
                    break;
            }
            return;
        }

        /// <summary>
        /// Closes the upgrade panel.
        /// </summary>
        public void UpgradeCancel()
        {
            UpgradePanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Attempts to upgrade the current player's train to the selected option.
        /// Closes the upgrade panel on success.
        /// </summary>
        /// <param name="value"></param>
        public void UpgradeTrain(int value)
        {
            if (Manager.Singleton.UpgradeTrain(value))
                UpgradePanel.gameObject.SetActive(false);
        }

        #endregion

        #region Move Panel

        /// <summary>
        /// Begins moving the train along the configured path.
        /// </summary>
        public void MoveTrain()
        {
            Manager.Singleton.MoveTrain();
        }

        /// <summary>
        /// Discards the current hand.
        /// </summary>
        public void DiscardHand()
        {
            Manager.Singleton.DiscardHand();
        }

        /// <summary>
        /// Ends the move phase.
        /// </summary>
        public void EndMove()
        {
            Manager.Singleton.EndMove();
        }

        #endregion

        #region Build Track UI

        /// <summary>
        /// Refreshes the build track UI and world-space build markers.
        /// </summary>
        private void Manager_OnBuildTrackUpdated(Manager manager)
        {
            var players = manager.Players;
            var currentPlayer = players[manager.CurrentPlayer];
            var pathCount = PathPlanner.Paths;

            // clear markers
            foreach (var marker in _buildMarkers)
                marker.Value.ClearText();

            // Delete excess track item gameobjects
            while (pathCount < _uiBuildTrackItems.Count)
            {
                DestroyUIBuildTrackItem(_uiBuildTrackItems[_uiBuildTrackItems.Count - 1]);
                _uiBuildTrackItems.RemoveAt(_uiBuildTrackItems.Count - 1);
            }

            // add missing tracks as empty
            while (pathCount > _uiBuildTrackItems.Count)
                _uiBuildTrackItems.Add(null);

            // populate markers
            for (int i = 0; i < pathCount; ++i)
            {
                var path = PathPlanner.GetPath(i);

                // update markers
                UpdateTrackMarkers(i, path);

                // update track ui
                var track = _uiBuildTrackItems[i];
                UpdateUIBuildTrackItems(i, ref track, path);
                _uiBuildTrackItems[i] = track;
            }

            // disable build button if build is not valid
            BuildButtonTooltipHandler.enabled = false;
            BuildButton.interactable = true;
            if (PathPlanner.CurrentCost > manager.Rules.MaxBuild)
            {
                BuildButton.interactable = false;
                BuildButtonTooltipHandler.enabled = true;
                BuildButtonTooltip.text = $"Unable to build track more than ${manager.Rules.MaxBuild}!";

            }
            else if (PathPlanner.CurrentCost > currentPlayer.money)
            {
                BuildButton.interactable = false;
                BuildButtonTooltipHandler.enabled = true;
                BuildButtonTooltip.text = $"You do not have enough money to build!";
            }
        }

        /// <summary>
        /// Destroys the given track item.
        /// </summary>
        private void DestroyUIBuildTrackItem(TrackItem trackItem)
        {
            // destroy
            if (trackItem)
            {
                Destroy(trackItem.gameObject);
            }
        }

        /// <summary>
        /// Updates the given track item's name and cost with the given path index.
        /// If the trackItem passed is null, a new one will be created.
        /// </summary>
        private void UpdateUIBuildTrackItems(int index, ref TrackItem trackItem, List<NodeId> path)
        {
            var trackName = Utilities.GetTrackNameByIndex(index);

            if (trackItem == null)
            {
                trackItem = Instantiate(TrackItemPrefab);
                trackItem.transform.SetParent(TracksRoot, false);
                trackItem.transform.SetSiblingIndex(index);
                trackItem.OnTrackSelected += OnUIBuildTrackSelect;
                trackItem.OnTrackDeleted += OnUIBuildTrackDelete;
            }

            trackItem.Name = $"{(PathPlanner.CurrentPath == index ? "<color=#FFFF00>" : "")}Track {Utilities.GetTrackNameByIndex(index)}";
            trackItem.Cost = $"{(index < PathPlanner.buildRoutes.Count ? PathPlanner.buildRoutes[index].Cost : 0)}";
        }

        /// <summary>
        /// Opens the track select panel with the given track.
        /// </summary>
        private void OnUIBuildTrackSelect(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUIBuildTrackSelect(index, true);
        }

        /// <summary>
        /// Opens the track delete panel with the given track.
        /// </summary>
        private void OnUIBuildTrackDelete(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUIBuildTrackSelect(index, false);
        }

        /// <summary>
        /// Adds a new track to the list of build tracks.
        /// </summary>
        public void OnUIBuildTrackAddNew()
        {
            var index = PathPlanner.CreatePath();
            PathPlanner.SetPath(index);
        }

        /// <summary>
        /// Deletes the entire (selected) track from the build tracks and closes the track select delete panel.
        /// </summary>
        public void OnUIBuildTrackSelectDeletePath()
        {
            PathPlanner.RemovePath(_uiTrackSelectPathIndex);

            // close
            OnUIBuildTrackSelectClose();
        }

        /// <summary>
        /// Selects the first node index in the selected path and closes the track select delete panel.
        /// </summary>
        public void OnUIBuildTrackSelectStart()
        {
            // set selected node to
            PathPlanner.SetPath(_uiTrackSelectPathIndex);
            PathPlanner.SetNode(_uiTrackSelectPathIndex, 0);

            // close
            OnUIBuildTrackSelectClose();
        }

        /// <summary>
        /// Closes the build track select panel.
        /// </summary>
        public void OnUIBuildTrackSelectClose()
        {
            // clear items
            foreach (var item in _uiTrackSelectDeleteItems)
            {
                if (item)
                    Destroy(item.gameObject);
            }

            _uiTrackSelectDeleteItems.Clear();

            // hide panel
            TrackSelectPanel.gameObject.SetActive(false);

            // enable game interactions
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        /// <summary>
        /// Opens the build track select delete panel with the given path index.
        /// If select is true, the select panel is shown.
        /// If select is false, the delete panel is shown.
        /// </summary>
        private void OpenUIBuildTrackSelect(int pathIndex, bool select)
        {
            // call close to ensure its closed before opening again
            OnUIBuildTrackSelectClose();

            // prevent game interactions
            GameInput.CurrentContext = GameInput.Context.Popup;

            // set path index
            _uiTrackSelectPathIndex = pathIndex;

            // show panel
            TrackSelectPanel.gameObject.SetActive(true);

            // show delete if not select
            TrackSelectDeleteButtonRoot.gameObject.SetActive(!select);
            TrackSelectStartRoot.gameObject.SetActive(select);

            // add items
            var manager = Manager.Singleton;
            var path = PathPlanner.GetPath(pathIndex);
            var pathName = Utilities.GetTrackNameByIndex(pathIndex);
            int i = 0;
            int offset = select ? 1 : 0;
            foreach (var nodeId in path)
            {
                var item = Instantiate(TrackSelectDeleteItemPrefab);
                item.transform.SetParent(TrackSelectItemsRoot, false);
                item.transform.SetSiblingIndex(i + offset);
                item.IsSelect = select;
                item.Name = $"{pathName}{i + 1}";

                var iRef = i;

                item.OnTrackSelected += (track) =>
                {
                    // set selected node to
                    PathPlanner.SetPath(pathIndex);
                    PathPlanner.SetNode(pathIndex, iRef+1);

                    // close
                    OnUIBuildTrackSelectClose();
                };
                item.OnTrackDeleted += (track) =>
                {
                    // remove
                    PathPlanner.RemoveNode(pathIndex, iRef);
                    PathPlanner.PlannedTracks();

                    // close
                    OnUIBuildTrackSelectClose();
                };

                _uiTrackSelectDeleteItems.Add(item);
                ++i;
            }
        }

        /// <summary>
        /// Updates the collection of markers to match the given path.
        /// </summary>
        private void UpdateTrackMarkers(int index, List<NodeId> path, int startIndex = 0)
        {
            var trackName = Utilities.GetTrackNameByIndex(index);
            var isTrackSelected = PathPlanner.CurrentPath == index;
            for (int i = startIndex; i < path.Count; ++i)
            {
                var nodeId = path[i];
                var marker = _buildMarkers.ContainsKey(nodeId) ? _buildMarkers[nodeId] : null;

                // if marker doesn't exist then add it
                if (marker == null)
                {
                    marker = new BuildMarkerContainer()
                    {
                        GameObject = Instantiate(BuildMarkerPrefab),
                        NodeId = nodeId
                    };

                    marker.TextComponent = marker.GameObject.GetComponentInChildren<TMPro.TMP_Text>();
                    marker.GameObject.transform.SetParent(WorldCanvas, false);
                    _buildMarkers.Add(nodeId, marker);
                }

                // add name
                var name = $"{trackName}{i + 1}";
                if (isTrackSelected && i == (PathPlanner.CurrentNode-1))
                    marker.AddName($"<color=#FFFF00>{name}</color>");
                else
                    marker.AddName($"{name}");

                // move to node
                marker.GameObject.transform.position = Utilities.GetPosition(nodeId);
            }
        }

        #endregion

        #region Move Track UI

        /// <summary>
        /// Refreshes the move track UI.
        /// </summary>
        private void Manager_OnMoveTrackUpdated(Manager manager)
        {
            var players = manager.Players;
            var currentPlayer = players[manager.CurrentPlayer];
            var path = currentPlayer.movePath;

            // clear markers
            foreach (var marker in _buildMarkers)
                marker.Value.ClearText();

            // Delete excess track item gameobjects
            while (path.Count < _uiMoveTrackItems.Count)
            {
                DestroyMoveTrackItem(_uiMoveTrackItems[_uiMoveTrackItems.Count - 1]);
                _uiMoveTrackItems.RemoveAt(_uiMoveTrackItems.Count - 1);
            }

            // add missing tracks as empty
            while (path.Count > _uiMoveTrackItems.Count)
                _uiMoveTrackItems.Add(null);

            // update markers
            UpdateTrackMarkers(0, path, 1);

            // populate markers
            for (int i = 0; i < _uiMoveTrackItems.Count; ++i)
            {
                // update track ui
                var track = _uiMoveTrackItems[i];
                UpdateUIMoveTrackItems(i, ref track, path[i]);
                _uiMoveTrackItems[i] = track;
            }
        }

        /// <summary>
        /// Destroys the given track item.
        /// </summary>
        private void DestroyMoveTrackItem(TrackSelectDeleteItem trackItem)
        {
            // destroy
            if (trackItem)
            {
                Destroy(trackItem.gameObject);
            }
        }

        /// <summary>
        /// Updates the given track item's name given a node index.
        /// If trackItem is null, a new track item will be created.
        /// </summary>
        private void UpdateUIMoveTrackItems(int index, ref TrackSelectDeleteItem trackItem, NodeId nodeId)
        {
            if (trackItem == null)
            {
                trackItem = Instantiate(TrackSelectDeleteItemSmallPrefab);
                trackItem.transform.SetParent(MoveInfoItemsRoot, false);
                trackItem.transform.SetSiblingIndex(index);
                trackItem.OnTrackSelected += OnUIMoveTrackSelect;
                trackItem.OnTrackDeleted += OnUIMoveTrackDelete;
            }

            // set name to train if index is 0
            var name = $"A{index+1}";
            if (index == 0)
            {
                name = "Train";
                trackItem.DeleteButton.interactable = false;
            }
            
            // update name, setting color to yellow if selected
            trackItem.Name = $"{(PathPlanner.CurrentNode == (index+1) ? "<color=#FFFF00>" : "")}{name}";
        }

        /// <summary>
        /// Selects the node index following the given trackItem in the current move path.
        /// </summary>
        private void OnUIMoveTrackSelect(TrackSelectDeleteItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            PathPlanner.SetNode(index+1);
        }

        /// <summary>
        /// Deletes the given track item's node in the move path.
        /// </summary>
        private void OnUIMoveTrackDelete(TrackSelectDeleteItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            PathPlanner.RemoveNode(index);
            PathPlanner.PlannedRoute();
        }

        #endregion

        #region Player Info

        /// <summary>
        /// Triggered whenever the manager wants to update the player info panel.
        /// Updates the current player info panel.
        /// </summary>
        private void Manager_OnPlayerInfoUpdate(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            CurrentPlayerInfo.UpdateInfo(currentPlayer);
        }

        /// <summary>
        /// Displays all player info panels.
        /// </summary>
        public void OnShowAllPlayerInfos()
        {
            var manager = Manager.Singleton;
            var players = manager.Players;

            // show root
            AllPlayerInfosRoot.gameObject.SetActive(true);

            for (int i = 0; i < AllPlayerInfos.Length; ++i)
            {
                // try and get player at index
                // if the player doesn't exist, then hide the card
                // otherwise update and show the card with the player info
                var player = players.ElementAtOrDefault(i);
                if (player == null)
                {
                    AllPlayerInfos[i].gameObject.SetActive(false);
                }
                else
                {
                    AllPlayerInfos[i].UpdateInfo(player);
                    AllPlayerInfos[i].gameObject.SetActive(true);
                }
            }

            // disable game interaction
            GameInput.CurrentContext = GameInput.Context.AllPlayers;
        }

        /// <summary>
        /// Hides all player info panels.
        /// </summary>
        public void OnHideAllPlayerInfos()
        {
            // hide root
            AllPlayerInfosRoot.gameObject.SetActive(false);

            // enable game interaction
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        #endregion

        #region End Game

        /// <summary>
        /// Triggered whenever the manager determines the game ends.
        /// Shows the end game panel.
        /// </summary>
        private void Manager_OnGameOver(Manager manager, int playerIdWon)
        {
            // set context
            GameInput.CurrentContext = GameInput.Context.EndGame;

            // ensure a real player won
            if (playerIdWon >= 0)
            {
                var playerWon = manager.Players[playerIdWon];

                // show panel
                PlayerWonPanel.gameObject.SetActive(true);

                // hide all other panels
                BuildInfoPanel.gameObject.SetActive(false);
                MoveInfoPanel.gameObject.SetActive(false);
                CurrentPlayerInfo.gameObject.SetActive(false);
                HelpPanel.SetActive(false);
                PhaseTurnTransitionPanel.gameObject.SetActive(false);

                // update player won info
                PlayerWonNameText.color = playerWon.color;
                PlayerWonNameText.text = playerWon.name;
                PlayerWonMoneyText.text = "$" + playerWon.money.ToString();
                PlayerWonCitiesText.text = playerWon.majorCities.ToString();

                // construct a list of all the other players, ordered by cities then by money
                var otherPlayers = manager.Players
                    .Where(x => x != playerWon)
                    .OrderBy(x => x.majorCities)
                    .ThenBy(x => x.money)
                    .ToList();

                // generate player entries for all the other players
                foreach (var otherPlayer in otherPlayers)
                {
                    var playerItem = Instantiate(EndGamePlayerItemPrefab);
                    playerItem.transform.SetParent(PlayerWonPlayersRoot, false);
                    playerItem.Name = otherPlayer.name;
                    playerItem.Money = otherPlayer.money;
                    playerItem.Cities = otherPlayer.majorCities;
                }
            }
            else
            {
                throw new NotImplementedException("Draws have not been implemented yet");
            }
        }

        /// <summary>
        /// Returns the user to the main menu.
        /// </summary>
        public void EndGame_Finish()
        {
            // load scene
            SceneManager.LoadScene("Main");
        }

        #endregion

        #region Phase Turn Transition

        /// <summary>
        /// Invoked whenever the turn ends.
        /// </summary>
        private void Manager_OnTurnEnd(Manager manager)
        {
            // find relevant phase info and open transition panel
            var phaseInfo = PhaseInfos.FirstOrDefault(x => x.Phase == manager.CurrentPhase);
            if (phaseInfo != null)
                OpenPhaseTurnTransition($"{phaseInfo.Name} Phase", manager.Player, phaseInfo.Description);
        }

        /// <summary>
        /// Invoked whenever the phase changes.
        /// </summary>
        private void Manager_OnPhaseChange(Manager manager)
        {
            switch (manager.CurrentPhase)
            {
                case Phase.Build:
                case Phase.InitBuild:
                case Phase.InitBuildRev:
                    {
                        BuildInfoPanel.gameObject.SetActive(true);
                        MoveInfoPanel.gameObject.SetActive(false);
                        break;
                    }
                case Phase.Move:
                    {
                        BuildInfoPanel.gameObject.SetActive(false);
                        MoveInfoPanel.gameObject.SetActive(true);
                        break;
                    }
            }

            // open transition panel
            var phaseInfo = PhaseInfos.FirstOrDefault(x => x.Phase == manager.CurrentPhase);
            if (phaseInfo != null)
                OpenPhaseTurnTransition($"{phaseInfo.Name} Phase", manager.Player, phaseInfo.Description);
        }

        /// <summary>
        /// Opens the phase turn transition panel with the given info.
        /// </summary>
        /// <param name="title">Title of the panel</param>
        /// <param name="player">Current player</param>
        /// <param name="subheading">Subheading text</param>
        /// <param name="duration">How long to show panel</param>
        private void OpenPhaseTurnTransition(string title, PlayerInfo player, string subheading, float duration = 3f)
        {
            // stop coroutine if already exists
            if (_phaseTurnTransitionCoroutine != null)
            {
                StopCoroutine(_phaseTurnTransitionCoroutine);
                _phaseTurnTransitionCoroutine = null;
            }

            // Begin coroutine
            StartCoroutine(_phaseTurnTransitionCoroutine = OpenPhaseTurnTransition_coroutine(title, player, subheading, duration));
        }

        /// <summary>
        /// Coroutine that opens the phase turn transition panel, updates the info, and waits the duration before automatically closing it.
        /// </summary>
        /// <param name="title">Title of the panel</param>
        /// <param name="player">Current player</param>
        /// <param name="subheading">Subheading text</param>
        /// <param name="duration">How long to show panel</param>
        private IEnumerator OpenPhaseTurnTransition_coroutine(string title, PlayerInfo player, string subheading, float duration)
        {
            // disable gameinput
            GameInput.CurrentContext = GameInput.Context.Popup;

            // open panel
            PhaseTurnTransitionPanel.gameObject.SetActive(true);
            PhaseTurnTransitionTitleText.text = title;
            PhaseTurnTransitionPlayerNameText.text = player.name;
            PhaseTurnTransitionPlayerNameText.color = player.color;
            PhaseTurnTransitionSubheadingText.text = subheading;

            // wait for duration
            yield return new WaitForSeconds(duration);

            // close panel if not already closed
            if (PhaseTurnTransitionPanel.gameObject.activeSelf)
            {
                // close panel
                PhaseTurnTransitionPanel.gameObject.SetActive(false);

                // enable gameinput
                GameInput.CurrentContext = GameInput.Context.Game;
            }

            // delete reference to coroutine now that it has finished
            _phaseTurnTransitionCoroutine = null;
        }

        /// <summary>
        /// Force closes the phase turn transition panel.
        /// </summary>
        public void PhaseTurnTransitionForceStop()
        {
            // stop existing coroutine
            if (_phaseTurnTransitionCoroutine != null)
            {
                StopCoroutine(_phaseTurnTransitionCoroutine);
                _phaseTurnTransitionCoroutine = null;
            }

            // close panel
            PhaseTurnTransitionPanel.gameObject.SetActive(false);

            // enable gameinput
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        #endregion

        #region Help Text

        /// <summary>
        /// Toggles the rendering of the help text.
        /// </summary>
        public void OpenHelpText()
        {
            // only open when game is active
            if (GameInput.CurrentContext != GameInput.Context.Game)
                return;

            GameInput.CurrentContext = GameInput.Context.Popup;
            HelpPanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Closes the help text panel.
        /// </summary>
        public void CloseHelpText()
        {
            GameInput.CurrentContext = GameInput.Context.Game;
            HelpPanel.gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the currently selected help text to the given id.
        /// </summary>
        public void SetHelpText(int helpStep)
        {
            HelpStep step = (HelpStep)helpStep;

            foreach (var item in HelpStepInfos)
            {
                // set item to active if step is the selected step
                // disable interaction with button if active
                var isActive = item.Step == step;
                item.Root.SetActive(isActive);
                item.NavButton.interactable = !isActive;
            }
        }

        #endregion

    }
}
