using Rails.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rails.Systems
{
    public static class Deck
    {
        private const int DemandCardCount = 136;
        private const float MinimumDistance = 20.0f;

        // Test data structure
        // -----------------------
        private static Demand[][] _demandCards = new Demand[][]
        {
            new Demand[]
            {
                new Demand (new City { Name = "Portland, OR" }, new Good { Name = "Hazelnuts" }, 100),
                new Demand (new City { Name = "Boise, ID" },    new Good { Name = "Wheat" },     75),
                new Demand (new City { Name = "Seattle, WA" },  new Good { Name = "Asparagus" }, 30 ),
            },
            new Demand[]
            {
                new Demand (new City { Name = "Missoula, MT" },       new Good { Name = "Computer Hardware" },   10),
                new Demand (new City { Name = "Casper, WY" },         new Good { Name = "Beef" },                100),
                new Demand (new City { Name = "Salt Lake City, UT" }, new Good { Name = "Integrated Circuits" }, 200),
            },
            new Demand[]
            {
                new Demand (new City { Name = "Denver, CO" },  new Good { Name = "Apples" }, 10),
                new Demand (new City { Name = "Redding, CA" }, new Good { Name = "Wine" },   80),
                new Demand (new City { Name = "Spokane, WA" }, new Good { Name = "Cattle" }, 70),
            }
        };
        // ---------------------

        private static List<Demand[]> _drawPile;
        private static List<Demand[]> _discardPile;
        private static Manager _manager;

        public static void Initialize()
        {
            _drawPile = new List<Demand[]>();
            _discardPile = new List<Demand[]>();
            _manager = Manager.Singleton;

            var demands = new List<Demand>();
            var cities = new City[][]
            {
                _manager.MapData.AllCitiesOfType(NodeType.SmallCity),
                _manager.MapData.AllCitiesOfType(NodeType.MediumCity),
                _manager.MapData.AllCitiesOfType(NodeType.MajorCity)
            };

            var citiesCount = new int[]
            {
                cities[0].Length / 3,
                cities[1].Length / 2,
                cities[2].Length / 5
            };

            var goods = _manager.MapData.Goods.Where(g => _manager.MapData.LocationsOfGood(g).Length > 0).ToArray();

            // Create a map determining the general position of a Good (ie
            // find a city's NodeId with that good). Then, one can compare the
            // distances to determine if the requesting city is far enough).
            var goodsPositions = new List<NodeId>();
            for (int i = 0; i < goods.Length; ++i)
            {
                var ids = _manager.MapData.LocationsOfGood(goods[i]);
                goodsPositions.Add(ids[0]);
            }

            while(demands.Count < DemandCardCount * 3)
            {
                for (int i = 0; i < cities.Length; ++i)
                {
                    for (int j = 0; j < citiesCount[i]; ++j)
                    {
                        int goodIndex = Random.Range(0, goods.Length);

                        int cityIndex = 0;
                        var distance = 0.0f; // Arbitrary small number,
                                             // to ensure the next while loop executes

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

                        int reward = ((int)distance + (i * 20)) / 10 * 10;
                        demands.Add(new Demand(cities[i][cityIndex], goods[goodIndex], reward));
                    }
                }
            }
 
            var demandCards = new List<Demand>();
            while(demands.Count > 0)
            {
                int demandIndex = Random.Range(0, demands.Count);
                var demand = demands[demandIndex];

                demandCards.Add(demand);
                if(demandCards.Count == 3)
                {
                    _drawPile.Add(demandCards.ToArray());
                    demandCards.Clear();
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

        public static Demand[] DrawOne()
        {
            if (_drawPile.Count == 0)
                ShuffleDiscards();

            var index = _drawPile.Count - 1;
            var card = _drawPile[index];

            _drawPile.RemoveAt(index);
            return card;
        }

        public static void Discard(Demand[] demand) => _discardPile.Add(demand);

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
