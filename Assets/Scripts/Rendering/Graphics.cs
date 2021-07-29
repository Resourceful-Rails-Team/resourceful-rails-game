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
        private Dictionary<NodeId, IGameToken> _mapTokens;
        private ReadOnlyDictionary<NodeId, int[]> _mapTracks;

        private void Start()
        {
            _manager = Manager.Singleton;
            _mapTokens = new Dictionary<NodeId, IGameToken>();
            _mapTracks = new ReadOnlyDictionary<NodeId, int[]>(_manager.Tracks);

            GenerateBoard();
            GenerateNodes();
        }

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
                    Node node = _manager.MapData.Nodes[nodeId.GetSingleId()];

                    if (node.Type == NodeType.MajorCity && !IsCenterOfMajorCity(nodeId))
                        continue;

                    Vector3 pos = Utilities.GetPosition(node.Id);
                    GameObject model = _manager.MapData.DefaultTokenTemplate.GetToken(node.Type);

                    if (model)
                    {
                        GameObject nodeObj = Instantiate(model, transform);
                        nodeObj.transform.position = pos;
                    }
                }
            }
        }

        private bool IsCenterOfMajorCity(NodeId nodeId)
        {
            if (_manager.MapData.Nodes[nodeId.GetSingleId()].Type != NodeType.MajorCity)
                return false;

            var node = _manager.MapData.Nodes[nodeId.GetSingleId()];
            var cityId = _manager.MapData.Nodes[nodeId.GetSingleId()].CityId;
            var cardinalRange = Enumerable.Range((int)Cardinal.N, (int)Cardinal.MAX_CARDINAL);

            return 
                 cardinalRange
                .Select(c => Utilities.PointTowards(nodeId, (Cardinal)c))
                .All(nId => nId.InBounds && _manager.MapData.Nodes[nId.GetSingleId()].CityId == node.CityId);
        }

        private void CreateTrains()
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
        }
        #endregion
    }
}
