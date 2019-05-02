namespace Gateway.Models
{
    public class Account : IEntity
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string FacebookId { get; set; }
        public double[] Categories { get; set; }
        public long Score { get; set; }
    }
}