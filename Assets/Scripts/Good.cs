using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rails
{
    [Serializable]
    public class Good
    {
        [SerializeField]
        public string Name;

        public override bool Equals(object obj) => 
            obj is Good good && good.Name == Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
}
