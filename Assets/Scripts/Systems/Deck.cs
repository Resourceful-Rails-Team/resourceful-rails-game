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

            var smCities = _manager.MapData.AllCitiesOfType(NodeType.SmallCity);
            var mdCities = _manager.MapData.AllCitiesOfType(NodeType.MediumCity);
            var mjCities = _manager.MapData.AllCitiesOfType(NodeType.MajorCity);

            var goods = _manager.MapData.Goods; 
            // Create a map determining the general position of a Good (ie
            // find a city's NodeId with that good). Then, one can compare the
            // distances to determine if the requesting city is far enough).
             
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
