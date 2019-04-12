using System;

namespace Gateway.Models
{
    public class Broadcast : IEntity
    {
        public Broadcast()
        {
            Activity = DateTime.UtcNow;
            Token = Guid.NewGuid().ToString("N");
        }

        public string Id { get; set; }
        public string Token { get; }
        public double[] Categories { get; set; }
        public int Bitrate { get; set; }
        public float Stability { get; set; }
        public Location Location { get; set; }
        public DateTime Activity { get; set; }
    }
}