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
        public int trainType;
        public int majorCities;
        public NodeId trainPosition;
        public bool trainPlaced;

        public int movePathStyle;
        public int buildPathStyle;
        public int movePointsLeft;

        public bool hasMajorCities;

        public List<NodeId> movePath;
        public List<DemandCard> demandCards;
        public List<Good> goodsCarried;

        public PlayerInfo(string name, Color color, int money, int trainType)
        {
            this.name = name;
            this.color = color;
            this.money = money;
            this.trainType = trainType;

            majorCities = 0;
            trainPosition = new NodeId(0, 0);
            trainPlaced = false;

            movePathStyle = 0;
            buildPathStyle = 0;
            movePointsLeft = 0;

            hasMajorCities = false;

            demandCards = new List<DemandCard>();
            goodsCarried = new List<Good>();
            movePath = new List<NodeId>();
        }
    }
}
