using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TrainSpecs 
{
    public string name;
    public int goodsTotal;
    public int movePoints;

    public override string ToString()
    {
        return
            $"{name} Train\n" +
            $"Capacity: {goodsTotal}\n" +
            $"Speed: {movePoints}";
    }
}
