using Rails.Data;
using Rails.Systems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails.Rendering
{
    public class Graphics : MonoBehaviour
    {
        private Manager _manager;
        private Dictionary<NodeId, GameToken> _mapTokens;
        private Dictionary<NodeId, GameToken[]> _trackTokens;
        private Dictionary<Route, List<GameToken>> _potentialTracks;
        private GameToken [] _playerTrains;

        private ObjectPool<GameToken> _trackPool;

        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        private void Start()
        {
            _manager = Manager.Singleton;
            _mapTokens = new Dictionary<NodeId, GameToken>();
            _trackTokens = new Dictionary<NodeId, GameToken[]>();
            _potentialTracks = new Dictionary<Route, List<GameToken>>();

            _trackPool = new ObjectPool<GameToken>(_manager.MapData.DefaultPlayerTemplate.RailToken, 20, 20);

            GenerateBoard();
            GenerateNodes();
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
                float rotateY = Utilities.GetCardinalRotation(
                    Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i + 1])
                );

                var trackToken = _trackPool.Retrieve();
                trackToken.transform.position = Utilities.GetPosition(route.Nodes[i]);
                trackToken.transform.rotation = Quaternion.Euler(0.0f, rotateY, 0.0f);

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
            if(_potentialTracks.TryGetValue(route, out var tokens))
            {
                for (int i = 0; i < route.Nodes.Count - 1; ++i)
                {
                    if (!_trackTokens.ContainsKey(route.Nodes[i]))
                        _trackTokens.Add(route.Nodes[i], new GameToken[(int)Cardinal.MAX_CARDINAL]);
                    if (!_trackTokens.ContainsKey(route.Nodes[i+1]))
                        _trackTokens.Add(route.Nodes[i+1], new GameToken[(int)Cardinal.MAX_CARDINAL]);

                    var direction = Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i+1]);
                    var reflectDirection = Utilities.CardinalBetween(route.Nodes[i+1], route.Nodes[i]);

                    _trackTokens[route.Nodes[i]][(int)direction] = tokens[i];
                    _trackTokens[route.Nodes[i + 1]][(int)reflectDirection] = tokens[i];

                    tokens[i].SetPrimaryColor(color);
                }
                _potentialTracks.Remove(route);
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
            for(int i = 0; i < route.Nodes.Count - 1; ++i)
            {
                int cIndex = (int)Utilities.CardinalBetween(route.Nodes[i], route.Nodes[i + 1]);
                if (_trackTokens.TryGetValue(route.Nodes[i], out var tokens))
                {
                    if (highlighted) tokens[cIndex]?.SetColor(Color.yellow);
                    else tokens[cIndex]?.ResetColor();
                }
            }
        }
        #endregion

        #region Private Methods
        // Instantiates the MapData board, and sets it to the correct size
        private void GenerateBoard()
        {
            var board = Instantiate(_manager.MapData.Board);
            board.transform.localScale = Vector3.one * _manager.WSSize;
        }

        // Instantiates all MapData nodes
        private void GenerateNodes()
        {
            for (int x = 0; x < Manager.Size; x++)
            {
                for (int y = 0; y < Manager.Size; y++)
                {
                    var nodeId = new NodeId(x, y);
                    var node = _manager.MapData.Nodes[nodeId.GetSingleId()];
                    var pos = Utilities.GetPosition(node.Id);

                    var modelToken = _manager.MapData.DefaultTokenTemplate.GetToken(node.Type);

                    if (modelToken != null)
                    {
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

        private void GenerateTrains(int playerCount, Color [] playerColors)
        {
            _playerTrains = new GameToken[playerCount];
            for (int p = 0; p < playerCount; p++)
            {
                _playerTrains[p] = Instantiate(_manager.MapData.DefaultPlayerTemplate.BaseTrainToken, transform);
                _playerTrains[p].gameObject.SetActive(false);

                if (p < playerColors.Length - 1)
                    _playerTrains[p].SetColor(playerColors[p]);
            }
        }
        /*private IEnumerator MoveTrain(int player, Route route, float speed)
        {
            if (player < 0 || player >= _playerTrains.Length)
                yield return null;

            var tr = _playerTrains[player].transform;

            while (tr.position != Utilities.Getroute)
            {
                time += Time.deltaTime;
                norm = speed * time / distance;
                pos = Vector3.Slerp(startv, endv, norm);
                PlayerTrains[player].transform.position = pos;

                yield return null;
            }
        }*/
        #endregion
    }
}
