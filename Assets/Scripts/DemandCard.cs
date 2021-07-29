using Rails.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    public class Demand
    {
        
    }

    public static class Deck
    {
        private static List<Demand[]> _cards;
        private static List<Demand[]> _discardDeck;

        public static void CreateDeck(MapData mapData)
        {

        }

        public static Demand[] DrawOne()
        {
            var demandsIndex = Random.Range(0, _cards.Count - 1);
            var demands = _cards[demandsIndex];

            _discardDeck.Add(_cards[demandsIndex]);
            _cards.RemoveAt(demandsIndex);

            return demands;
        }
    }
}
