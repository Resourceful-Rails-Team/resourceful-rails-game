using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Rails.Data
{
    public class DemandCard : IEnumerable<Demand>
    {     
        private const int DemandCount = 3;

        private List<Demand> _demands;

        public DemandCard(IEnumerable<Demand> demands)
        {
            int count = Mathf.Min(DemandCount, demands.Count());
            _demands = demands.Take(count).ToList();
        }

        public Demand Card1 => _demands[0];
        public Demand Card2 => _demands[1];
        public Demand Card3 => _demands[2];

        public Demand this[int index] => _demands[index];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public IEnumerator<Demand> GetEnumerator()
        {
            return _demands.GetEnumerator();
        }
        public override string ToString()
        {
            string output = "";
            foreach (Demand demand in _demands)
            {
                output += demand.ToString() + "\n";
            }
            return output;
        }
    }
}
