/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

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
