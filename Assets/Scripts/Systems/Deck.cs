using Rails.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails.Systems
{
    public static class Deck
    {
        // The number of Demand cards to generate
        public const int DemandCardCount = 136;
        // The minumum distance a Demand City can be
        // from the general location of a Demand Good
        private const float MinimumDistance = 20.0f;
 
        private static List<Demand[]> _drawPile;
        private static List<Demand[]> _discardPile;
        private static Manager _manager;
        
        /// <summary>
        /// Initializes the Deck, creating all cards
        /// associated with the `Manager`'s `MapData`
        /// </summary>
        public static void Initialize()
        {
            _drawPile = new List<Demand[]>();
            _discardPile = new List<Demand[]>();
            _manager = Manager.Singleton;

            var demands = new List<Demand>();

            // Grab all cities, grouping them by type
            var cities = new City[][]
            {
                _manager.MapData.AllCitiesOfType(NodeType.SmallCity),
                _manager.MapData.AllCitiesOfType(NodeType.MediumCity),
                _manager.MapData.AllCitiesOfType(NodeType.MajorCity)
            };
            
            // Create a integer value representing the number of Demands
            // to generate per City type. Medium cities have the most preference
            // while major cities have the least.
            var citiesCount = new int[]
            {
                cities[0].Length / 3,
                cities[1].Length / 2,
                cities[2].Length / 5
            };
            
            // Grab all used Goods
            var goods = _manager.MapData.Goods.Where(g => _manager.MapData.LocationsOfGood(g).Length > 0).ToArray();

            // Create a map determining the general, localized position of a Good
            // (ie. find a city's NodeId with that good).
            var goodsPositions = new List<NodeId>();
            for (int i = 0; i < goods.Length; ++i)
            {
                var ids = _manager.MapData.LocationsOfGood(goods[i]);
                goodsPositions.Add(ids[0]);
            }
            
            // Generate 3x the amount of Demand cards
            // as each has three different Demands on them
            while(demands.Count < DemandCardCount * 3)
            {
                // Cycle through all cities per City type
                for (int i = 0; i < cities.Length; ++i)
                {
                    // And repeat Demand generation per City preference count
                    for (int j = 0; j < citiesCount[i]; ++j)
                    {
                        // Select a random Good
                        int goodIndex = Random.Range(0, goods.Length);

                        int cityIndex = 0;
                        var distance = 0.0f; // Arbitrary small number,
                                             // to ensure the following while loop executes
                        
                        // Select a random City from the current group.
                        // While the distance between the city and the selected Good
                        // is less than the MinimumDistance, or if the City holds
                        // the Good being sought, reselect a City
                        while (
                            distance < MinimumDistance || 
                            cities[i][cityIndex].Goods.Any(g => g.x == goodIndex)
                        ) {
                            cityIndex = Random.Range(0, cities[i].Length);
                            distance = NodeId.Distance(
                                _manager.MapData.LocationsOfCity(cities[i][cityIndex]).First(),
                                _manager.MapData.LocationsOfGood(goods[goodIndex]).First()
                            );
                        }
                        
                        // Determine the reward by City NodeType, with distance considered
                        int reward = ((int)distance + (i * 10)) / 10 * 10;
                        demands.Add(new Demand(cities[i][cityIndex], goods[goodIndex], reward));

                        if (demands.Count >= DemandCardCount * 3) break;
                    }
                    if (demands.Count >= DemandCardCount * 3) break;
                }
            }
            
            var demandCard = new List<Demand>();

            // Cycle through all demands, choosing one at random, and
            // adding it to demandCard. When 3 cards are inserted,
            // generate a new Demand card, and add it to the draw pile
            while(demands.Count > 0)
            {
                int demandIndex = Random.Range(0, demands.Count);
                var demand = demands[demandIndex];

                demandCard.Add(demand);
                if(demandCard.Count == 3)
                {
                    _drawPile.Add(demandCard.ToArray());
                    demandCard.Clear();
                }
                demands.RemoveAt(demandIndex);
            }

            // Add all draw cards to the discard pile, to
            // ensure the deck is properly shuffled on start
            for(int i = _drawPile.Count - 1; i >= 0; --i)
            {
                _discardPile.Add(_drawPile[i]);
                _drawPile.RemoveAt(i);
            }    
        }
    
        /// <summary>
        /// Draws a single Demand card
        /// </summary>
        /// <returns>An array of 3 demands, 
        /// representing the card contents</returns>
        public static Demand[] DrawOne()
        {
            // If there are no cards in the draw pile,
            // shuffle the discards and readd them to the draw pile.
            if (_drawPile.Count == 0)
                ShuffleDiscards();

            var index = _drawPile.Count - 1;
            var card = _drawPile[index];

            _drawPile.RemoveAt(index);
            return card;
        }
        
        /// <summary>
        /// Discard a single Demand card into the discard pile
        /// </summary>
        /// <param name="demandCard">The card to add to the discard pile</param>
        public static void Discard(Demand[] demandCard) => _discardPile.Add(demandCard);
        
        // Randomly reinserts all discard Demand cards
        // into the draw pile
        private static void ShuffleDiscards()
        {
            while(_discardPile.Count > 0)
            {
                int cardIndex = Random.Range(0, _discardPile.Count);
                _drawPile.Add(_discardPile[cardIndex]);
                _discardPile.RemoveAt(cardIndex); 
            }
        }
    }
}
