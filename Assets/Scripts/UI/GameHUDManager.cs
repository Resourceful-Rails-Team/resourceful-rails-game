using Rails.Controls;
using Rails.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Rails.UI
{
    public class GameHUDManager : MonoBehaviour
    {
        [Header("Other References")]
        public Transform WorldCanvas;

        [Header("Basic")]
        public TMPro.TMP_Text PlayerNameText;
        public TMPro.TMP_Text PlayerMoneyText;
        public TMPro.TMP_Text PlayerCitiesText;

        [Header("Train")]
        public TMPro.TMP_Text TrainNameText;
        public Image TrainIconImage;
        public TMPro.TMP_Text TrainUpgradeTextUpper;
        public TMPro.TMP_Text TrainUpgradeTextLower;

        [Header("Goods")]
        public IconValueItem[] Goods;

        [Header("Cards")]
        public CardItem[] Cards;

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
        }

        private void Manager_OnPhaseChange(Manager manager)
        {

        }

        private void Manager_OnPlayerInfoUpdate(Manager manager)
        {
            var currentPlayerIndex = manager.CurrentPlayer;
            var currentPlayer = manager.Players[currentPlayerIndex];

            // update basic info
            this.PlayerNameText.text = currentPlayer.name;
            this.PlayerMoneyText.text = $"{currentPlayer.money}";
            this.PlayerCitiesText.text = $"{currentPlayer.majorCities}";

            // update train
            var trainSpecs = manager._rules.TrainSpecs[currentPlayer.trainStyle];
            this.TrainNameText.text = currentPlayer.trainStyle.ToString();
            this.TrainUpgradeTextUpper.text = $"{0}";
            this.TrainUpgradeTextLower.text = $"{trainSpecs.movePoints}";

            // update demand cards
            for (int i = 0; i < Cards.Length; ++i)
            {
                if (i < currentPlayer.demandCards.Count)
                {
                    Cards[i].gameObject.SetActive(true);
                    foreach (var demandCard in currentPlayer.demandCards)
                    {
                        int d = 0;
                        foreach (var demand in demandCard)
                        {
                            Cards[i].SetDemand(d, demand);
                            ++d;
                        }
                    }
                }
                else
                {
                    Cards[i].gameObject.SetActive(false);
                }
            }

            // update goods
            SetGoods(currentPlayer.goodsCarried);
        }

        private void Manager_OnBuildTrack(Manager manager)
        {
            var players = manager.Players;
            var currentPlayer = players[manager.CurrentPlayer];
            var pathCount = manager.GetPathCount();

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
                var path = manager.GetPath(i);

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

            trackItem.Name = $"{(Manager.Singleton.CurrentPath == index ? "<color=#FFFF00>" : "")}Track {Utilities.GetTrackNameByIndex(index)}";
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
            var manager = Manager.Singleton;
            var index = manager.CreateNewPath();
            manager.SetPath(index);
        }

        public void OnUITrackSelectDeletePath()
        {
            var manager = Manager.Singleton;
            manager.RemovePath(_uiTrackSelectPathIndex);

            // close
            OnUITrackSelectClose();
        }

        public void OnUITrackSelectStart()
        {
            var manager = Manager.Singleton;

            // set selected node to
            manager.SetNode(_uiTrackSelectPathIndex, 0);

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
            GameInput.IsEnabled = true;
        }

        private void OpenUITrackSelect(int pathIndex, bool select)
        {
            // call close to ensure its closed before opening again
            OnUITrackSelectClose();

            // prevent game interactions
            GameInput.IsEnabled = false;

            // set path index
            _uiTrackSelectPathIndex = pathIndex;

            // show panel
            TrackSelectPanel.gameObject.SetActive(true);

            // show delete if not select
            TrackSelectDeleteButtonRoot.gameObject.SetActive(!select);
            TrackSelectStartRoot.gameObject.SetActive(select);

            // add items
            var manager = Manager.Singleton;
            var path = manager.GetPath(pathIndex);
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
                    // set current path to
                    manager.SetPath(pathIndex);

                    // set selected node to
                    manager.SetNode(pathIndex, iRef);

                    // close
                    OnUITrackSelectClose();
                };
                item.OnTrackDeleted += (track) =>
                {
                    // remove
                    manager.RemoveNode(pathIndex, iRef);

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
            var isTrackSelected = Manager.Singleton.CurrentPath == index;
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
                if (isTrackSelected && i == 0)
                    marker.AddName($"<color=#FFFF00>{name}</color>");
                else
                    marker.AddName($"{name}");

                // move to node
                marker.GameObject.transform.position = Utilities.GetPosition(nodeId);
            }
        }

        public void SetGoods(IEnumerable<Good> values)
        {
            var valuesArr = values.ToArray();
            for (int i = 0; i < Goods.Length; ++i)
            {
                var good = Goods[i];

                if (i < valuesArr.Length)
                {
                    good.Value = valuesArr[i].Name;
                    good.Sprite = valuesArr[i].Icon;
                }
                else
                {
                    good.Sprite = null;
                    good.Value = null;
                    good.Disabled = true;
                }
            }
        }
    }
}
