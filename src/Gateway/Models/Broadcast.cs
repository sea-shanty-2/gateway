using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Gateway.Models
{
    public class Broadcast : IEntity
    {
        public Broadcast()
        {
            Activity = DateTime.UtcNow;
            Token = Guid.NewGuid().ToString("N");
            
            JoinedTimeStamps = new List<ViewerDateTimePair>();
            LeftTimeStamps = new List<ViewerDateTimePair>();
        }

        public string Id { get; set; }
        public double[] Categories { get; set; }
        public int Bitrate { get; set; }
        public double Stability { get; set; }
        public Location Location { get; set; }
        public DateTime Activity { get; set; }
        public Boolean Expired { get; set; }
        public string AccountId { get; set; }
        public string Token { get; }
        public int PositiveRatings { get; set; }
        public int NegativeRatings { get; set; }
        public List<ViewerDateTimePair> JoinedTimeStamps { get; set; }
        public List<ViewerDateTimePair> LeftTimeStamps { get; set; }
    }

    public class ViewerDateTimePair
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }

        public ViewerDateTimePair(string id, DateTime dateTime)
        {
            Id = id;
            Time = dateTime;
        }
    }
}