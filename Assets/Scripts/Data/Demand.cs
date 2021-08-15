/* This work is released under the MIT license.
    Please see the file LICENSE in this distribution for
    license terms. */

namespace Rails.Data
{
    public class Demand
    {
        public City City { get; private set; }
        public Good Good { get; private set; }
        public int Reward { get; private set; }

        public Demand(City city, Good good, int reward)
        {
            City = city;
            Good = good;
            Reward = reward;
        }

        public override string ToString()
            => $"{City.Name}, {Good.Name}, {Reward}";
    }
}
