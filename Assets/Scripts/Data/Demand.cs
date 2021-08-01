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
            => $"Demand: {City.Name}, {Good.Name}, {Reward}";
    }
}
