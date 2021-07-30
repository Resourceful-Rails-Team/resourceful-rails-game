using Rails.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Rails.Rendering
{
    public class Graphics : MonoBehaviour
    {
        private Manager _manager;
        private Dictionary<NodeId, GameToken> _mapTokens;
        private Dictionary<NodeId, GameToken[]> _trackTokens;
        private Dictionary<int, List<GameToken>> _potentialTracks;
        private int _trackId = 0;

        private ObjectPool<GameToken> _trackPool;

        private void Start()
        {
            _manager = Manager.Singleton;
            _mapTokens = new Dictionary<NodeId, GameToken>();
            _trackTokens = new Dictionary<NodeId, GameToken[]>();
            _potentialTracks = new Dictionary<int, List<GameToken>>();
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
        /// Generates a highlighted track given a Route
        /// </summary>
        /// <param name="route">The Route to build the track on</param>
        /// <returns>An index representing the ID of the track</returns>
        public int GeneratePotentialTrack(Route route)
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

            _potentialTracks[_trackId] = trackTokens;

            return _trackId++;
        }

        /// <summary>
        /// Destroys a potential-built track given the supplied ID
        /// </summary>
        /// <param name="trackId">The ID of the track being destroyed</param>
        public void DestroyPotentialTrack(int trackId)
        {
            if(_potentialTracks.TryGetValue(trackId, out var trackTokens))
            {
                foreach (var trackToken in trackTokens)
                    _trackPool.Return(trackToken);

                _potentialTracks.Remove(trackId);
            }
        }
        
        public void CommitPotentialTrack(int trackId, int player)
        {
            if(_potentialTracks.TryGetValue(trackId, out var trackTokens))
            {
                    
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

        /*private void CreateTrains()
        {
            for (int p = 0; p < MaxPlayers; p++)
            {
                PlayerTrains[p] = Instantiate(trainData[0].model, transform);
                PlayerTrains[p].SetActive(false);
                // TODO: Set the material to the correct color.
            }
        }
        private IEnumerator MoveTrain(int player, NodeId start, NodeId end, float speed)
        {
            float norm = 0f;
            float time = 0f;
            Vector3 startv = Utilities.GetPosition(start);
            Vector3 endv = Utilities.GetPosition(end);

            float distance = Vector3.Distance(startv, endv);
            Vector3 pos;

            while (norm <= 1f)
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
