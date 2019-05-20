using System;
using System.Collections.Generic;
using System.Linq;
using Gateway.Models;

public static class BroadcastUtility{
    public static int CalculateScore(IEnumerable<Viewer> viewers, DateTime? date){
        var joined = GetJoinedTimeStamps(viewers);
        var left = GetLeftTimeStamps(viewers);
        var score = 0;
        var lastActivity = (new DateTimeOffset(date.GetValueOrDefault())).ToUnixTimeSeconds();
        var a = joined.GroupBy(x => x.Id);

        foreach(ViewerDateTimePair joinPair in joined)
        {
            var leftPair = left.FirstOrDefault(x => x.Id == joinPair.Id);
            var leftTime = leftPair != default ? leftPair.Time : lastActivity;
            var difference = leftTime - joinPair.Time;

            if (difference >= 30) score += (int) difference;
        }
        
        return score;
    }

    public static IEnumerable<ViewerDateTimePair> GetJoinedTimeStamps(IEnumerable<Viewer> viewers){
        return viewers
                .GroupBy(x => x.AccountId)
                .SelectMany(g => g.Select((x, i) => new {
                    Index = i,
                    Viewer = x
                }))
                .Where(x => x.Index % 2 == 0)
                .Select(x => new ViewerDateTimePair(x.Viewer.AccountId, x.Viewer.Timestamp));
    }

    public static IEnumerable<ViewerDateTimePair> GetLeftTimeStamps(IEnumerable<Viewer> viewers){
    return viewers
            .GroupBy(x => x.AccountId)
            .SelectMany(g => g.Select((x, i) => new {
                Index = i,
                Viewer = x
            }))
            .Where(x => x.Index % 2 == 1)
            .Select(x => new ViewerDateTimePair(x.Viewer.AccountId, x.Viewer.Timestamp));
    }
}