using Rails.Controls;
using Rails.Data;
using Rails.Systems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class GameHUDManager : MonoBehaviour
    {
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



        private Dictionary<NodeId, BuildMarkerContainer> _buildMarkers = new Dictionary<NodeId, BuildMarkerContainer>();
        private List<TrackItem> _uiTrackItems = new List<TrackItem>();
        private int _uiTrackSelectPathIndex = -1;
        private List<TrackSelectDeleteItem> _uiTrackSelectDeleteItems = new List<TrackSelectDeleteItem>();

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
        }

        private void Update()
        {
            Manager_OnBuildTrack(Manager.Singleton);

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
        }

        private void Manager_OnPlayerInfoUpdate(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            CurrentPlayerInfo.UpdateInfo(currentPlayer);
        }

        private void Manager_OnBuildTrack(Manager manager)
        {
            var players = manager.Players;
            var currentPlayer = players[manager.CurrentPlayer];
            var pathCount = PathPlanner.Paths;

            // clear markers
            foreach (var marker in _buildMarkers)
                marker.Value.ClearText();

            // Delete excess track item gameobjects
            while (pathCount < _uiTrackItems.Count)
            {
                DestroyUITrackItem(_uiTrackItems[_uiTrackItems.Count - 1]);
                _uiTrackItems.RemoveAt(_uiTrackItems.Count - 1);
            }

            // add missing tracks as empty
            while (pathCount > _uiTrackItems.Count)
                _uiTrackItems.Add(null);

            // populate markers
            for (int i = 0; i < pathCount; ++i)
            {
                var path = PathPlanner.GetPath(i);

                // update markers
                UpdateBuildMarkers(i, path);

                // update track ui
                var track = _uiTrackItems[i];
                UpdateUITrackItems(i, ref track, path);
                _uiTrackItems[i] = track;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DestroyUITrackItem(TrackItem trackItem)
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
        private void UpdateUITrackItems(int index, ref TrackItem trackItem, List<NodeId> path)
        {
            var trackName = Utilities.GetTrackNameByIndex(index);
            int i = 0;

            if (trackItem == null)
            {
                trackItem = Instantiate(TrackItemPrefab);
                trackItem.transform.SetParent(TracksRoot, false);
                trackItem.transform.SetSiblingIndex(index);
                trackItem.OnTrackSelected += OnUITrackSelect;
                trackItem.OnTrackDeleted += OnUITrackDelete;
            }

            trackItem.Name = $"{(PathPlanner.CurrentPath == index ? "<color=#FFFF00>" : "")}Track {Utilities.GetTrackNameByIndex(index)}";
            trackItem.Cost = $"{path.Count}"; // todo
        }

        private void OnUITrackSelect(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUITrackSelect(index, true);

            // Manager.Singleton.SetPath(index);
        }

        private void OnUITrackDelete(TrackItem trackItem)
        {
            var index = trackItem.transform.GetSiblingIndex();
            OpenUITrackSelect(index, false);

            // Manager.Singleton.ClearPath(index);
        }

        public void OnUITrackAddNew()
        {
            var index = PathPlanner.CreatePath();
            PathPlanner.SetPath(index);
        }

        public void OnUITrackSelectDeletePath()
        {
            PathPlanner.RemovePath(_uiTrackSelectPathIndex);

            // close
            OnUITrackSelectClose();
        }

        public void OnUITrackSelectStart()
        {
            // set selected node to
            PathPlanner.SetNode(_uiTrackSelectPathIndex, 0);

            // close
            OnUITrackSelectClose();
        }

        public void OnUITrackSelectClose()
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

        private void OpenUITrackSelect(int pathIndex, bool select)
        {
            // call close to ensure its closed before opening again
            OnUITrackSelectClose();

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
            int i = select ? 1 : 0;
            foreach (var nodeId in path)
            {
                var item = Instantiate(TrackSelectDeleteItemPrefab);
                item.transform.SetParent(TrackSelectItemsRoot, false);
                item.transform.SetSiblingIndex(i);
                item.IsSelect = select;
                item.Name = $"{pathName}{i + 1}";

                var iRef = i;

                item.OnTrackSelected += (track) =>
                {
                    // set selected node to
                    PathPlanner.SetNode(pathIndex, iRef);

                    // close
                    OnUITrackSelectClose();
                };
                item.OnTrackDeleted += (track) =>
                {
                    // remove
                    PathPlanner.RemoveNode(pathIndex, iRef);

                    // close
                    OnUITrackSelectClose();
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
        private void UpdateBuildMarkers(int index, List<NodeId> path)
        {
            var trackName = Utilities.GetTrackNameByIndex(index);
            var isTrackSelected = PathPlanner.CurrentPath == index;
            for (int i = 0; i < path.Count; ++i)
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
                if (isTrackSelected && i == PathPlanner.CurrentNode)
                    marker.AddName($"<color=#FFFF00>{name}</color>");
                else
                    marker.AddName($"{name}");

                // move to node
                marker.GameObject.transform.position = Utilities.GetPosition(nodeId);
            }
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
    }
}
