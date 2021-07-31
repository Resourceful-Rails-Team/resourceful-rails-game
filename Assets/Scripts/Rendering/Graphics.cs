using Assets.Scripts.Data;
using Rails.Collections;
using Rails.Data;
using Rails.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails.Rendering
{
    public class Graphics : MonoBehaviour
    {
        private Manager _manager;
        // A collection of all node GameTokens on the map
        private Dictionary<NodeId, GameToken> _mapTokens;
        // A collection of all track GameTokens on the map
        private TrackGraph<GameToken> _trackTokens;
        // A collection of route GameTokens
        private Dictionary<Route, List<GameToken>> _potentialTracks;

        private GameToken [] _playerTrains;
        
        // An object pool for the track GameTokens
        private ObjectPool<GameToken> _trackPool;

        // A set of all currently running trains.
        // Used to avoid coroutine issues when if run on the same train
        // more than once at a time.
        private HashSet<int> _currentlyRunningTrains;

        private void Start()
        {
            _manager = Manager.Singleton;
            _mapTokens = new Dictionary<NodeId, GameToken>();
            _trackTokens = new TrackGraph<GameToken>();
            _potentialTracks = new Dictionary<Route, List<GameToken>>();

            _trackPool = new ObjectPool<GameToken>(_manager.MapData.DefaultPlayerTemplate.RailToken, 20, 20);
            _currentlyRunningTrains = new HashSet<int>();

            GenerateBoard();
            GenerateNodes();
            GenerateTrains(2, new Color[] { Color.blue, Color.green });
        }

        #region Public Methods
        /// <summary>
        /// Retrieves the GameToken located at the given NodeId
        /// </summary>
        /// <param name="nodeId">The NodeId the token is stored at</param>
        /// <returns>The GameToken, if any are located at the NodeId position</returns>
        public GameToken GetMapToken(NodeId nodeId)
        {
            _mapTokens.TryGetValue(nodeId, out var token);
            return token;
        }
        
        /// <summary>
        /// Generates a highlighted track given a `Route`
        /// </summary>
        /// <param name="route">The `Route` to build the track on</param>
        /// <returns>An index representing the ID of the track</returns>
        public void GeneratePotentialTrack(Route route)
        {
            var trackTokens = new List<GameToken>(route.Nodes.Count - 1);
            for(int i = 0; i < route.Nodes.Count - 1; ++i)
            {
                // Determine the rotation between adjacent Route nodes
                var rotation = Utilities.GetCardinalRotation(
                    Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i + 1])
                );
                
                // Setup the track GameToken
                var trackToken = _trackPool.Retrieve();
                trackToken.transform.position = Utilities.GetPosition(route.Nodes[i]);
                trackToken.transform.rotation = rotation;
                
                // Highlight the token and add it to the token list
                trackToken.SetColor(Color.yellow);
                trackTokens.Add(trackToken);
            }

            _potentialTracks[route] = trackTokens;
        }
        
        /// <summary>
        /// Destroys a potential-built track given the supplied `Route`
        /// </summary>
        /// <param name="route">The `Route` of the track being destroyed</param>
        public void DestroyPotentialTrack(Route route)
        {
            if (route == null) return;

            // If the route exists in the current potential tracks,
            // return all tracks to the ObjectPool
            if(_potentialTracks.TryGetValue(route, out var tokens))
            {
                foreach (var token in tokens)
                    _trackPool.Return(token);

                _potentialTracks.Remove(route);
            }
        } 

        /// <summary>
        /// Commits the given `Route` to the map, overlaying the track with a given color.
        /// </summary>
        /// <param name="route">The potential `Route` held in the map.</param>
        /// <param name="color">The color to set the new track to.</param>
        public void CommitPotentialTrack(Route route, Color color)
        {
            if (route == null) return;

            // If the route exists in the current potential tracks,
            // Add its GameTokens to the track token map.
            if(_potentialTracks.TryGetValue(route, out var tokens))
            {
                for (int i = 0; i < route.Nodes.Count - 1; ++i)
                {
                    _trackTokens[route.Nodes[i], route.Nodes[i+1]] = tokens[i]; 
                    tokens[i].SetPrimaryColor(color);
                }
                _potentialTracks.Remove(route);

                StartCoroutine(MoveTrain(1, route, 5.0f)); // For testing purposes only
            }
        }
        
        /// <summary>
        /// Highlights or dehighlights a given track `Route`
        /// Skips any route index that is not on the track map
        /// </summary>
        /// <param name="route">The `Route` to alter</param>
        /// <param name="highlighted">Whether it should be highlighted, or color reset</param>
        public void SetHighlightRoute(Route route, bool highlighted)
        {
            // Check each Route node to see if it exists in the track token map.
            // If it does, (de)activate its highlight.
            for(int i = 0; i < route.Nodes.Count - 1; ++i)
            {
                int direction = (int)Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i + 1]);
                if (_trackTokens.TryGetValue(route.Nodes[i], out var tokens))
                {
                    if (highlighted) tokens[direction]?.SetColor(Color.yellow);
                    else tokens[direction]?.ResetColor();
                }
            }
        }
        
        /// <summary>
        /// Sets a given player's train's `GameToken` based on the `TrainType` provided.
        /// </summary>
        public void UpdatePlayerTrain(int player, TrainType type)
        {
            if (player < 0 || player >= _playerTrains.Length)
                throw new ArgumentException("Attempted to upgrade train for player that doesn't exist.");

            var oldToken = _playerTrains[player];

            var newToken = Instantiate(_manager.MapData.DefaultPlayerTemplate.TrainTokenOfType(type));
            newToken.transform.position = oldToken.transform.position;
            newToken.transform.rotation = oldToken.transform.rotation;
            _playerTrains[player] = newToken;

            Destroy(oldToken.gameObject);
        }

        /// <summary>
        /// Moves a player train along the given `Route`.
        /// </summary>
        /// <param name="player">The player index to move.</param>
        /// <param name="route">The nodes the player train will traverse on.</param>
        public void MoveTrain(int player, Route route) => StartCoroutine(MoveTrain(player, route, 5.0f));
        #endregion

        #region Private Methods
        /// Instantiates the MapData board, and sets it to the correct size
        private void GenerateBoard()
        {
            var board = Instantiate(_manager.MapData.Board);

            // Scale the board to match the current spacing size
            board.transform.localScale = Vector3.one * _manager.WSSize;
        }
        
        /// Instantiates all MapData nodes
        private void GenerateNodes()
        {
            // Cycle through all NodeIds
            for (int x = 0; x < Manager.Size; x++)
            {
                for (int y = 0; y < Manager.Size; y++)
                {
                    var nodeId = new NodeId(x, y);
                    var node = _manager.MapData.Nodes[nodeId.GetSingleId()];
                    var pos = Utilities.GetPosition(node.Id);
                    
                    // Retrieve the GameToken from the Map's default
                    var modelToken = _manager.MapData.DefaultTokenTemplate.GetToken(node.Type);
       
                    if (modelToken != null)
                    {
                        // If the node is a MajorCity, determine if its the center
                        // node (ie. surrounded by nodes with the same CityId).
                        // If so, spawn the MajorCity token there.
                        if (node.Type == NodeType.MajorCity)
                        {
                            var neighborNodes = _manager.MapData.GetNeighborNodes(nodeId);
                            if (neighborNodes.All(nn => nn.Item2.CityId == node.CityId))
                            {
                                var token = Instantiate(modelToken, transform);
                                token.transform.position = pos;

                                foreach (var nId in neighborNodes.Select(nn => nn.Item1))
                                    _mapTokens[nId] = token;

                                _mapTokens[nodeId] = token;
                            }
                        }
                        else
                        {
                            var token = Instantiate(modelToken, transform);
                            token.transform.position = pos;

                            _mapTokens[nodeId] = token;
                        }
                    }
                }
            }
        }
        
        // Generates all player trains, and assigns them their colors
        private void GenerateTrains(int playerCount, Color [] playerColors)
        {
            _playerTrains = new GameToken[playerCount];
            for (int p = 0; p < playerCount; p++)
            {
                _playerTrains[p] = Instantiate(_manager.MapData.DefaultPlayerTemplate.BaseTrainToken, transform);
                _playerTrains[p].gameObject.SetActive(false);

                if (p < playerColors.Length)
                    _playerTrains[p].SetColor(playerColors[p]);
            }
        }
        
        // Moves a player train through the given `Route`
        private IEnumerator MoveTrain(int player, Route route, float speed)
        {
            if (player < 0 || player >= _playerTrains.Length)
                yield break;
            if (route == null || route.Distance == 0)
                yield break;

            if(_currentlyRunningTrains.Contains(player))
            {
                _currentlyRunningTrains.Remove(player);
                yield return null;
            }
            _currentlyRunningTrains.Add(player);

            var tr = _playerTrains[player].transform;
            tr.gameObject.SetActive(true);

            Vector3 nextPoint;
            var nextRotation = Utilities.GetCardinalRotation(Utilities.CardinalBetween(route.Nodes[0], route.Nodes[1]));

            tr.position = Utilities.GetPosition(route.Nodes[0]);
            tr.rotation = nextRotation;

            for(int i = 0; i < route.Distance; ++i)
            {
                nextPoint = Utilities.GetPosition(route.Nodes[i+1]);
                nextRotation = Utilities.GetCardinalRotation(Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i+1])); 

                while (tr.position != nextPoint)
                {
                    if (!_currentlyRunningTrains.Contains(player))
                    {
                        tr.position = Utilities.GetPosition(route.Nodes.Last());
                        tr.rotation = Utilities.GetCardinalRotation(
                            Utilities.CardinalBetween(route.Nodes[route.Nodes.Count - 2], route.Nodes.Last())
                        );
                        yield break;
                    }

                    tr.position = Vector3.MoveTowards(tr.position, nextPoint, speed * Time.deltaTime);
                    tr.rotation = Quaternion.Slerp(tr.rotation, nextRotation, speed * 2.5f * Time.deltaTime);
                    yield return null;
                }
            }
        }
        #endregion
    }
}
