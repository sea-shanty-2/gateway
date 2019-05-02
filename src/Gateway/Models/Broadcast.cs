using System;
using System.Collections.Generic;

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
        public double[] Categories { get; set; }
        public int Bitrate { get; set; }
        public double Stability { get; set; }
        public Location Location { get; set; }
        public DateTime Activity { get; set; }
        public string AccountId { get; set; }
        public string Token { get; }
        public List<(string, DateTime)> JoinedTimeStamps { get; set; }
        public List<(string, DateTime)> LeftTimeStamps { get; set; }
    }
}