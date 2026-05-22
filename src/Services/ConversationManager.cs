using System;
using System.Collections.Generic;
using System.Linq;
using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// ConversationManager.cs - Tracks and manages conversation context
    /// 
    /// Responsibilities:
    /// - Track the current and previous topics being discussed
    /// - Maintain user's recent sentiment across multiple messages
    /// - Track the last user intent to enable follow-up understanding
    /// - Provide context for natural conversation continuation
    /// - Enable the chatbot to understand flexible phrasing like "tell me more"
    /// 
    /// Design:
    /// - Conversation state with history (via ConversationContext)
    /// - Sentiment history for multi-turn tracking
    /// - Intent tracking for follow-up requests
    /// - Previous message context for "another", "more", etc.
    /// </summary>
    public class ConversationManager
    {
        /// <summary>
        /// Intent enumeration for tracking user intentions.
        /// </summary>
        public enum UserIntent
        {
            Unknown = 0,
            AskingTip = 1,
            RequestingEducation = 2,
            AskingQuestion = 3,
            SettingPreference = 4,
            ContinuingConversation = 5,
            ExploringSameTopic = 6
        }

        // Core conversation context
        private ConversationContext _context;

        // Additional tracking not in context
        private string _currentSentiment;
        private string _previousSentiment;
        private const int MaxSentimentHistory = 5;

        // Intent tracking
        private UserIntent _lastIntent;
        private UserIntent _previousIntent;

        // Context tracking
        private string _lastUserInput;
        private int _consecutiveFollowUpRequests;
        private const int MaxConsecutiveFollowUps = 5;

        // Topic exploration tracking
        private readonly Dictionary<string, int> _topicExplorationCount;

        /// <summary>
        /// Constructor - Initializes the conversation manager.
        /// </summary>
        public ConversationManager()
        {
            _context = new ConversationContext();
            _currentSentiment = "neutral";
            _previousSentiment = "neutral";
            _lastIntent = UserIntent.Unknown;
            _previousIntent = UserIntent.Unknown;
            _lastUserInput = string.Empty;
            _consecutiveFollowUpRequests = 0;
            _topicExplorationCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        // ===== TOPIC TRACKING =====

        /// <summary>
        /// UpdateTopic() - Updates the current topic being discussed.
        /// Automatically moves current to previous via ConversationContext.
        /// 
        /// Parameters:
        /// - topic: The new current topic
        /// </summary>
        public void UpdateTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                // Store in context
                _context.PreviousTopic = _context.CurrentTopic;
                _context.CurrentTopic = topic.ToLower();

                // Track exploration count
                if (!_topicExplorationCount.ContainsKey(topic))
                {
                    _topicExplorationCount[topic] = 0;
                }
                _topicExplorationCount[topic]++;
            }
        }

        /// <summary>
        /// GetCurrentTopic() - Returns the current topic from context.
        /// </summary>
        public string GetCurrentTopic() => _context.CurrentTopic;

        /// <summary>
        /// GetPreviousTopic() - Returns the previous topic from context.
        /// </summary>
        public string GetPreviousTopic() => _context.PreviousTopic;

        /// <summary>
        /// HasCurrentTopic() - Checks if a topic is currently active.
        /// </summary>
        public bool HasCurrentTopic() => !string.IsNullOrEmpty(_context.CurrentTopic);

        /// <summary>
        /// GetTopicExplorationCount() - Returns how many times a topic has been explored.
        /// </summary>
        public int GetTopicExplorationCount(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return 0;
            }

            return _topicExplorationCount.ContainsKey(topic) ? _topicExplorationCount[topic] : 0;
        }

        // ===== SENTIMENT TRACKING =====

        /// <summary>
        /// UpdateSentiment() - Updates the current sentiment.
        /// Automatically maintains sentiment history in context.
        /// 
        /// Parameters:
        /// - sentiment: The new sentiment (e.g., "worried", "curious")
        /// </summary>
        public void UpdateSentiment(string sentiment)
        {
            if (!string.IsNullOrWhiteSpace(sentiment))
            {
                _previousSentiment = _currentSentiment;
                _currentSentiment = sentiment.ToLower();

                // Add to context's sentiment history
                _context.AddSentiment(_currentSentiment);
            }
        }

        /// <summary>
        /// GetCurrentSentiment() - Returns the current sentiment.
        /// </summary>
        public string GetCurrentSentiment() => _currentSentiment;

        /// <summary>
        /// GetPreviousSentiment() - Returns the previous sentiment.
        /// </summary>
        public string GetPreviousSentiment() => _previousSentiment;

        /// <summary>
        /// GetDominantSentiment() - Returns the most common sentiment from recent history.
        /// Useful for understanding overall user mood.
        /// </summary>
        public string GetDominantSentiment()
        {
            if (_context.SentimentHistory.Count == 0)
            {
                return "neutral";
            }

            return _context.SentimentHistory
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        /// <summary>
        /// IsSentimentChanged() - Detects if sentiment has changed from previous.
        /// </summary>
        public bool IsSentimentChanged()
        {
            return !_currentSentiment.Equals(_previousSentiment, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// IsSentimentNegative() - Checks if current sentiment indicates concern.
        /// </summary>
        public bool IsSentimentNegative()
        {
            return _currentSentiment == "worried" || _currentSentiment == "frustrated";
        }

        // ===== INTENT TRACKING =====

        /// <summary>
        /// UpdateIntent() - Updates the user's current intent.
        /// Automatically moves current to previous.
        /// 
        /// Parameters:
        /// - intent: The detected user intent
        /// </summary>
        public void UpdateIntent(UserIntent intent)
        {
            _previousIntent = _lastIntent;
            _lastIntent = intent;

            // Reset follow-up counter if intent changed significantly
            if (intent != UserIntent.ContinuingConversation && intent != UserIntent.ExploringSameTopic)
            {
                _consecutiveFollowUpRequests = 0;
            }
            else
            {
                _consecutiveFollowUpRequests++;
            }
        }

        /// <summary>
        /// GetCurrentIntent() - Returns the current intent.
        /// </summary>
        public UserIntent GetCurrentIntent() => _lastIntent;

        /// <summary>
        /// GetPreviousIntent() - Returns the previous intent.
        /// </summary>
        public UserIntent GetPreviousIntent() => _previousIntent;

        /// <summary>
        /// IsFollowUpRequest() - Checks if this appears to be a follow-up request.
        /// </summary>
        public bool IsFollowUpRequest()
        {
            return _lastIntent == UserIntent.ContinuingConversation ||
                   _lastIntent == UserIntent.ExploringSameTopic;
        }

        /// <summary>
        /// HasExcessiveFollowUps() - Detects if user is stuck in follow-up loop.
        /// </summary>
        public bool HasExcessiveFollowUps()
        {
            return _consecutiveFollowUpRequests > MaxConsecutiveFollowUps;
        }

        // ===== CONVERSATION CONTEXT =====

        /// <summary>
        /// UpdateContext() - Updates the last user input and interaction time.
        /// Called at the start of each interaction.
        /// 
        /// Parameters:
        /// - userInput: The user's latest input
        /// </summary>
        public void UpdateContext(string userInput)
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                _lastUserInput = userInput.ToLower();
                _context.LastInteractionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// GetLastUserInput() - Returns the last user input.
        /// </summary>
        public string GetLastUserInput() => _lastUserInput;

        /// <summary>
        /// GetSecondsSinceLastInteraction() - Returns time elapsed since last interaction.
        /// Useful for detecting pauses or new conversations.
        /// </summary>
        public int GetSecondsSinceLastInteraction()
        {
            return (int)(DateTime.Now - _context.LastInteractionTime).TotalSeconds;
        }

        /// <summary>
        /// IsNewConversationContext() - Detects if the conversation has a significant pause.
        /// Useful for resetting context after long pauses.
        /// </summary>
        public bool IsNewConversationContext()
        {
            return GetSecondsSinceLastInteraction() > 300; // 5 minutes
        }

        // ===== NATURAL LANGUAGE HELPERS =====

        /// <summary>
        /// CanInferFollowUp() - Checks if a follow-up like "another one" can be understood.
        /// Returns true if there's active context (current topic + recent interaction).
        /// </summary>
        public bool CanInferFollowUp()
        {
            return HasCurrentTopic() && GetSecondsSinceLastInteraction() < 120; // 2 minutes
        }

        /// <summary>
        /// SuggestContextualResponse() - Provides context-aware response hints.
        /// Useful for generating natural, contextual responses.
        /// 
        /// Returns:
        /// - String describing the context (e.g., "continuing phishing tips")
        /// </summary>
        public string SuggestContextualResponse()
        {
            if (!HasCurrentTopic())
            {
                return "unknown";
            }

            return _lastIntent switch
            {
                UserIntent.AskingTip => $"continuing {_context.CurrentTopic} tips",
                UserIntent.ExploringSameTopic => $"exploring {_context.CurrentTopic} further",
                UserIntent.RequestingEducation => $"providing {_context.CurrentTopic} education",
                _ => $"discussing {_context.CurrentTopic}"
            };
        }

        // ===== PHASE 1: FOLLOW-UP SUPPORT =====

        /// <summary>
        /// GetContext() - Returns the ConversationContext for external access.
        /// Allows Chatbot.cs to inspect and update conversation state directly.
        /// </summary>
        public ConversationContext GetContext()
        {
            return _context;
        }

        /// <summary>
        /// IsValidFollowUp() - Checks if the current context supports follow-up.
        /// </summary>
        public bool IsValidFollowUp()
        {
            return _context.IsValidForFollowUp();
        }

        /// <summary>
        /// IncrementTipsShown() - Increments the tip counter for tracking.
        /// </summary>
        public void IncrementTipsShown()
        {
            _context.TipsShownCount++;
        }

        /// <summary>
        /// ResetTipsShown() - Resets tip counter when topic changes.
        /// </summary>
        public void ResetTipsShown()
        {
            _context.TipsShownCount = 0;
        }

        /// <summary>
        /// SetIntentType() - Sets the intent type in context.
        /// </summary>
        public void SetIntentType(string intentType)
        {
            if (!string.IsNullOrWhiteSpace(intentType))
            {
                _context.LastIntentType = intentType;
            }
        }

        /// <summary>
        /// SetResponseCategory() - Sets the response category in context.
        /// </summary>
        public void SetResponseCategory(string responseCategory)
        {
            if (!string.IsNullOrWhiteSpace(responseCategory))
            {
                _context.LastResponseCategory = responseCategory;
            }
        }
    }
}
