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

        public Queue<NodeId> movepath;
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

            movepath = new Queue<NodeId>();
            movePathStyle = 0;
            buildPathStyle = 0;
        }
    }
}
