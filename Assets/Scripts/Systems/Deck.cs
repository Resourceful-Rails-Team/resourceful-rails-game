using Rails.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Systems
{
   
    public static class Deck
    {
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

        public static void Initialize()
        {

        }

        public static Demand[] DrawOne()
            => _demandCards[Random.Range(0, _demandCards.Length)];

        public static void Discard(Demand[] demand)
        {
            _discardPile.Add(demand);
        }

        public static void ShuffleDiscards()
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
