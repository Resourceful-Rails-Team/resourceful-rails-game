/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Assets.Scripts.Data;
using Rails.Rendering;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace Rails.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerTemplate", menuName = "ScriptableObjects/Players/PlayerTokenTemplate", order = 2)]
    public class PlayerTokenTemplate : ScriptableObject
    {
        [SerializeField]
        private string _trainName;
        public string TrainName => _trainName;
 
        [SerializeField]
        private GameToken _railToken;
        public GameToken RailToken => _railToken;

        [SerializeField]
        private List<GameToken> _trains;

        public GameToken GetTrainToken(int index)
            => _trains[index];
    }
}