using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot.Core
{
    /// <summary>
    /// SentimentAnalyzer.cs - Detects user sentiment using rule-based keyword matching
    /// 
    /// Responsibilities:
    /// - Analyze user input for emotional tone
    /// - Detect sentiment: Worried, Frustrated, Curious, Neutral
    /// - No external APIs or ML models (simple, scalable approach)
    /// - Provide sentiment-based response suggestions
    /// 
    /// Design:
    /// - Keyword lists for each sentiment type
    /// - Case-insensitive matching
    /// - Prioritized detection (some sentiments checked before others)
    /// </summary>
    public class SentimentAnalyzer
    {
        // Define sentiment keyword lists (case-insensitive)
        private readonly List<string> _worriedKeywords;
        private readonly List<string> _frustratedKeywords;
        private readonly List<string> _curiousKeywords;
        private readonly List<string> _positiveKeywords;

        /// <summary>
        /// Sentiment enumeration for type-safe sentiment representation.
        /// </summary>
        public enum Sentiment
        {
            Neutral = 0,
            Curious = 1,
            Worried = 2,
            Frustrated = 3,
            Positive = 4
        }

        /// <summary>
        /// Constructor - Initializes sentiment keyword lists.
        /// </summary>
        public SentimentAnalyzer()
        {
            _worriedKeywords = new List<string>
            {
                "scared", "worry", "worried", "afraid", "unsafe", "danger",
                "anxious", "anxiety", "panic", "fear", "threat", "concerned",
                "nervous", "terror", "frightened", "alarmed", "dread", "unease",
                "vulnerable", "exposed", "risk", "risky", "dangerous", "worry about"
            };

            _frustratedKeywords = new List<string>
            {
                "confused", "confusing", "frustrat", "understand", "unclear",
                "complicated", "complex", "hard", "difficult", "struggle",
                "lost", "overwhelm", "don't get it", "don't know", "how do",
                "explain", "lost", "bewild", "perplex", "puzzle", "annoying",
                "irritat", "tire", "exhausted", "fed up", "this sucks"
            };

            _curiousKeywords = new List<string>
            {
                "how", "why", "what", "when", "where", "tell me more",
                "curious", "interested", "interested in", "like to know",
                "want to learn", "intrigu", "fascin", "question", "ask",
                "explain", "elaborate", "deeper", "more about", "details",
                "help me understand", "can you tell me", "i want to know"
            };

            _positiveKeywords = new List<string>
            {
                "great", "awesome", "excellent", "perfect", "thank you",
                "thanks", "appreciate", "helpful", "useful", "good",
                "love", "amazing", "fantastic", "wonderful", "brilliant",
                "learned a lot", "understand now", "makes sense", "clear now"
            };
        }

        /// <summary>
        /// DetectSentiment() - Analyzes input and returns sentiment as string.
        /// Returns "worried", "frustrated", "curious", "positive", or "neutral".
        /// 
        /// Parameters:
        /// - input: User input text to analyze
        /// 
        /// Returns:
        /// - Sentiment string: "worried", "frustrated", "curious", "positive", "neutral"
        /// </summary>
        public string DetectSentiment(string input)
        {
            var sentiment = DetectSentimentEnum(input);
            return sentiment.ToString().ToLower();
        }

        /// <summary>
        /// DetectSentimentEnum() - Analyzes input and returns sentiment as enum.
        /// Uses enum for type-safe handling.
        /// 
        /// Parameters:
        /// - input: User input text to analyze
        /// 
        /// Returns:
        /// - Sentiment enum value
        /// </summary>
        public Sentiment DetectSentimentEnum(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Sentiment.Neutral;
            }

            string lowerInput = input.ToLower();

            // Check sentiments in priority order
            // Worried takes precedence (safety concern)
            if (ContainsKeywords(lowerInput, _worriedKeywords))
            {
                return Sentiment.Worried;
            }

            // Frustrated is checked next
            if (ContainsKeywords(lowerInput, _frustratedKeywords))
            {
                return Sentiment.Frustrated;
            }

            // Positive (affirmation)
            if (ContainsKeywords(lowerInput, _positiveKeywords))
            {
                return Sentiment.Positive;
            }

            // Curious (learning intent)
            if (ContainsKeywords(lowerInput, _curiousKeywords))
            {
                return Sentiment.Curious;
            }

            // Default to neutral if no sentiment detected
            return Sentiment.Neutral;
        }

        /// <summary>
        /// ContainsKeywords() - Helper method to check if input contains any keywords.
        /// This method handles partial matches (e.g., "frustrated" matches "frustrat").
        /// 
        /// Parameters:
        /// - input: The input text (already lowercased)
        /// - keywords: List of keywords to search for
        /// 
        /// Returns:
        /// - true if any keyword is found in input, false otherwise
        /// </summary>
        private bool ContainsKeywords(string input, List<string> keywords)
        {
            return keywords.Any(keyword => input.Contains(keyword));
        }

        /// <summary>
        /// GetSentimentResponse() - Provides a suggested response prefix based on sentiment.
        /// These can be prepended to bot responses for sentiment-aware interaction.
        /// 
        /// Parameters:
        /// - sentiment: The detected sentiment
        /// 
        /// Returns:
        /// - Suggested response prefix or empty string for neutral
        /// </summary>
        public string GetSentimentResponse(string sentiment)
        {
            return GetSentimentResponse(ParseSentiment(sentiment));
        }

        /// <summary>
        /// GetSentimentResponse() - Provides a suggested response prefix based on sentiment enum.
        /// 
        /// Parameters:
        /// - sentiment: The sentiment enum value
        /// 
        /// Returns:
        /// - Suggested response prefix or empty string for neutral
        /// </summary>
        public string GetSentimentResponse(Sentiment sentiment)
        {
            return sentiment switch
            {
                Sentiment.Worried => "I understand this can feel concerning, and you're taking a positive step by learning. ",
                Sentiment.Frustrated => "I completely understand this can be confusing. Let me break it down simply. ",
                Sentiment.Curious => "Great question! I love your curiosity. ",
                Sentiment.Positive => "I'm glad you found that helpful! ",
                _ => string.Empty  // Neutral - no prefix
            };
        }

        /// <summary>
        /// ParseSentiment() - Converts string sentiment to enum.
        /// Helper method for sentiment string conversions.
        /// 
        /// Parameters:
        /// - sentimentStr: The sentiment as a string
        /// 
        /// Returns:
        /// - Corresponding Sentiment enum, or Neutral if invalid
        /// </summary>
        private Sentiment ParseSentiment(string sentimentStr)
        {
            if (Enum.TryParse<Sentiment>(sentimentStr, ignoreCase: true, out var result))
            {
                return result;
            }
            return Sentiment.Neutral;
        }

        /// <summary>
        /// IsSentiment() - Checks if input matches a specific sentiment.
        /// Convenience method for conditional logic.
        /// 
        /// Parameters:
        /// - input: User input text
        /// - targetSentiment: The sentiment to check for
        /// 
        /// Returns:
        /// - true if detected sentiment matches target, false otherwise
        /// </summary>
        public bool IsSentiment(string input, Sentiment targetSentiment)
        {
            return DetectSentimentEnum(input) == targetSentiment;
        }

        /// <summary>
        /// IsSentiment() - String-based sentiment check.
        /// 
        /// Parameters:
        /// - input: User input text
        /// - sentimentStr: The sentiment as a string (e.g., "worried", "curious")
        /// 
        /// Returns:
        /// - true if detected sentiment matches target, false otherwise
        /// </summary>
        public bool IsSentiment(string input, string sentimentStr)
        {
            var detectedSentiment = DetectSentimentEnum(input);
            var targetSentiment = ParseSentiment(sentimentStr);
            return detectedSentiment == targetSentiment;
        }

        /// <summary>
        /// AddKeyword() - Allows dynamic addition of keywords for sentiment detection.
        /// Enables customization and extensibility.
        /// 
        /// Parameters:
        /// - sentiment: The sentiment type ("worried", "frustrated", "curious", "positive")
        /// - keyword: The keyword to add
        /// </summary>
        public void AddKeyword(string sentiment, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            string lowerSentiment = sentiment.ToLower();
            string lowerKeyword = keyword.ToLower();

            switch (lowerSentiment)
            {
                case "worried":
                    if (!_worriedKeywords.Contains(lowerKeyword))
                    {
                        _worriedKeywords.Add(lowerKeyword);
                    }
                    break;

                case "frustrated":
                    if (!_frustratedKeywords.Contains(lowerKeyword))
                    {
                        _frustratedKeywords.Add(lowerKeyword);
                    }
                    break;

                case "curious":
                    if (!_curiousKeywords.Contains(lowerKeyword))
                    {
                        _curiousKeywords.Add(lowerKeyword);
                    }
                    break;

                case "positive":
                    if (!_positiveKeywords.Contains(lowerKeyword))
                    {
                        _positiveKeywords.Add(lowerKeyword);
                    }
                    break;
            }
        }

        /// <summary>
        /// GetEmoji() - Returns a fitting emoji for the sentiment.
        /// Useful for visual feedback in responses.
        /// 
        /// Parameters:
        /// - sentiment: The sentiment type
        /// 
        /// Returns:
        /// - Emoji string representing the sentiment
        /// </summary>
        public string GetEmoji(string sentiment)
        {
            return sentiment.ToLower() switch
            {
                "worried" => "😟",
                "frustrated" => "😤",
                "curious" => "🤔",
                "positive" => "😊",
                _ => "💬"
            };
        }
    }
}
