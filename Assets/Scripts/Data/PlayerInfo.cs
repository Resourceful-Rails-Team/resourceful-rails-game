using Rails.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct PlayerInfo
    {
        public string name;
        public Color color;
        public int money;
        public int trainStyle;
        public int majorcities;
        public NodeId train_position;
        public Stack<NodeId> movepath;
        public List<Stack<NodeId>> buildpaths;
        public int currentPath;
        public int movePathStyle;
        public int buildPathStyle;

        public PlayerInfo(string name, Color color, int money, int train)
        {
            this.name = name;
            this.color = color;
            this.money = money;
            this.trainStyle = train;
            majorcities = 0;
            train_position = new NodeId(0, 0);
            movepath = new Stack<NodeId>();
            buildpaths = new List<Stack<NodeId>>();
            currentPath = 0;
            movePathStyle = 0;
            buildPathStyle = 0;
        }
    }
}
