using Rails.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public class TrainCityInteractionResult
    {
        public DemandCard[] ChosenCards { get; set; }
        public Good[] Goods { get; set; }
    }
}
