using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
{
    public class GameStartRules : MonoBehaviour
    {
        public StartPlayerInfo[] Players;
    }



    [Serializable]
    public struct StartPlayerInfo
    {
        public string Name;
        public Color Color;
    }
}
