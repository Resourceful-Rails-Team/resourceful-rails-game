using Rails.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class PlayerInfo
    {
        public string name;
        public Color color;
        public int money;
        public int trainStyle;
        public int majorCities;
        public NodeId trainPosition;
        public bool trainPlaced;

        public List<NodeId> moveSegments;
        public int movePathStyle;
        public int buildPathStyle;

        public List<Demand[]> demandCards;
        public List<Good> goodsCarried;

        public PlayerInfo(string name, Color color, int money, int train)
        {
            this.name = name;
            this.color = color;
            this.money = money;
            this.trainStyle = train;
            majorCities = 0;
            trainPosition = new NodeId(0, 0);
            trainPlaced = false;

            moveSegments = new List<NodeId>();
            movePathStyle = 0;
            buildPathStyle = 0;

            demandCards = new List<Demand[]>();
            goodsCarried = new List<Good>();
        }
    }
}
