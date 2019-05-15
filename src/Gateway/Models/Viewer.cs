namespace Gateway.Models
{
    public class Viewer : IEntity
    {
        public string Id { get; set; }
        public string BroadcastId { get; set; }
        public string AccountId { get; set; }
        public long Timestamp { get; set; }
    }
}