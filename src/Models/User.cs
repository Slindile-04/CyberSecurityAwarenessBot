namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// User.cs - Represents a user of the application
    /// </summary>
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public int MessageCount { get; set; }

        public User(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            CreatedAt = DateTime.Now;
            LastActiveAt = DateTime.Now;
            MessageCount = 0;
        }

        public void UpdateActivity()
        {
            LastActiveAt = DateTime.Now;
            MessageCount++;
        }
    }
}
