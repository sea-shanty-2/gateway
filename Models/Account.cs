namespace Gateway.Models
{
    public class Account : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Join(' ', FirstName, LastName);
    }
}