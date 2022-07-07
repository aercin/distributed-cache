namespace DistributedCacheSample.Models
{
    public class Repository
    {
        public Repository()
        {
            Players = new List<Player>();
        }
        public List<Player> Players { get; set; }
    }
}
