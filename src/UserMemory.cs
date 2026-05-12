using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot.Core
{
    /// <summary>
    /// UserMemory.cs - Stores and manages user preferences and interests
    /// 
    /// Responsibilities:
    /// - Store user preferences (favorite topics, interests, etc.)
    /// - Retrieve stored preferences for personalization
    /// - Detect and store user interests from natural language
    /// - Persist memory throughout the session
    /// 
    /// Design:
    /// - Dictionary-based storage for flexibility
    /// - Simple key-value pairs for easy extension
    /// - Session-scoped memory (resets on app restart)
    /// </summary>
    public class UserMemory
    {
        private readonly Dictionary<string, string> _preferences;
        private readonly List<string> _discussedTopics;

        /// <summary>
        /// Constructor - Initializes memory storage structures.
        /// </summary>
        public UserMemory()
        {
            _preferences = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _discussedTopics = new List<string>();
        }

        /// <summary>
        /// SetPreference() - Stores a preference key-value pair.
        /// Overwrites existing preference if key already exists.
        /// 
        /// Parameters:
        /// - key: The preference type (e.g., "favoriteTopic", "interests")
        /// - value: The preference value (e.g., "privacy", "phishing,passwords")
        /// </summary>
        public void SetPreference(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            _preferences[key] = value ?? string.Empty;
        }

        /// <summary>
        /// GetPreference() - Retrieves a stored preference value.
        /// 
        /// Parameters:
        /// - key: The preference type to retrieve
        /// 
        /// Returns:
        /// - The preference value if it exists
        /// - null if the preference doesn't exist
        /// </summary>
        public string GetPreference(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            if (_preferences.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// HasPreference() - Checks if a preference exists.
        /// 
        /// Parameters:
        /// - key: The preference type to check
        /// 
        /// Returns:
        /// - true if preference exists, false otherwise
        /// </summary>
        public bool HasPreference(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _preferences.ContainsKey(key);
        }

        /// <summary>
        /// SetFavoriteTopic() - Convenience method to set the user's favorite topic.
        /// 
        /// Parameters:
        /// - topic: The topic name (e.g., "privacy", "phishing", "2fa")
        /// </summary>
        public void SetFavoriteTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                SetPreference("favoriteTopic", topic.ToLower());
            }
        }

        /// <summary>
        /// GetFavoriteTopic() - Convenience method to retrieve the favorite topic.
        /// 
        /// Returns:
        /// - The favorite topic if set, null otherwise
        /// </summary>
        public string GetFavoriteTopic()
        {
            return GetPreference("favoriteTopic");
        }

        /// <summary>
        /// HasFavoriteTopic() - Checks if a favorite topic has been set.
        /// 
        /// Returns:
        /// - true if favorite topic exists, false otherwise
        /// </summary>
        public bool HasFavoriteTopic()
        {
            return HasPreference("favoriteTopic");
        }

        /// <summary>
        /// AddDiscussedTopic() - Records that a topic was discussed.
        /// Prevents duplicate entries.
        /// 
        /// Parameters:
        /// - topic: The topic that was discussed
        /// </summary>
        public void AddDiscussedTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                string lowerTopic = topic.ToLower();
                if (!_discussedTopics.Any(t => t.Equals(lowerTopic, StringComparison.OrdinalIgnoreCase)))
                {
                    _discussedTopics.Add(lowerTopic);
                }
            }
        }

        /// <summary>
        /// GetDiscussedTopics() - Retrieves all topics discussed so far.
        /// 
        /// Returns:
        /// - List of discussed topics, empty if none
        /// </summary>
        public List<string> GetDiscussedTopics()
        {
            return new List<string>(_discussedTopics);
        }

        /// <summary>
        /// HasDiscussedTopic() - Checks if a topic has been discussed.
        /// 
        /// Parameters:
        /// - topic: The topic to check
        /// 
        /// Returns:
        /// - true if topic was discussed, false otherwise
        /// </summary>
        public bool HasDiscussedTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            return _discussedTopics.Any(t => t.Equals(topic.ToLower(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// AddInterest() - Records a user interest.
        /// Interests are stored as a comma-separated list.
        /// Prevents duplicate interests.
        /// 
        /// Parameters:
        /// - interest: The topic of interest (e.g., "phishing")
        /// </summary>
        public void AddInterest(string interest)
        {
            if (string.IsNullOrWhiteSpace(interest))
            {
                return;
            }

            interest = interest.ToLower().Trim();
            
            // Get existing interests
            string existingInterests = GetPreference("interests") ?? string.Empty;
            List<string> interests = new List<string>();

            if (!string.IsNullOrWhiteSpace(existingInterests))
            {
                interests = existingInterests.Split(',').Select(i => i.Trim()).ToList();
            }

            // Add new interest if not already present
            if (!interests.Any(i => i.Equals(interest, StringComparison.OrdinalIgnoreCase)))
            {
                interests.Add(interest);
            }

            // Store updated interests
            SetPreference("interests", string.Join(", ", interests));
        }

        /// <summary>
        /// GetInterests() - Retrieves all recorded interests.
        /// 
        /// Returns:
        /// - List of interests, empty if none
        /// </summary>
        public List<string> GetInterests()
        {
            string interests = GetPreference("interests") ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(interests))
            {
                return new List<string>();
            }

            return interests.Split(',').Select(i => i.Trim()).ToList();
        }

        /// <summary>
        /// HasInterests() - Checks if any interests have been recorded.
        /// 
        /// Returns:
        /// - true if at least one interest exists, false otherwise
        /// </summary>
        public bool HasInterests()
        {
            return GetInterests().Count > 0;
        }

        /// <summary>
        /// GetFirstInterest() - Convenience method to get the primary interest.
        /// 
        /// Returns:
        /// - The first recorded interest, or null if no interests
        /// </summary>
        public string GetFirstInterest()
        {
            var interests = GetInterests();
            return interests.Count > 0 ? interests[0] : null;
        }

        /// <summary>
        /// ClearMemory() - Clears all stored preferences and discussion history.
        /// Used cautiously (mainly for testing or explicit reset).
        /// </summary>
        public void ClearMemory()
        {
            _preferences.Clear();
            _discussedTopics.Clear();
        }

        /// <summary>
        /// GetSummary() - Returns a formatted summary of all stored preferences.
        /// Useful for debugging and displaying user profile.
        /// 
        /// Returns:
        /// - Formatted string with all key-value pairs
        /// </summary>
        public string GetSummary()
        {
            if (_preferences.Count == 0 && _discussedTopics.Count == 0)
            {
                return "No preferences stored.";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("=== User Memory ===");

            foreach (var kvp in _preferences)
            {
                summary.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            if (_discussedTopics.Count > 0)
            {
                summary.AppendLine($"Discussed Topics: {string.Join(", ", _discussedTopics)}");
            }

            return summary.ToString();
        }
    }
}
