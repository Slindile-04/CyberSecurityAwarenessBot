using System;
using System.Collections.Generic;

namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// Represents the state of a conversation at any given moment.
    /// Tracks topics, intents, response types, and conversation flow.
    /// </summary>
    public class ConversationContext
    {
        /// <summary>
        /// The current topic being discussed (e.g., "phishing", "passwords")
        /// </summary>
        public string CurrentTopic { get; set; }

        /// <summary>
        /// The previous topic discussed (enables topic switching detection)
        /// </summary>
        public string PreviousTopic { get; set; }

        /// <summary>
        /// Type of intent from last user input (e.g., "TipRequest", "Education")
        /// </summary>
        public string LastIntentType { get; set; }

        /// <summary>
        /// Category of last response given (e.g., "Tip", "Education", "FollowUp")
        /// </summary>
        public string LastResponseCategory { get; set; }

        /// <summary>
        /// Number of tips shown for the current topic
        /// </summary>
        public int TipsShownCount { get; set; }

        /// <summary>
        /// Whether we're currently in education/deep-dive mode for this topic
        /// </summary>
        public bool IsInEducationMode { get; set; }

        /// <summary>
        /// Depth of education (1=intro, 2=intermediate, 3=comprehensive)
        /// </summary>
        public int EducationDepth { get; set; }

        /// <summary>
        /// History of detected sentiments in conversation (last 5)
        /// </summary>
        public List<string> SentimentHistory { get; set; }

        /// <summary>
        /// Timestamp of last user interaction
        /// </summary>
        public DateTime LastInteractionTime { get; set; }

        /// <summary>
        /// Whether conversation is still active (hasn't timed out)
        /// </summary>
        public bool IsConversationActive { get; set; }

        /// <summary>
        /// Flag to track if we should prompt user to continue on current topic
        /// or ask what they want to explore next
        /// </summary>
        public bool ShouldPromptForContinuation { get; set; }

        public ConversationContext()
        {
            // Initialize default values
            CurrentTopic = null;
            PreviousTopic = null;
            LastIntentType = "Unknown";
            LastResponseCategory = "Unknown";
            TipsShownCount = 0;
            IsInEducationMode = false;
            EducationDepth = 0;
            SentimentHistory = new List<string>();
            LastInteractionTime = DateTime.Now;
            IsConversationActive = true;
            ShouldPromptForContinuation = false;
        }

        /// <summary>
        /// Checks if the current context is valid for following up on a topic
        /// </summary>
        public bool IsValidForFollowUp()
        {
            return !string.IsNullOrWhiteSpace(CurrentTopic) &&
                   (DateTime.Now - LastInteractionTime).TotalMinutes < 30; // 30 minute timeout
        }

        /// <summary>
        /// Adds a sentiment to the history and keeps only last 5
        /// </summary>
        public void AddSentiment(string sentiment)
        {
            if (!string.IsNullOrWhiteSpace(sentiment))
            {
                SentimentHistory.Add(sentiment);
                if (SentimentHistory.Count > 5)
                {
                    SentimentHistory.RemoveAt(0); // Keep only last 5
                }
            }
        }

        /// <summary>
        /// Gets the most recent sentiment from history
        /// </summary>
        public string GetMostRecentSentiment()
        {
            return SentimentHistory.Count > 0 ? SentimentHistory[SentimentHistory.Count - 1] : "neutral";
        }

        /// <summary>
        /// Clears context for a new conversation topic
        /// </summary>
        public void ResetForNewTopic()
        {
            PreviousTopic = CurrentTopic;
            CurrentTopic = null;
            TipsShownCount = 0;
            IsInEducationMode = false;
            EducationDepth = 0;
            LastIntentType = "Unknown";
            LastResponseCategory = "Unknown";
            ShouldPromptForContinuation = false;
        }

        /// <summary>
        /// Returns a summary of the current context for debugging
        /// </summary>
        public override string ToString()
        {
            return $"ConversationContext{{\n" +
                   $"  CurrentTopic: {CurrentTopic}\n" +
                   $"  PreviousTopic: {PreviousTopic}\n" +
                   $"  LastIntentType: {LastIntentType}\n" +
                   $"  LastResponseCategory: {LastResponseCategory}\n" +
                   $"  TipsShownCount: {TipsShownCount}\n" +
                   $"  IsInEducationMode: {IsInEducationMode}\n" +
                   $"  EducationDepth: {EducationDepth}\n" +
                   $"  MostRecentSentiment: {GetMostRecentSentiment()}\n" +
                   $"  IsValidForFollowUp: {IsValidForFollowUp()}\n" +
                   $"}}";
        }
    }
}
