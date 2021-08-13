using Assets.Scripts.Data;
using Rails;
using Rails.Data;
using Rails.Rendering;
using Rails.ScriptableObjects;
using Rails.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Manager _manager;
    private MapData _mapData;
    private GameRules _rules;
    private bool _trainMoving = false;

    // Is the player still choosing whether to load new Goods at a city?
    private bool _awaitingGUI = false;

    private void Start()
    {
        _manager = Manager.Singleton;
        _mapData = _manager.MapData;
        _rules = _manager._rules;

        Manager.Singleton.OnTrainMeetsCityComplete += (__, _) => _awaitingGUI = false;
        GameGraphics.OnTrainMovementFinished += (_, __) => _trainMoving = false;
    }
    
    /// <summary>
    /// Moves a player's train along the given path, stopping and
    /// invoking interactions events when the train meets a City.
    /// </summary>
    /// <param name="playerIndex">The index of the player whose train is moving</param>
    /// <param name="path">The given path the player's train will progress through</param>
    public void MoveTrain(int playerIndex, List<NodeId> path) => StartCoroutine(CMoveTrain(playerIndex, path));
    private IEnumerator CMoveTrain(int playerIndex, List<NodeId> route)
    {
        var visitedCityIndices = new HashSet<int>();

        for (int i = 0; i < route.Count - 1; ++i)
        {
            // Apply the path node in the route
            GameGraphics.MoveTrain(playerIndex, route.GetRange(i, 2));
            _trainMoving = true;
            
            // Await the GameGraphics finishing moving the train
            while (_trainMoving) yield return null;

            // Check if the current NodeId is a City with Goods
            var node = _mapData.Nodes[route[i+1].GetSingleId()];
            
            // If the city hasn't been already visited, and if
            // it has any goods, invoke the interaction method
            if(
                node.Type >= NodeType.SmallCity &&
                node.Type <= NodeType.MajorCity &&
                !visitedCityIndices.Contains(node.CityId) &&
                _mapData.Cities[node.CityId].Goods.Count > 0
            ) {
                _awaitingGUI = true;
                visitedCityIndices.Add(node.CityId);
                var currentPlayer = _manager.Players[_manager.CurrentPlayer];

                // Invoke the Manager OnMeetsCity method with the provided
                // interaction arguments
                OnMeetsCity?.Invoke(this, new TrainCityInteraction
                {
                    // Select any demand cards that both match the city, and
                    // which the player has the good in their load
                    Cards = currentPlayer.demandCards
                        .Where(dc => dc.Any(d => 
                                d.City == _mapData.Cities[node.CityId] && 
                                currentPlayer.goodsCarried.Contains(d.Good)
                        ))
                        .ToArray(),

                    // Select any good that is from the city, and that
                    // the player can currently pick up
                    Goods = 
                        currentPlayer.goodsCarried.Count < _rules.TrainSpecs[currentPlayer.trainType].goodsTotal 
                        ?
                        _mapData.GetGoodsAtCity(_mapData.Cities[node.CityId])
                            .Select(gi => _mapData.Goods[gi])
                            .Where(g => GoodsBank.GetGoodQuantity(g) > 0)
                            .ToArray()
                        :
                        new Good[0],

                    PlayerIndex = playerIndex,
                    TrainPosition = route[i],
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
