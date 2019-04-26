using System;

namespace Gateway.Models
{
    public class Broadcast : IEntity
    {
        public Broadcast()
        {
            Activity = DateTime.UtcNow;
        }

        public string Id { get; set; }
        public double[] Categories { get; set; }
        public int Bitrate { get; set; }
        public double Stability { get; set; }
        public Location Location { get; set; }
        public DateTime Activity { get; set; }
        public string AccountId { get; set; }
    }
}