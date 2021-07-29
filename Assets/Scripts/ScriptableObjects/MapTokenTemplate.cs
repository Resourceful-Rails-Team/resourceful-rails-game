using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TokenTemplate", menuName = "ScriptableObjects/Map/MapTokenTemplate", order = 2)]
    public class MapTokenTemplate : ScriptableObject
    {
        [SerializeField] private GameObject _clear;
        [SerializeField] private GameObject _mountain;
        [SerializeField] private GameObject _smallCity;
        [SerializeField] private GameObject _mediumCity;
        [SerializeField] private GameObject _majorCity;

        public GameObject GetToken(NodeType nodeType) => nodeType switch
        {
            NodeType.Clear => _clear,
            NodeType.Mountain => _mountain,
            NodeType.SmallCity => _smallCity,
            NodeType.MediumCity => _mediumCity,
            NodeType.MajorCity => _majorCity,
            _ => null,
        };
    }
}