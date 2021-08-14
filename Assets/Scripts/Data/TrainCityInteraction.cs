/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

using Rails.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public class TrainCityInteraction
    {
        public DemandCard[] Cards { get; set; }
        public Good[] Goods { get; set; }

        public int PlayerIndex { get; set; }
        public City City { get; set; }
        public NodeId TrainPosition { get; set; }
    }
}
