/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails.Data
{
    [Serializable]
    public class Good
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public Sprite Icon;
    }
}
