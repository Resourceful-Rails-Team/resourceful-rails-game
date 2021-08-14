/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Data;
using Rails.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TokenTemplate", menuName = "ScriptableObjects/Map/MapTokenTemplate", order = 2)]
    public class MapTokenTemplate : ScriptableObject
    {
        [SerializeField] private GameToken _clear;
        [SerializeField] private GameToken _mountain;
        [SerializeField] private GameToken _smallCity;
        [SerializeField] private GameToken _mediumCity;
        [SerializeField] private GameToken _majorCity;

        public GameToken GetToken(NodeType nodeType) => nodeType switch
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