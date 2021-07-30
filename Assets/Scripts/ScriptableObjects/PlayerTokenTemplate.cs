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
        private GameObject _baseTrainToken;
        public GameObject BaseTrainToken => _baseTrainToken;

        [SerializeField]
        private GameObject _fastTrainToken;
        public GameObject FastTrainToken => _fastTrainToken;

        [SerializeField]
        private GameObject _heavyTrainToken;
        public GameObject HeavyTrainToken => _heavyTrainToken;

        [SerializeField]
        private GameObject _superTrainToken;
        public GameObject SuperTrainToken => _superTrainToken;

        [SerializeField]
        private GameObject _railToken;
        public GameObject RailToken => _railToken;
    }
}