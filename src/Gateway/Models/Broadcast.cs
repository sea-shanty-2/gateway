using System;

namespace Gateway.Models
{
  public class Broadcast : IEntity
  {
    public Broadcast(string token)
    {
      this.Token = token;
    }
    public string Id { get; set; }
    public string Token { get; }
    public string Title { get; set; }
    public string Tag { get; set; }
    public int Bitrate { get; set; }
    public float Stability { get; set; }
    public Location Location { get; set; }
    public DateTime Activity { get; set; }
  }
}