/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

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
