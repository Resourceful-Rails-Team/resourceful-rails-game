using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rails.ScriptableObjects;
using UnityEngine.InputSystem;
using System.Collections.ObjectModel;
using Rails.Rendering;
using Rails.Controls;
using Rails.Data;
using Rails.Systems;
using System.Linq;

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
    /// The Cost for a player to use another player's track
    /// </summary>
    public const int AltTrackCost = 4;

    /// <summary>
    /// Controls the spacing between nodes in terms of Unity units.
    /// </summary>
    public float WSSize = 1f;

    /// <summary>
    /// Stores the layout of the map, including nodes, cities, goods, etc.
    /// </summary>
    [SerializeField]
    public MapData MapData;

    /// <summary>
    /// Stores the tracks on the map.
    /// </summary>
    [SerializeField]
    public Dictionary<NodeId, int[]> Tracks = new Dictionary<NodeId, int[]>();
    #endregion // Properties

    private Rails.Rendering.Graphics _graphics;
    private GameRules _rules;

    #region Utilities
    /// <summary>
    /// Inserts a new track onto the Map, based on position and direction.
    /// </summary>
    /// <param name="player">The player who owns the track</param>
    /// <param name="position">The position the track is placed</param>
    /// <param name="towards">The cardinal direction the track moves towards</param>
    private void InsertTrack(int player, NodeId position, Cardinal towards) {
      // If Cardinal data doesn't exist for the point yet,
      // insert and initialize the data
      if (!Tracks.ContainsKey(position)) {
        Tracks[position] = new int[(int)Cardinal.MAX_CARDINAL];
        for (int i = 0; i < (int)Cardinal.MAX_CARDINAL; ++i)
          Tracks[position][i] = -1;
      }

      Tracks[position][(int)towards] = player;

      // As Tracks is undirected, insert a track moving the opposite way from the
      // target node as well.
      InsertTrack(player, Utilities.PointTowards(position, towards), Utilities.ReflectCardinal(towards));
    }

    #endregion // Utilities
    #endregion // Map

    #region Unity Events

    private void Awake() {
      // set singleton reference on awake
      _singleton = this;
      _graphics = GetComponent<Rails.Rendering.Graphics>();
      _rules = MapData.DefaultRules;
      Deck.Initialize();
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

    private GameToken _highlightToken;
    private Route _currentRoute = null;
    private List<NodeId> _targetNodes = new List<NodeId>();
     
    private void Update() {
           
      // ---------------------------
      // Test of Graphics component
      // Not production code
      //
      _highlightToken?.ResetColor();
      var highlightToken = _graphics.GetMapToken(GameInput.MouseNodeId);

      if (highlightToken != null)
      {
          highlightToken.SetColor(Color.yellow);
          _highlightToken = highlightToken;
      }

      if(GameInput.SelectJustPressed && GameInput.MouseNodeId.InBounds && !_targetNodes.Contains(GameInput.MouseNodeId))
      {
          _targetNodes.Add(GameInput.MouseNodeId); 
          if(_targetNodes.Count > 1)
          {
              _graphics.DestroyPotentialTrack(_currentRoute);
              _currentRoute = Pathfinding.CheapestBuild(Tracks, MapData, _targetNodes.ToArray());
              _graphics.GeneratePotentialTrack(_currentRoute);
          }
      }

      if (GameInput.DeleteJustPressed)
      {
          _graphics.DestroyPotentialTrack(_currentRoute);
          _targetNodes.Clear();
      }
      if(GameInput.EnterJustPressed)
      {
          _graphics.CommitPotentialTrack(_currentRoute, Color.red);
          _targetNodes.Clear();
      }

      // ------------------------------------

      InputUpdate();
    }

    #endregion

    #region Input

    void InputUpdate() {

    }

    #endregion

    #region Game Loop
    public struct PlayerInfo {
      public string name;
      public Color color;
      public int money;
      public int trainStyle;
      public int majorcities;
      public NodeId train_position;
      public Stack<NodeId> movepath;
      public List<Stack<NodeId>> buildpaths;
      public int currentPath;
      public int movePathStyle;
      public int buildPathStyle;

      public PlayerInfo(string name, Color color, int money, int train) {
        this.name = name;
        this.color = color;
        this.money = money;
        this.trainStyle = train;
        majorcities = 0;
        train_position = new NodeId(0, 0);
        movepath = new Stack<NodeId>();
        buildpaths = new List<Stack<NodeId>>();
        currentPath = 0;
        movePathStyle = 0;
        buildPathStyle = 0;
      }
    }

    #region Public Data
    
    // The cost to build a track to a respective NodeType
    public static readonly ReadOnlyDictionary<NodeType, int> NodeCosts = new ReadOnlyDictionary<NodeType, int>(
        new Dictionary<NodeType, int>
        {
            { NodeType.Clear,      1 },
            { NodeType.Mountain,   4 },
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
    int phases;
    int currentPlayer = 0;
    int currentPhase = -2;
    #endregion

    /// <summary>
    /// Sets up the current game.
    /// </summary>
    private void GameLoopSetup() {
      phases = PhasePanels.Length;

      // Initiate all player info.
      players = new PlayerInfo[_rules.maxPlayers];
      for (int p = 0; p < _rules.maxPlayers; p++)
        players[p] = new PlayerInfo("Player " + p, Color.white, _rules.moneyStart, 0);

      // Deactivate all panels just in case.
      for (int u = 0; u < phases; u++)
        PhasePanels[u].SetActive(false);

      // Activate first turn panel.
      PhasePanels[1].SetActive(true);
      player = players[currentPlayer];
      UpdatePlayerInfo();
    }

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
      BuildTrack_();
      // Ends the turn and changes phase.
      if (phases < 0)
        BuildTurn();
      else
        NormalTurn();
      return;
    }
    // Upgrades the player's train.
    public void UpgradeTrain(int choice) {
      UpgradeTrain_(choice);
      // Ends the turn and changes phase.
      if (phases < 0)
        BuildTurn();
      else
        NormalTurn();
      return;
    }
    // Places the current player's train at position.
    public void PlaceTrain(NodeId position) {
      player.train_position = position;
      return;
    }
    #endregion // Player Actions

    #region Path Methods
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
    #endregion // Path

    #region Private Methods
    // Updates current player through normal turns.
    private void NormalTurn() {
      IncrementPlayer();
      UpdatePhase();
      return;
    }
    // Updates current player through the intial build turns.
    private void BuildTurn() {
      // Phase -2, build turns, normal player order.
      // Phase -1, build turns, reverse player order.
      // Phase 0, normal turns, place trains.
      switch (currentPhase) {
        case -2: IncrementPlayer(); break;
        case -1: DecrementPlayer(); break;
      }
      if (currentPlayer == 5 || currentPlayer == 0) {
        UpdatePhase();
      }
      if (currentPhase == 0) {
        // TODO: Change buttons to normal build/upgrade methods.

      }
      return;
    }
    // Private method for building.
    private void BuildTrack_() {
      // TODO: Build track between all nodes in stack.
      List<Route> routes = new List<Route>();

      foreach (Stack<NodeId> stack in player.buildpaths) {
        NodeId start;
        while (stack.Count != 0) {
          start = stack.Pop();

        }
      }

      return;
    }
    // Private method for upgrading.
    private void UpgradeTrain_(int choice) {
      // If player doesn't have enough money, don't upgrade
      if (player.money < _rules.trainUpgrade) {
        // TODO: Activate failure UI message here.
        return;
      }

      // Deduct value from player's money stock and change train value.
      player.money -= _rules.trainUpgrade;
      player.trainStyle = choice;
      Debug.Log(currentPlayer + " $" + player.money);
      return;
    }
    // Changes the current player
    private int IncrementPlayer() {
      currentPlayer += 1;
      if (currentPlayer >= _rules.maxPlayers)
        currentPlayer = 0;
      UpdatePlayerInfo();
      return currentPlayer;
    }
    // Changes players for switchback start.
    private int DecrementPlayer() {
      currentPlayer -= 1;
      if (currentPlayer < 0)
        currentPlayer = 0;
      UpdatePlayerInfo();
      return currentPlayer;
    }
    // Updates name and money amount. Placeholder.
    private void UpdatePlayerInfo() {
      //Transform playertext = PlayerInfoPanel.transform.Find("Player");
      //playertext.GetComponent<TMP_Text>().text = "Player #" + (currentPlayer + 1);
      //playertext = PlayerInfoPanel.transform.Find("Money");
      //playertext.GetComponent<TMP_Text>().text = "$" + players[currentPlayer].money;
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
      if (player.majorcities >= _rules.winMajorCities &&
        player.money >= _rules.winMoney) {
        return true;
      }
      return false;
    }
    #endregion // Private

    #endregion // Game Loop
  }
}