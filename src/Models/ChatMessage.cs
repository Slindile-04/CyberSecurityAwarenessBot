namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// ChatMessage.cs - Represents a single message in the conversation
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; set; }  // "User" or "Bot"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }

        public ChatMessage(string sender, string content, MessageType type = MessageType.Normal)
        {
            Sender = sender;
            Content = content;
            Timestamp = DateTime.Now;
            Type = type;
        }
    }

    /// <summary>
    /// Categorizes different message types for display purposes
    /// </summary>
    public enum MessageType
    {
        Normal,
        Alert,
        Warning,
        Success,
        Error
    }
}
