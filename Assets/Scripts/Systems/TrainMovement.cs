using Assets.Scripts.Data;
using Rails;
using Rails.Data;
using Rails.Rendering;
using Rails.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements the movement of trains, synchronizing
/// with the Manager and GameGraphics to create a step by
/// step progression from node to node, and interaction with
/// cities.
/// </summary>
public class TrainMovement : MonoBehaviour
{
    /// <summary>
    /// Invoked when the train hits a City with Goods
    /// </summary>
    public EventHandler<TrainCityInteraction> OnMeetsCity;
    /// <summary>
    /// Invoked when the TrainMovement has completed a train's movement
    /// </summary>
    public EventHandler OnMovementFinished;

    private MapData _mapData;
    private bool _trainMoving = false;

    // Is the player still choosing whether to load new Goods at a city?
    private bool _awaitingGUI = false;

    private void Start()
    {
        _mapData = Manager.Singleton.MapData;
        Manager.Singleton.OnTrainMeetsCityComplete += (__, _) => _awaitingGUI = false;
        GameGraphics.OnTrainMovementFinished += (_, __) => _trainMoving = false;
    }
    
    /// <summary>
    /// Moves a player's train along the given path, stopping and
    /// invoking interactions events when the train meets a City.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="path"></param>
    public void MoveTrain(int playerIndex, List<NodeId> path) => StartCoroutine(CMoveTrain(playerIndex, path));
    private IEnumerator CMoveTrain(int playerIndex, List<NodeId> path)
    {
        var visitedCityIndices = new HashSet<int>();

        for (int i = 0; i < path.Count - 1; ++i)
        {
            // Apply the Route
            GameGraphics.MoveTrain(playerIndex, path.GetRange(i, 2));
            _trainMoving = true;
            
            // Await the GameGraphics finishing moving the train
            while (_trainMoving) yield return null;

            // Check if the current NodeId is a City with Goods
            var node = _mapData.Nodes[path[i+1].GetSingleId()];
            
            // If the city hasn't been already visited, and if
            // it has any goods, invoke the interaction method
            if(
                (node.Type == NodeType.MajorCity  ||
                node.Type == NodeType.MediumCity ||
                node.Type == NodeType.SmallCity)
                && !visitedCityIndices.Contains(node.CityId)
                && _mapData.Cities[node.CityId].Goods.Count > 0
            ) {
                _awaitingGUI = true;
                visitedCityIndices.Add(node.CityId);
                
                // Invoke the Manager OnMeetsCity method with the provided
                // interaction arguments
                OnMeetsCity?.Invoke(this, new TrainCityInteraction
                {
                    Cards = Manager.Singleton.Players[Manager.Singleton.CurrentPlayer].demandCards.ToArray(),
                    PlayerIndex = playerIndex,
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
