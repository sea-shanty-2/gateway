using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Principal;

namespace Gateway.Models
{
    public class Broadcast : IEntity
    {
        public Broadcast()
        {
            Reports = new List<string>();
        }

        public string Id { get; set; }
        public double[] Categories { get; set; }
        public int? Bitrate { get; set; }
        public double? Stability { get; set; }
        public Location Location { get; set; }
        public DateTime? Activity { get; set; }
        public Boolean? Expired { get; set; }
        public string AccountId { get; set; }
        public string Token { get; set; }
        public int? Score { get; set; }
        public int? PositiveRatings { get; set; }
        public int? NegativeRatings { get; set; }
        public ICollection<string> Reports { get; set; }
    }

    public class ViewerDateTimePair
    {
        public string Id { get; set; }
        public long Time { get; set; }

        public ViewerDateTimePair(string id, long ticks)
        {
            Id = id;
            Time = ticks;
        }
    }
}