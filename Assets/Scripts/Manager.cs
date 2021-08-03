using System;
using System.Collections.Generic;
using UnityEngine;
using Rails.ScriptableObjects;
using System.Collections.ObjectModel;
using Rails.Rendering;
using Rails.Controls;
using Rails.Data;
using Rails.Systems;
using Rails.Collections;
using System.Linq;
using Assets.Scripts.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rails {
  public class Manager : MonoBehaviour {
    #region Singleton

    private static Manager _singleton = null;

    /// <summary>
    /// Manager singleton
    /// </summary>
    public static Manager Singleton {
      get {
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

    #region Map
    #region Properties
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
    /// Controls the spacing between nodes in terms of Unity units.
    /// </summary>
    public float WSSize = 1f;

    /// <summary>
    /// Stores the layout of the map, including nodes, cities, goods, etc.
    /// </summary>
    [SerializeField]
    public MapData MapData;


    #endregion // Properties
    private GameRules _rules;

    /// <summary>
    /// Stores the tracks on the map.
    /// </summary>
    private static TrackGraph<int> Tracks = new TrackGraph<int>(-1);

    #endregion // Map

    #region Unity Events

    private void Awake() {
      // set singleton reference on awake
      _singleton = this;
    }

        private void Start() => GameGraphics.Initialize(MapData);

        private GameToken _highlightToken;
        private Route _currentRoute = null;
        private List<NodeId> _targetNodes = new List<NodeId>();
        private void Update()
        {
            // ---------------------------
            // Test of Graphics component
            // Not production code
            //
            _highlightToken?.ResetColor();
            var highlightToken = GameGraphics.GetMapToken(GameInput.MouseNodeId);

            if (highlightToken != null)
            {
                highlightToken.SetColor(Color.yellow);
                _highlightToken = highlightToken;
            }

            if (GameInput.SelectJustPressed && GameInput.MouseNodeId.InBounds && !_targetNodes.Contains(GameInput.MouseNodeId))
            {
                _targetNodes.Add(GameInput.MouseNodeId);
                if (_targetNodes.Count > 1)
                {
                    GameGraphics.DestroyPotentialTrack(_currentRoute);
                    _currentRoute = Pathfinding.CheapestBuild(Tracks, MapData, _targetNodes.ToArray());
                    GameGraphics.GeneratePotentialTrack(_currentRoute);
                }
            }

            if (GameInput.DeleteJustPressed)
            {
                GameGraphics.DestroyPotentialTrack(_currentRoute);
                _targetNodes.Clear();
            }
            if (GameInput.EnterJustPressed)
            {
                GameGraphics.CommitPotentialTrack(_currentRoute, Color.red);

                for (int i = 0; i < _currentRoute.Distance; ++i)
                    Tracks[_currentRoute.Nodes[i], _currentRoute.Nodes[i + 1]] = 0;

                _targetNodes.Clear();
            }
        }

#if UNITY_EDITOR

            private void OnDrawGizmos() {
      List<Action> postDraws = new List<Action>();
      if (MapData == null || MapData.Nodes == null || MapData.Nodes.Length == 0)
        return;

      var labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
      labelStyle.alignment = TextAnchor.UpperCenter;
      labelStyle.fontSize = 16;
      labelStyle.fontStyle = FontStyle.Bold;

      for (int x = 0; x < Size; x++) {
        for (int y = 0; y < Size; y++) {
          // draw node
          var node = MapData.Nodes[(y * Size) + x];
          var pos = Utilities.GetPosition(node.Id);
          Gizmos.color = Utilities.GetNodeColor(node.Type);
          Gizmos.DrawCube(pos, Vector3.one * WSSize * 0.3f);

          //
          if (node.CityId >= 0 && node.CityId < MapData.Cities.Count) {
            var city = MapData.Cities[node.CityId];
            if (node.Type == NodeType.MajorCity || node.Type == NodeType.MediumCity || node.Type == NodeType.SmallCity) {

              postDraws.Add(() =>
              {
                Handles.Label(pos + Vector3.up, city.Name, labelStyle);
              });

            }
          }

          // draw segments
          // we iterate only bottom-right half of segments to prevent drawing them twice
          var segments = MapData.GetNodeSegments(node.Id);
          for (Cardinal c = Cardinal.NE; c <= Cardinal.S; ++c) {
            // get segment
            var segment = segments[(int)c];
            if (segment != null) {
              // get neighboring nodeid
              var nextNodeId = Utilities.PointTowards(node.Id, c);
              if (nextNodeId.InBounds) {
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

    #region Game Loop

    #region Public Data
    
    // The cost to build a track to a respective NodeType
    public static readonly ReadOnlyDictionary<NodeType, int> NodeCosts = new ReadOnlyDictionary<NodeType, int>(
        new Dictionary<NodeType, int>
        {
            { NodeType.Clear,      1 },
            { NodeType.Mountain,   2 },
            { NodeType.SmallCity,  3 },
            { NodeType.MediumCity, 3 },
            { NodeType.MajorCity,  5 },
            { NodeType.Water, 1000   },
        }
    );


    // The cost to build over a river
    public const int RiverCost = 2;

    /// <summary>
    /// The trains that players can use.
    /// </summary>
    public TrainData[] trainData;
    /// <summary>
    /// UI window that shows stats of the current player.
    /// </summary>
    public GameObject PlayerInfoPanel;
    /// <summary>
    /// UI windows that show the controls for each phase.
    /// </summary>
    public GameObject[] PhasePanels;
    #endregion

    #region Private Data
    PlayerInfo[] players;
    PlayerInfo player;
    int maxPhases;
    int currentPlayer = 0;
    int currentPhase = -2;
    #endregion

    /// <summary>
    /// Sets up the current game.
    /// </summary>
    private void GameLoopSetup() {
      maxPhases = PhasePanels.Length;

      // Initiate all player info.
      players = new PlayerInfo[_rules.maxPlayers];
      for (int p = 0; p < _rules.maxPlayers; p++)
        players[p] = new PlayerInfo("Player " + p, Color.white, _rules.moneyStart, 0);

      // Deactivate all panels just in case.
      for (int u = 0; u < maxPhases; u++)
        PhasePanels[u].SetActive(false);

      // Activate first turn panel.
      PhasePanels[1].SetActive(true);
      player = players[currentPlayer];
      UpdatePlayerInfo();
    }

    // Moves the train to final node in path.
    public void MoveTrain() {
      // TODO: Move train to last pushed node.
      // Moving only updates the phase.
      GameLogic.UpdatePhase(PhasePanels, ref currentPhase, maxPhases);
      return;
    }
    // Discards the player's hand.
    public void DiscardHand() {
      // TODO: removing and refilling player's hand
      // Ends the turn.
      GameLogic.IncrementPlayer(ref currentPlayer, _rules.maxPlayers);
      return;
    }

    // Builds the track between the nodes in path.
    public void BuildTrack() {
      GameLogic.BuildTrack();
      EndTurn();
      return;
    }
    // Upgrades the player's train.
    public void UpgradeTrain(int choice) {
      GameLogic.UpgradeTrain(ref player.trainStyle, ref player.money, choice, _rules.trainUpgrade);
      EndTurn();
      return;
    }

    // Ends the turn and changes phase.
    private void EndTurn() {
      if (maxPhases >= 0) {
        GameLogic.IncrementPlayer(ref currentPlayer, _rules.maxPlayers);
        GameLogic.UpdatePhase(PhasePanels, ref currentPhase, maxPhases);
      }
      else {
        GameLogic.BuildTurn(ref currentPlayer, ref currentPhase, _rules.maxPlayers);
      }
      return;
    }

    // Places the current player's train at position.
    public void PlaceTrain(NodeId position) {
      player.train_position = position;
      return;
    }

    // Path Management
    // Adds nodes to a path stack
    // Used for building and movement
    public void PushNode(NodeId node) {
      // Phase 0 is the movement phase.
      if (currentPhase == 0 && !player.movepath.Contains(node)) {
        player.movepath.Push(node);
        return;
      }
      // Other phases involve building.
      // Check every path for existing node.
      foreach (Stack<NodeId> stack in player.buildpaths) {
        if (stack.Contains(node))
          return;
      }
      if (player.currentPath == -1) {
        player.buildpaths.Add(new Stack<NodeId>());
        player.currentPath++;
      }
      player.buildpaths[player.currentPath].Push(node);
      return;
    }
    public NodeId PopNode() {
      if (currentPhase == 0) {
        return player.movepath.Pop();
      }
      return player.buildpaths[player.currentPath].Pop();
    }
    public void ClearPath(int path) {
      // Clear the path specified.
      if (currentPhase == 0) {
        player.movepath.Clear();
      }
      player.buildpaths.RemoveAt(path);
      if (player.buildpaths.Count == 0)
        player.currentPath = -1;
      return;
    }
    public void SwitchPath(int path) {
      // Switch to a different building path.
      player.currentPath = path;
      return;
    }
    #endregion


  }
}