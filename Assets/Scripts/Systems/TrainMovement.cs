using Assets.Scripts.Data;
using Rails;
using Rails.Data;
using Rails.Rendering;
using Rails.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    public EventHandler<TrainCityInteraction> OnMeetsCity;
    public EventHandler OnMovementFinished;

    private MapData _mapData;
    private bool _trainMoving = false;
    private bool _awaitingGUI = false;

    private void Start()
    {
        _mapData = Manager.Singleton.MapData;
        Manager.Singleton.OnTrainMeetsCityComplete += (__, _) => _awaitingGUI = false;
        GameGraphics.OnTrainMovementFinished += (_, __) => _trainMoving = false;
    }

    public void MoveTrain(int player, List<NodeId> path) => StartCoroutine(CMoveTrain(player, path));
    private IEnumerator CMoveTrain(int player, List<NodeId> path)
    {
        var visitedCityIndices = new HashSet<int>();

        for (int i = 0; i < path.Count - 1; ++i)
        {
            // Apply the Route
            GameGraphics.MoveTrain(player, path.GetRange(i, 2));
            _trainMoving = true;
            
            // Await the GameGraphics finishing moving the train
            while (_trainMoving) yield return null;

            // Check if the current NodeId is a City with Goods
            var node = _mapData.Nodes[path[i+1].GetSingleId()];

            if(
                (node.Type == NodeType.MajorCity  ||
                node.Type == NodeType.MediumCity ||
                node.Type == NodeType.SmallCity)
                && !visitedCityIndices.Contains(node.CityId)
                && _mapData.Cities[node.CityId].Goods.Count > 0
            ) {
                _awaitingGUI = true;
                visitedCityIndices.Add(node.CityId);

                OnMeetsCity?.Invoke(this, new TrainCityInteraction
                {
                    PlayerIndex = player,
                    TrainPosition = path[i],
                    City = _mapData.Cities[node.CityId]
                });
            }

            while (_awaitingGUI)
                yield return null;

            yield return null;
        }

        OnMovementFinished?.Invoke(this, null);
    }
}
