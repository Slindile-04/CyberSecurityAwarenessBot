using System;

namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// ActivityLogEntry.cs - Represents a single activity record in the chatbot activity log.
    /// </summary>
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; init; }
        public string ActionType { get; init; }
        public string Description { get; init; }

        public ActivityLogEntry(DateTime timestamp, string actionType, string description)
        {
            Timestamp = timestamp;
            ActionType = actionType ?? string.Empty;
            Description = description ?? string.Empty;
        }
    }
}
