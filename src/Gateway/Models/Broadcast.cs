namespace Gateway.Models
{
    public class Broadcast : IEntity
    {
        public string Id { get; set; }
        public string Token { get; }
        public string Title { get; set; }
        public string Tag { get; set; }
        public int Bitrate { get; set; }
        public float Stability { get; set; }
        public Location Location { get; set; }
    }
}