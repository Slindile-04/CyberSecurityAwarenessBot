using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// TipTracker.cs - Manages tip selection and prevents repetition
    /// 
    /// Responsibilities:
    /// - Track which tips have been shown for each topic
    /// - Prevent showing the same tip twice
    /// - Support the 7-tip flow per topic
    /// - Handle the transition from random tips to educational content
    /// - Track tip counts and provide guidance on remaining tips
    /// 
    /// Design:
    /// - Dictionary<topic, List<used tip indices>>
    /// - Dictionary<topic, Random instance> for reproducibility
    /// - Separate tracking per topic to allow independent progression
    /// </summary>
    public class TipTracker
    {
        // Tracks which tips have been used for each topic (by index)
        private readonly Dictionary<string, HashSet<int>> _usedTipIndices;

        // Tracks tip count per topic (for UI feedback)
        private readonly Dictionary<string, int> _tipCounts;

        // Random generator per topic (seeded for consistency)
        private readonly Dictionary<string, Random> _topicRandomGenerators;

        /// <summary>
        /// Constructor - Initializes the tip tracker.
        /// </summary>
        public TipTracker()
        {
            _usedTipIndices = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            _tipCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _topicRandomGenerators = new Dictionary<string, Random>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// GetNextUnusedTip() - Retrieves the next unused tip for a topic.
        /// 
        /// Parameters:
        /// - topic: The cybersecurity topic
        /// - availableTips: List of available tips for the topic
        /// 
        /// Returns:
        /// - The next unused tip, or null if all tips are exhausted
        /// </summary>
        public string GetNextUnusedTip(string topic, List<string> availableTips)
        {
            if (string.IsNullOrWhiteSpace(topic) || availableTips == null || availableTips.Count == 0)
            {
                return null;
            }

            // Initialize tracking for this topic if needed
            if (!_usedTipIndices.ContainsKey(topic))
            {
                _usedTipIndices[topic] = new HashSet<int>();
                _tipCounts[topic] = 0;
                _topicRandomGenerators[topic] = new Random(topic.GetHashCode());
            }

            var usedIndices = _usedTipIndices[topic];

            // Check if all tips are exhausted
            if (usedIndices.Count >= availableTips.Count)
            {
                return null;
            }

            // Find an unused tip
            int tipIndex;
            int attempts = 0;
            int maxAttempts = availableTips.Count * 3;  // Increased attempts to find an unused tip
            const int AbsoluteMaxAttempts = 50;  // Safety limit to prevent infinite loops

            do
            {
                tipIndex = _topicRandomGenerators[topic].Next(0, availableTips.Count);
                attempts++;
            } while (usedIndices.Contains(tipIndex) && attempts < Math.Min(maxAttempts, AbsoluteMaxAttempts));

            // If we found an unused tip
            if (!usedIndices.Contains(tipIndex))
            {
                usedIndices.Add(tipIndex);
                _tipCounts[topic]++;
                return availableTips[tipIndex];
            }

            return null;
        }

        /// <summary>
        /// GetTipCount() - Returns how many tips have been shown for a topic.
        /// 
        /// Parameters:
        /// - topic: The cybersecurity topic
        /// 
        /// Returns:
        /// - Number of tips shown for this topic
        /// </summary>
        public int GetTipCount(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return 0;
            }

            return _tipCounts.ContainsKey(topic) ? _tipCounts[topic] : 0;
        }

        /// <summary>
        /// GetRemainingTipCount() - Returns how many tips remain for a topic.
        /// 
        /// Parameters:
        /// - topic: The cybersecurity topic
        /// - totalTips: Total number of tips available for this topic
        /// 
        /// Returns:
        /// - Number of remaining tips
        /// </summary>
        public int GetRemainingTipCount(string topic, int totalTips)
        {
            if (string.IsNullOrWhiteSpace(topic) || totalTips <= 0)
            {
                return 0;
            }

            int shown = GetTipCount(topic);
            return Math.Max(0, totalTips - shown);
        }

        /// <summary>
        /// HasAllTipsBeenShown() - Checks if all tips for a topic have been shown.
        /// 
        /// Parameters:
        /// - topic: The cybersecurity topic
        /// - totalTips: Total number of tips available
        /// 
        /// Returns:
        /// - true if all tips have been shown, false otherwise
        /// </summary>
        public bool HasAllTipsBeenShown(string topic, int totalTips)
        {
            if (string.IsNullOrWhiteSpace(topic) || totalTips <= 0)
            {
                return false;
            }

            return GetRemainingTipCount(topic, totalTips) <= 0;
        }

        /// <summary>
        /// ResetTopic() - Resets tip tracking for a specific topic.
        /// Useful for testing or if user wants to restart learning a topic.
        /// 
        /// Parameters:
        /// - topic: The topic to reset
        /// </summary>
        public void ResetTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                _usedTipIndices.Remove(topic);
                _tipCounts.Remove(topic);
                _topicRandomGenerators.Remove(topic);
            }
        }

        /// <summary>
        /// ResetAll() - Resets all tip tracking.
        /// Called when starting a new session.
        /// </summary>
        public void ResetAll()
        {
            _usedTipIndices.Clear();
            _tipCounts.Clear();
            _topicRandomGenerators.Clear();
        }

        /// <summary>
        /// GetTrackedTopics() - Returns a list of topics being tracked.
        /// Useful for debugging or session summary.
        /// 
        /// Returns:
        /// - List of topics with tip tracking
        /// </summary>
        public List<string> GetTrackedTopics()
        {
            return new List<string>(_usedTipIndices.Keys);
        }

        /// <summary>
        /// GetTopicProgress() - Returns progress info for a topic.
        /// 
        /// Parameters:
        /// - topic: The topic to check
        /// - totalTips: Total tips available
        /// 
        /// Returns:
        /// - A string describing progress (e.g., "3 of 7 tips shown")
        /// </summary>
        public string GetTopicProgress(string topic, int totalTips)
        {
            if (string.IsNullOrWhiteSpace(topic) || totalTips <= 0)
            {
                return "No progress tracked";
            }

            int shown = GetTipCount(topic);
            return $"{shown} of {totalTips} tips shown";
        }
    }
}
