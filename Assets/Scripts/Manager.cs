/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using TMPro;
using Rails.Controls;
using Rails.Collections;
using Rails.Data;
using Rails.Rendering;
using Rails.ScriptableObjects;
using Rails.Systems;
using Rails.UI;
using Assets.Scripts.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rails
{
    public class Manager : MonoBehaviour
    {

        #region Events
        // Turn End
        public delegate void OnTurnEndEventHandler(Manager manager);
        public event OnTurnEndEventHandler OnTurnEnd;

        // Player Info Update
        public delegate void OnPlayerInfoUpdateEventHandler(Manager manager);
        public event OnPlayerInfoUpdateEventHandler OnPlayerInfoUpdate;

        // Phase Change
        public delegate void OnPhaseChangeHandler(Manager manager);
        public event OnTurnEndEventHandler OnPhaseChange;

        // Build Track
        public delegate void OnBuildTrackHandler(Manager manager);
        public event OnTurnEndEventHandler OnBuildTrack;

        // Game over
        public delegate void OnGameOverHandler(Manager manager, int playerIdWon);
        public event OnGameOverHandler OnGameOver;
        
        // Invoked when a moving train meets a City
        public EventHandler<TrainCityInteraction> OnTrainMeetsCityHandler;

        // Invoked when the UI has finished loading a train at a given City
        public EventHandler<TrainCityInteractionResult> OnTrainMeetsCityComplete;

        #endregion

        #region Singleton

        private static Manager _singleton = null;

        /// <summary>
        /// Manager singleton
        /// </summary>
        public static Manager Singleton
        {
            get
            {
                if (_singleton)
                    return _singleton;

                _singleton = FindObjectOfType<Manager>();
                if (_singleton)
                    return _singleton;

                GameObject go = new GameObject("Manager");
                return go.AddComponent<Manager>();
            }
        }

        #endregion

        #region Public Fields
        /// <summary>
        /// Map size.
        /// </summary>
        public const int Size = 64;
        /// <summary>
        /// Max number of cities.
        /// </summary>
        public const int MaxCities = 32;
        /// <summary>
        /// Max number of goods.
        /// </summary>
        public const int MaxGoods = 64;
        /// <summary>
        /// The value representing a MajorCity NodeType.
        /// Used when setting down initial MajorCity tracks,
        /// Tracks values and ghost tracks.
        /// </summary>
        public const int MajorCityIndex = -2;

        /// <summary>
        /// Controls the spacing between nodes in terms of Unity units.
        /// </summary>
        public float WSSize = 1f;
        /// <summary>
        /// For creating all of the city labels.
        /// </summary>
        public CityLabel _cityLabelPrefab;

        /// <summary>
        /// A collection of game rules.
        /// </summary>
        public GameRules Rules;
        /// <summary>
        /// Stores the layout of the map, including nodes, cities, goods, etc.
        /// </summary>
        [SerializeField]
        public MapData MapData;
        /// <summary>
        /// Rules set at the start of the game.
        /// </summary>
        public GameStartRules _startRules;
        /// <summary>
        /// UI windows that show the controls for each phase.
        /// </summary>
        public GameObject[] PhasePanels;
        /// <summary>
        /// UI window that shows stats of the current player.
        /// </summary>
        public GameHUDManager GameHUDObject;
        /// <summary>
        /// Stores info for all players.
        /// </summary>
        public PlayerInfo[] Players;

        /// <summary>
        /// Gets the player info for the current player.
        /// </summary>
        public PlayerInfo Player
        {
            get { return player; }
        }
        /// <summary>
        /// Gets the index of the current player.
        /// </summary>
        public int CurrentPlayer
        {
            get { return currentPlayer; }
        }
        ///<summary>
        /// Gets the current phase of the turn.
        ///</summary>
        public Phase CurrentPhase
        {
            get { return currentPhase; }
        }
        #endregion // Properties

        #region Private Fields
        /// <summary>
        /// Stores the tracks on the map.
        /// </summary>
        private static TrackGraph<int> Tracks = new TrackGraph<int>(-1);
        /// <summary>
        /// For rotating the labels so they stay in line with the camera.
        /// </summary>
        private List<CityLabel> cityLabels;
        /// <summary>
        /// A reference to the current players info.
        /// </summary>
        private PlayerInfo player;
        /// <summary>
        /// Player number whose turn it currently is.
        /// </summary>
        private int currentPlayer;
        /// <summary>
        /// Phase of the turn of current player.
        /// </summary>
        private Phase currentPhase;
        /// <summary>
        /// Token that is to be highlighted when selected.
        /// </summary>
        private GameToken _highlightToken;
        /// <summary>
        /// To stop the same train from moving multiple times at once.
        /// </summary>
        private bool _movingTrain = false;
        /// <summary>
        /// To determine whether a player has paid for another's track this turn
        /// </summary>
        private bool[] _altTracksPaid = new bool[6];
        #endregion

        #region Unity Events

        private void Awake()
        {
            // set singleton reference on awake
            _singleton = this;
            _startRules = FindObjectOfType<GameStartRules>();
            Rules = MapData.DefaultRules;

            // generate start rules if empty
            if (_startRules == null)
            {
                GameObject go = new GameObject("start rules");
                _startRules = go.AddComponent<GameStartRules>();
                _startRules.Players = new StartPlayerInfo[2]
                {
                    new StartPlayerInfo() { Name = "Player 1", Color = Color.red },
                    new StartPlayerInfo() { Name = "Player 2", Color = Color.blue }
                };
            }

            OnTrainMeetsCityComplete += (_, result) => CompleteCityTransaction(result);
        }
 
        private void Start()
        {
            GameGraphics.Initialize(MapData, _startRules.Players.Length, _startRules.Players.Select(p => p.Color).ToArray());
            Pathfinding.Initialize(Rules, Tracks, MapData);
            PathPlanner.Initialize();
            Deck.Initialize();
            GoodsBank.Initialize();
            GenerateCityLabels();
            SetupMajorCityTracks();
            GameLoopSetup();
        }

        private void Update()
        {
            // Highlights game pieces of interest
            _highlightToken?.ResetColor();
            var highlightToken = GameGraphics.GetMapToken(GameInput.MouseNodeId);

            if (highlightToken != null)
            {
                highlightToken.Color = Color.yellow;
                _highlightToken = highlightToken;
            }

            // Rotate the city labels to match the camera
            RotateCityLabels();
            // Keep checking inputs
            Loop();
        }

        /// <summary>
        /// Checks all of the inputs every frame.
        /// </summary>
        private void Loop()
        {
            // Prevent train from being moved twice.
            if (!_movingTrain)
            {
                // Check the main inputs
                if (GameInput.SelectJustPressed && GameInput.MouseNodeId.InBounds)
                {
                    Debug.Log("player.trainPosition = " + player.trainPosition.ToString());

                    if (currentPhase == Phase.Move)
                    {
                        // For the movement phase we check if the train has been placed yet.
                        if (!player.trainPlaced)
                        {
                            Debug.Log("Place Train");
                            PlaceTrain(GameInput.MouseNodeId);
                        }
                        else
                        {
                            // Adds a node to the players movement path.
                            PathPlanner.AddNode(GameInput.MouseNodeId);
                        }
                    }
                    else
                    {
                        // Adds a node to the general build path..
                        Debug.Log("Add Node");
                        PathPlanner.AddNode(PathPlanner.CurrentPath, GameInput.MouseNodeId);
                    }
                }

                // Extra controls
                if (GameInput.DeleteJustPressed)
                {
                    if (currentPhase == Phase.Move)
                        PathPlanner.ClearPath();
                    else
                        PathPlanner.ClearPath(PathPlanner.CurrentPath);
                }
                if (GameInput.EnterJustPressed)
                {
                    if (currentPhase == Phase.Move)
                        MoveTrain();
                    else
                        BuildTrack();
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws all of the map editor nodes.
        /// </summary>
        private void OnDrawGizmos()
        {
            List<Action> postDraws = new List<Action>();
            if (MapData == null || MapData.Nodes == null || MapData.Nodes.Length == 0)
                return;

            var labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.alignment = TextAnchor.UpperCenter;
            labelStyle.fontSize = 16;
            labelStyle.fontStyle = FontStyle.Bold;

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    // draw node
                    var node = MapData.Nodes[(y * Size) + x];
                    var pos = Utilities.GetPosition(node.Id);
                    Gizmos.color = Utilities.GetNodeColor(node.Type);
                    Gizmos.DrawCube(pos, Vector3.one * WSSize * 0.3f);

                    //
                    if (node.CityID >= 0 && node.CityID < MapData.Cities.Count)
                    {
                        var city = MapData.Cities[node.CityID];
                        if (node.Type == NodeType.MajorCity || node.Type == NodeType.MediumCity || node.Type == NodeType.SmallCity)
                        {

                            postDraws.Add(() => {
                                Handles.Label(pos + Vector3.up, city.Name, labelStyle);
                            });

                        }
                    }

                    // draw segments
                    // we iterate only bottom-right half of segments to prevent drawing them twice
                    var segments = MapData.GetNodeSegments(node.Id);
                    for (Cardinal c = Cardinal.NE; c <= Cardinal.S; ++c)
                    {
                        // get segment
                        var segment = segments[(int)c];
                        if (segment != null)
                        {
                            // get neighboring nodeid
                            var nextNodeId = Utilities.PointTowards(node.Id, c);
                            if (nextNodeId.InBounds)
                            {
                                // draw line to
                                Gizmos.color = Utilities.GetSegmentColor(segment.Type);
                                Gizmos.DrawLine(pos, Utilities.GetPosition(nextNodeId));
                            }
                        }
                    }
                }
            }

            foreach (var postDraw in postDraws)
                postDraw?.Invoke();
        }
#endif
        #endregion

        #region Public
        // General gameplay methods.

        /// <summary>
        /// Moves the train via the determined route. The train
        /// will move as far as it can before ending the player's
        /// Move phase.
        /// </summary>
        public void MoveTrain()
        {
            if (!_movingTrain)
                StartCoroutine(CMoveTrain());
        }
        /// <summary>
        /// Coroutine for MoveTrain. Provides graphical representation
        /// of train movement, and invokes interaction between the train and cities
        /// </summary>
        private IEnumerator CMoveTrain()
        {
            // Retrieve the move route from PathPlanner
            var moveRoute = PathPlanner.moveRoute;  

            // Remove the player's current position from
            // their move path
            Player.movePath.RemoveAt(0);

            _movingTrain = true;

            // While the route still has nodes to traverse
            for(int i = 0; i < moveRoute.Distance; ++i)
            {
                // Move the player train position, and subtract from their move points
                Player.trainPosition = moveRoute.Nodes[i + 1];
                Player.movePointsLeft -= 1;
                
                // Show the movement of the train, awaiting until it moves to the next node.
                GameGraphics.MoveTrain(currentPlayer, moveRoute.Nodes[i], moveRoute.Nodes[i + 1]);
                yield return null;

                while (GameGraphics.IsTrainMoving) yield return null;
                
                // If the player passes one of its path nodes, remove it
                if (moveRoute.Nodes[i + 1] == Player.movePath[0])
                    Player.movePath.RemoveAt(0);
                
                // Determine if the player is at a city, and that it's not a major
                // city it has already visited
                var stop = PathPlanner.GetStop(moveRoute.Nodes[i + 1]);
                var previousCityIndex = MapData.Nodes[moveRoute.Nodes[i].GetSingleId()].CityID;
            
                // If so, invoke the UI to respond to dropoff / pickup
                if (stop != null && MapData.Cities.IndexOf(stop.City) != previousCityIndex)
                {
                    OnTrainMeetsCityHandler?.Invoke(this, stop);
                    break;
                }

                int trackOwner = Tracks[moveRoute.Nodes[i], moveRoute.Nodes[i + 1]];
                if (trackOwner != MajorCityIndex && !_altTracksPaid[trackOwner])
                {
                    _altTracksPaid[trackOwner] = true;
                    Player.money -= Rules.AltTrackCost;
                    OnPlayerInfoUpdate?.Invoke(this);
                }

                if (Player.movePointsLeft == 0) break;
            }
            _movingTrain = false;

            // Reinsert the player's new train position to the beginning of the list
            Player.movePath.Insert(0, Player.trainPosition);
            
            PathPlanner.PlannedRoute();
            PathPlanner.SetNode(Player.movePath.Count);
   
            // End the phase if the player is out of move points
            if (Player.movePointsLeft == 0)
            {
                EndMove();
            } 
        }

        /// <summary>
        /// Discards the player's hand and draws 3 new cards.
        /// Can only be done once and turn and the player can't do anything else.
        /// </summary>
        public void DiscardHand()
        {
            // You can't discard unless you haven't moved yet.
            if (player.movePointsLeft != Rules.TrainSpecs[player.trainType].movePoints)
            {
                Debug.Log("You cannot discard after moving the train.");
                return;
            }

            // Remove and refill players' hand
            foreach (DemandCard card in player.demandCards) {
                Deck.Discard(card);
            }
            player.demandCards.Clear();
            for (int c = 0; c < Rules.HandSize; c++)
            {
                player.demandCards.Add(Deck.DrawOne()); 
            }

            // Ends the turn.
            GameGraphics.HighlightRoute(PathPlanner.moveRoute, null);
            GameLogic.UpdatePhase(PhasePanels, ref currentPhase);
            PathPlanner.CurrentNode = 0;
            EndTurn();
        }

        /// <summary>
        /// Builds the track between the nodes in the build path in PathPlanner.
        /// </summary>
        /// <returns>If the track was built.</returns>
        public bool BuildTrack()
        {
            // Build the tracks
            if (PathPlanner.Paths != 0)
            {
                // Make sure the cost of the track is lower than both the limit and the players bank.
                if (PathPlanner.CurrentCost > Rules.MaxBuild)
                    return false;
                if (PathPlanner.CurrentCost > player.money)
                    return false;

                // Build the track and trigger the UI.
                player.money -= GameLogic.BuildTrack(Tracks, PathPlanner.buildRoutes, currentPlayer);
                OnBuildTrack?.Invoke(this);
            }

            // Keep track of major cities to determine the winner.
            Player.majorCities = CountMajorCities();

            // End the turn
            PathPlanner.ClearBuild();
            EndTurn();
            return true;
        }

        /// <summary>
        /// Upgrade the player's train given a choice. Game Logic will make sure they player has enough money.
        /// </summary>
        /// <param name="choice"></param>
        /// <returns>If the upgrade was successful.</returns>
        public bool UpgradeTrain(int choice)
        {
            bool success = GameLogic.UpgradeTrain(ref player.trainType, ref player.money, choice, Rules.TrainUpgrade);
            if (success)
                EndTurn();
            return success;
        }

        /// <summary>
        /// For inital placement of a players train. Must be placed on a city node.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>If the placement was successful.</returns>
        public bool PlaceTrain(NodeId position)
        {
            NodeType type = MapData.GetNodeAt(position).Type;
            bool city = false;

            // Check to make sure the given node is a city.
            switch (type)
            {
                case NodeType.SmallCity:
                case NodeType.MediumCity:
                case NodeType.MajorCity:
                    city = true;
                    break;
            }

            // If it is, place the train there and set up the move path.
            if (city)
            {
                GameGraphics.PositionTrain(currentPlayer, position);
                player.trainPosition = position;
                player.movePath = new List<NodeId> { position };
                player.trainPlaced = true;
                PathPlanner.PlannedRoute();
                Debug.Log("Train position = " + player.trainPosition.ToString());
            }

            return city;
        }

        /// <summary>
        /// Ends the move phase without moving.
        /// </summary>
        public void EndMove()
        {
            // De-highlight the route and update the phase.
            GameGraphics.HighlightRoute(PathPlanner.moveRoute, null);
            GameLogic.UpdatePhase(PhasePanels, ref currentPhase);
            OnPhaseChange?.Invoke(this);
            PathPlanner.CurrentNode = 0;
            return;
        }

        /// <summary>
        /// Ends the build phase without building anything.
        /// </summary>
        public void EndBuild()
        {
            // End the turn
            PathPlanner.ClearBuild();
            EndTurn();
        }


        #endregion

        #region Private
        /// <summary>
        /// Sets up the current game.
        /// </summary>
        private void GameLoopSetup()
        {
            // Assign integers
            currentPlayer = 0;
            currentPhase = Phase.InitBuild;

            // Initiate all player info.
            Players = new PlayerInfo[_startRules.Players.Length];
            for (int p = 0; p < Players.Length; p++)
                Players[p] = new PlayerInfo(_startRules.Players[p].Name,
                  _startRules.Players[p].Color, Rules.MoneyStart, 0);
            player = Players[currentPlayer];

            // Draw all players' cards.
            for (int c = 0; c < Rules.HandSize; c++)
            {
                for (int p = 0; p < Players.Length; p++)
                {
                    Players[p].demandCards.Add(Deck.DrawOne());
                }
            }

            // Deactivate all panels just in case.
            for (int u = 0; u < (int)Phase.MAX; u++)
                PhasePanels[u].SetActive(false);

            // Activate first turn panel.
            PhasePanels[1].SetActive(true);
        }

        /// <summary>
        /// Creates, places, and modifies a city node for each city on the map.
        /// </summary>
        private void GenerateCityLabels()
        {
            // Create the parent of all labels.
            GameObject parent = Instantiate(new GameObject());
            parent.transform.name = "City Labels";
            cityLabels = new List<CityLabel>();

            // Create a label for each city.
            foreach (City city in MapData.Cities)
            {
                // Find the location of the city.
                var CityId = MapData.Cities.IndexOf(city);
                var cityLocations = MapData.LocationsOfCity(city);
                var cityNodeId = cityLocations[0];

                // Special case of Major city since they have 8 nodes each rather than 1.
                if (MapData.GetNodeAt(cityNodeId).Type == NodeType.MajorCity)
                {
                    cityNodeId = MapData.LocationsOfCity(city)
                        .First(x => MapData.GetNeighborNodes(x).
                        All(nn => nn.Item2.CityID == CityId && nn.Item2.Type == NodeType.MajorCity));

                }

                // Create the label.
                Vector3 pos = Utilities.GetPosition(cityNodeId);
                pos.y += 1;
                CityLabel lab = Instantiate(_cityLabelPrefab);
                lab.transform.name = "Label: " + city.Name;
                lab.transform.SetParent(parent.transform);
                lab.Set(pos, city);
                cityLabels.Add(lab);
            }
            return;
        }

        /// <summary>
        /// Rotates the labels each frame so they're readable.
        /// </summary>
        private void RotateCityLabels()
        {
            foreach (CityLabel lab in cityLabels)
            {
                lab.transform.rotation = Camera.main.transform.rotation;
            }
        }

        /// <summary> 
        /// Ends the turn and changes phase.
        /// </summary>
        private void EndTurn()
        {
            // Check to see if the player won that turn.
            if (PlayerWon())
            {
                OnGameOver?.Invoke(this, CurrentPlayer);
                return;
            }

            // Special Build Turn only logic.
            if (currentPhase < Phase.Move)
            {
                GameLogic.BuildTurn(ref currentPlayer, ref currentPhase, Players.Length);
            }
            else
            {
                GameLogic.IncrementPlayer(ref currentPlayer, Players.Length);
            }

            // Applies on each Normal Turn.
            if (currentPhase >= Phase.Move)
            {
                GameLogic.UpdatePhase(PhasePanels, ref currentPhase);
                OnPhaseChange?.Invoke(this);
            }

            // Update the player info and current player reference.
            player = Players[currentPlayer];
            OnPlayerInfoUpdate?.Invoke(this);
            OnTurnEnd?.Invoke(this);

            // Set up move path
            if(CurrentPhase == Phase.Move)
                InitializePlayerMove();

            return;
        }

        /// <summary>
        /// Sets up player movement for the next turn.
        /// </summary>
        private void InitializePlayerMove()
        {
            // Check movement points remaining.
            Player.movePointsLeft = Rules.TrainSpecs[Player.trainType].movePoints;

            // Adapt the move path for next turn.
            if (Player.movePath.Count == 0)
            {
                Player.movePath.Add(Player.trainPosition);
                PathPlanner.SetNode(1);
            }
            else
                PathPlanner.SetNode(Player.movePath.Count);

            // Set if an opponents track was used and the cost paid.
            _altTracksPaid = new bool[6];
            _altTracksPaid[CurrentPlayer] = true;

            PathPlanner.PlannedRoute();
        }

        
        /// <summary>
        /// Sets up the tracks that are automatically assigned to
        /// a Major City
        /// </summary>
        private void SetupMajorCityTracks()
        {
            for (int i = 0; i < Size * Size; ++i)
            {
                if (MapData.Nodes[i].Type == NodeType.MajorCity)
                {
                    // If the selected Node is a major city, and if no track exists
                    // with it's Cardinal neighbors, create a track.
                    var nodeId = NodeId.FromSingleId(i);
                    for (var c = Cardinal.N; c < Cardinal.MAX_CARDINAL; ++c)
                    {
                        var adjNodeId = Utilities.PointTowards(nodeId, c);
                        if (MapData.Nodes[adjNodeId.GetSingleId()].Type == NodeType.MajorCity &&
                           (!Tracks.TryGetEdgeValue(nodeId, c, out int e) || e == -1))
                        {
                            Tracks[nodeId, adjNodeId] = MajorCityIndex;
                            var route = new Route(0, new List<NodeId> { nodeId, adjNodeId });

                            GameGraphics.GeneratePotentialTrack(route, Color.clear);
                            GameGraphics.CommitPotentialTrack(route, Color.clear);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Completes a city transaction, returned by UI, removing the specified loads
        /// and adding the specified funds to the player's account.
        /// </summary>
        private void CompleteCityTransaction(TrainCityInteractionResult result)
        {
            var playerCityId = MapData.Nodes[Player.trainPosition.GetSingleId()].CityID;

            if (result.ChosenCards != null)
            {
                var income = 0;
                foreach (var card in result.ChosenCards)
                {
                    var match = card.FirstOrDefault(demand => demand.City == MapData.Cities[playerCityId]);
                    if (match != null)
                    {
                        income += match.Reward;
                        GoodsBank.GoodDropoff(Player.goodsCarried.IndexOf(match.Good), Player.goodsCarried);

                        // Update the Deck, and the player's demand cards
                        Deck.Discard(card);
                        Player.demandCards.Remove(card);
                        Player.demandCards.Add(Deck.DrawOne());

                        Player.money += income;
                    }
                }
            }
            if (result.Goods != null)
            {
                foreach (var good in result.Goods)
                    GoodsBank.GoodPickup(good, Player.goodsCarried, Player.trainType);
            }
            OnPlayerInfoUpdate?.Invoke(this);
        }
      
        /// <summary>
        /// Counts the number of major cities the player has built to
        /// in one continuous line of track.
        /// </summary>
        /// <returns>The number of Major cities.</returns>
        private int CountMajorCities()
        {
            var connected =
                Tracks.GetConnected(
                    (id) => MapData.Nodes[id.GetSingleId()].CityID, currentPlayer, MajorCityIndex
                );

            if(connected.Length > 0)
                return connected.Max(
                    g => g
                    .Where(id => id != -1)
                    .Where(id => MapData.GetCityType(MapData.Cities[id]) == NodeType.MajorCity)
                    .Count()
                );

            return 0;
        }

        /// <summary>
        /// Checks if the player has won.
        /// </summary>
        /// <returns>Win status.</returns>
        private bool PlayerWon() => Player.majorCities >= Rules.WinMajorCities && Player.money >= Rules.WinMoney;

        #endregion
    }
}