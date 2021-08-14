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
        [Serializable]
        public class PhaseInfo
        {
            public Phase Phase;
            public string Name;
            public string Description;
        }

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

        private Dictionary<NodeId, BuildMarkerContainer> _buildMarkers = new Dictionary<NodeId, BuildMarkerContainer>();
        private List<TrackItem> _uiBuildTrackItems = new List<TrackItem>();
        private int _uiTrackSelectPathIndex = -1;
        private List<TrackSelectDeleteItem> _uiTrackSelectDeleteItems = new List<TrackSelectDeleteItem>();
        private List<TrackSelectDeleteItem> _uiMoveTrackItems = new List<TrackSelectDeleteItem>();
        private TrainCityInteraction _cityPickDropInteraction;

        private class BuildMarkerContainer
        {
            public NodeId NodeId { get; set; }
            public GameObject GameObject { get; set; }
            public TMPro.TMP_Text TextComponent { get; set; }

            public List<string> NodeNames { get; } = new List<string>();

            public void AddName(string name)
            {
                NodeNames.Add(name);
                UpdateText();
            }

            public void RemoveName(string name)
            {
                NodeNames.Remove(name);
                UpdateText();
            }

            public void UpdateText()
            {
                TextComponent.text = string.Join(",", NodeNames);
            }

            public void ClearText()
            {
                NodeNames.Clear();
                UpdateText();
            }
        }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            var manager = Manager.Singleton;
            if (!manager)
                return;

            manager.OnPlayerInfoUpdate += Manager_OnPlayerInfoUpdate;
            Manager_OnPlayerInfoUpdate(manager);
            manager.OnPhaseChange += Manager_OnPhaseChange;
            Manager_OnPhaseChange(manager);
            manager.OnTrainMeetsCityHandler += Manager_OnTrainMeetsCity;
            manager.OnGameOver += Manager_OnGameOver;
            manager.OnTurnEnd += Manager_OnTurnEnd;
        }

        private void Manager_OnTurnEnd(Manager manager)
        {
            // open transition panel
            var phaseInfo = PhaseInfos.FirstOrDefault(x => x.Phase == manager.CurrentPhase);
            if (phaseInfo != null)
                OpenPhaseTurnTransition($"{phaseInfo.Name} Phase", manager.Player, phaseInfo.Description);
        }

        private void Update()
        {
            var manager = Manager.Singleton;
            switch (manager.CurrentPhase)
            {
                case Phase.Build:
                case Phase.InitBuild:
                case Phase.InitBuildRev:
                    {
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
                        Manager_OnMoveTrackUpdated(Manager.Singleton);
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

        #region City PickDrop


        private void Manager_OnTrainMeetsCity(object sender, TrainCityInteraction e)
        {
            CityPickDropPanel.gameObject.SetActive(true);
            GameInput.CurrentContext = GameInput.Context.Popup;
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
                var demand = e.Cards.ElementAtOrDefault(i)?.FirstOrDefault(x => x.City == e.City);

                if (demand == null)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
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

        public void CityPickDrop_Validate()
        {
            bool isValid = true;
            string invalidMessage = "";
            var manager = Manager.Singleton;
            var player = manager.Players[_cityPickDropInteraction.PlayerIndex];
            var playerGoodsCarried = player.goodsCarried.ToList();

            // 
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

            // 
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

        public void BuildTrack()
        {
            Manager.Singleton.BuildTrack();
        }

        public void EndBuild()
        {
            Manager.Singleton.EndBuild();
        }

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

        public void UpgradeCancel()
        {
            UpgradePanel.gameObject.SetActive(false);
        }

        public void UpgradeTrain(int value)
        {
            if (Manager.Singleton.UpgradeTrain(value))
                UpgradePanel.gameObject.SetActive(false);
        }

        #endregion

        #region Move Panel

        public void MoveTrain()
        {
            Manager.Singleton.MoveTrain();
        }

        public void DiscardHand()
        {
            Manager.Singleton.DiscardHand();
        }

        public void EndMove()
        {
            Manager.Singleton.EndMove();
        }

        #endregion

        #region Build Track UI

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
        /// 
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
        /// 
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

        private void OnUIBuildTrackSelect(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUIBuildTrackSelect(index, true);

            // Manager.Singleton.SetPath(index);
        }

        private void OnUIBuildTrackDelete(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUIBuildTrackSelect(index, false);

            // Manager.Singleton.ClearPath(index);
        }

        public void OnUIBuildTrackAddNew()
        {
            var index = PathPlanner.CreatePath();
            PathPlanner.SetPath(index);
        }

        public void OnUIBuildTrackSelectDeletePath()
        {
            PathPlanner.RemovePath(_uiTrackSelectPathIndex);

            // close
            OnUIBuildTrackSelectClose();
        }

        public void OnUIBuildTrackSelectStart()
        {
            // set selected node to
            PathPlanner.SetPath(_uiTrackSelectPathIndex);
            PathPlanner.SetNode(_uiTrackSelectPathIndex, 0);

            // close
            OnUIBuildTrackSelectClose();
        }

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
        /// <param name="index"></param>
        /// <param name="path"></param>
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

        private void DestroyMoveTrackItem(TrackSelectDeleteItem trackItem)
        {
            // destroy
            if (trackItem)
            {
                Destroy(trackItem.gameObject);
            }
        }

        /// <summary>
        /// 
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

        private void OnUIMoveTrackSelect(TrackSelectDeleteItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            PathPlanner.SetNode(index+1);
        }

        private void OnUIMoveTrackDelete(TrackSelectDeleteItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            PathPlanner.RemoveNode(index);
            PathPlanner.PlannedRoute();
        }

        #endregion

        #region Player Info

        private void Manager_OnPlayerInfoUpdate(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            CurrentPlayerInfo.UpdateInfo(currentPlayer);
        }

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

        public void OnHideAllPlayerInfos()
        {
            // hide root
            AllPlayerInfosRoot.gameObject.SetActive(false);

            // enable game interaction
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        #endregion

        #region End Game

        private void Manager_OnGameOver(Manager manager, int playerIdWon)
        {
            // set context
            GameInput.CurrentContext = GameInput.Context.EndGame;

            if (playerIdWon >= 0)
            {
                var playerWon = manager.Players[playerIdWon];

                // show panel
                PlayerWonPanel.gameObject.SetActive(true);

                // hide all other panels
                BuildInfoPanel.gameObject.SetActive(false);
                MoveInfoPanel.gameObject.SetActive(false);
                CurrentPlayerInfo.gameObject.SetActive(false);

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
        }

        public void EndGame_Finish()
        {
            // load scene
            SceneManager.LoadScene("Main");
        }

        #endregion

        #region Phase Turn Transition

        private void OpenPhaseTurnTransition(string title, PlayerInfo player, string subheading, float duration = 3f)
        {
            StartCoroutine(OpenPhaseTurnTransition_coroutine(title, player, subheading, duration));
        }

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

            // close panel
            PhaseTurnTransitionPanel.gameObject.SetActive(false);

            // enable gameinput
            GameInput.CurrentContext = GameInput.Context.Game;
        }

        #endregion
    }
}
