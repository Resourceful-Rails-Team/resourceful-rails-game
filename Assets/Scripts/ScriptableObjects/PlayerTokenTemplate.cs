using Assets.Scripts.Data;
using Rails.Rendering;
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
        private GameToken _baseTrainToken;
        [SerializeField]
        private GameToken _fastTrainToken;
        [SerializeField]
        private GameToken _heavyTrainToken;
        [SerializeField]
        private GameToken _superTrainToken;

        [SerializeField]
        private GameToken _railToken;
        public GameToken RailToken => _railToken;

        public GameToken TrainTokenOfType(TrainType trainType) => trainType switch
        {
            TrainType.Fast => _fastTrainToken,
            TrainType.Heavy => _heavyTrainToken,
            TrainType.Super => _superTrainToken,
            _ => _baseTrainToken,
        };
    }
}