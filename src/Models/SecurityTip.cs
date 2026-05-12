namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// SecurityTip.cs - Represents a security tip or educational content
    /// </summary>
    public class SecurityTip
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RiskLevel { get; set; }  // Low, Medium, High, Critical

        public SecurityTip(string id, string category, string title, string description, string riskLevel)
        {
            Id = id;
            Category = category;
            Title = title;
            Description = description;
            RiskLevel = riskLevel;
        }
    }

    /// <summary>
    /// Supported security categories
    /// </summary>
    public static class SecurityCategories
    {
        public const string Passwords = "Passwords";
        public const string Phishing = "Phishing";
        public const string Malware = "Malware";
        public const string Authentication = "Two-Factor Authentication";
        public const string Privacy = "Data Privacy";
        public const string Networks = "Network Security";
        public const string SocialEngineering = "Social Engineering";
    }
}
