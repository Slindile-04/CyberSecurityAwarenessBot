using CyberSecurityAwarenessBot.Helpers;
using CyberSecurityAwarenessBot.Services;

namespace CyberSecurityAwarenessBot.Core
{
    /// <summary>
    /// Chatbot.cs - Core conversation logic for the Cyber Security Awareness Bot
    /// 
    /// Responsibilities:
    /// - Handle all chatbot interactions and conversation logic
    /// - Process both menu-based numeric inputs and natural language questions
    /// - Provide personalized responses using the user's name
    /// - Educate users about cybersecurity best practices
    /// - Manage the conversation loop
    /// 
    /// Key Features:
    /// - Conversational input handling (not just menu-based)
    /// - Keyword detection for case-insensitive matching
    /// - Personalized responses with user's name
    /// - Graceful handling of invalid or empty inputs
    /// - Support for both structured topics and natural language questions
    /// </summary>
    public class CyberSecurityAwarenessBot
    {
        private readonly string _audioPath;
        private readonly InputHelper _inputHelper;
        private readonly string _userName;
        private bool _isRunning;
        private readonly TipRepository _tipRepository;

        // Conversation state tracking for Step 3 & 4
        private string? _currentTopic;
        private int _tipCount;

        // Step 5: Memory and Recall
        private readonly UserMemory _userMemory;

        // Step 6: Sentiment Detection
        private readonly SentimentAnalyzer _sentimentAnalyzer;

        // Track sentiment frequency to avoid repetitive responses
        private string? _lastSentimentResponse;
        private int _sentimentResponseCount;

        // NEW: Conversation context and tip tracking services
        private readonly ConversationManager _conversationManager;
        private readonly TipTracker _tipTracker;

        /// <summary>
        /// Constructor - Initializes the chatbot with audio path and user's name for personalization.
        /// </summary>
        public CyberSecurityAwarenessBot(string audioPath, string userName)
        {
            _audioPath = audioPath;
            _userName = userName;
            _inputHelper = new InputHelper();
            _isRunning = false;
            _tipRepository = new TipRepository();
            _currentTopic = null;
            _tipCount = 0;

            // Initialize memory and sentiment analysis
            _userMemory = new UserMemory();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _lastSentimentResponse = null;
            _sentimentResponseCount = 0;

            // Initialize new conversation management services
            _conversationManager = new ConversationManager();
            _tipTracker = new TipTracker();
        }

        /// <summary>
        /// Start() - Initiates the chatbot's main conversation loop.
        /// Displays welcome message and keeps the conversation running until the user exits.
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            DisplayWelcomeMessage();
            InteractionLoop();
        }

        /// <summary>
        /// InteractionLoop() - Main conversation loop that continuously prompts for user input.
        /// 
        /// Features:
        /// - Supports both numeric menu selection and natural language input
        /// - Validates input and handles empty responses gracefully
        /// - Processes user input and provides appropriate responses
        /// - Continues until the user decides to exit
        /// - Uses color-coded prompts for visual clarity
        /// </summary>
        private void InteractionLoop()
        {
            while (_isRunning)
            {
                DisplayMenuOptions();
                UIHelper.PrintColoredLine($"\n{_userName}, what would you like to know? ", ConsoleColor.White, false);

                string? userInput = _inputHelper.GetValidInput();

                // Handle empty input gracefully
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    UIHelper.DisplayBotMessage($"I didn't catch that, {_userName}. Please enter something or type 'help' for options.", ConsoleColor.Yellow);
                    continue;
                }

                // Process the user's input
                HandleUserInput(userInput);
            }
        }

        /// <summary>
        /// DisplayMenuOptions() - Shows the available topics and commands in a user-friendly format.
        /// Provides both numeric menu options and text-based keywords for conversational input.
        /// Uses cyan color for visual consistency with the title screen.
        /// </summary>
        private void DisplayMenuOptions()
        {
            UIHelper.PrintColoredLine("\n╔════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║             Cybersecurity Topics (Numeric Menu)            ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╠════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  1. Phishing Attacks   | 2. Strong Passwords               ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  3. Two-Factor Auth    | 4. Data Privacy                   ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  5. Secure Browsing    | 6. Ransomware                     ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  7. Social Engineering | 8. Patch Management               ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  9. Public Wi-Fi Safety| 10. Password Managers             ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║  0. Exit               |                                   ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╠════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ Or just ask naturally:                                     ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"How are you?\"             • \"What's your purpose?\"      ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"Tell me about 'phishing', 'privacy', etc\"               ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"Give me a phishing tip\"                                 ║ ", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"help\"                                                   ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╚════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }


        /// <summary>
        /// HandleUserInput() - Processes user input and routes to appropriate handler.
        /// 
        /// NOW INCLUDES:
        /// - Conversation context tracking (ConversationManager)
        /// - Sentiment analysis (SentimentAnalyzer)
        /// - Interest/preference detection (UserMemory)
        /// - Improved follow-up phrase detection
        /// 
        /// Supports:
        /// - Numeric menu selections (1-10, 0)
        /// - Tip requests with sentiment-aware responses
        /// - Continuation requests ("another tip", "tell me more", "what else", etc.)
        /// - Conversational queries
        /// - Interest statements ("I'm interested in privacy", "I like learning about phishing")
        /// - Exit commands
        /// - Help command
        /// </summary>
        private void HandleUserInput(string input)
        {
            string lowerInput = input.Trim().ToLower();

            // Update conversation context
            _conversationManager.UpdateContext(input);

            // STEP 6: Analyze sentiment BEFORE responding
            string sentiment = _sentimentAnalyzer.DetectSentiment(lowerInput);
            _conversationManager.UpdateSentiment(sentiment);

            // STEP 5: Check for interest/preference statements
            CheckAndStoreInterests(input);

            // Check for tip requests with improved follow-up phrase support
            if (IsTipRequest(lowerInput))
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.AskingTip);
                HandleTipRequest(lowerInput, sentiment);
                return;
            }

            // Improved follow-up detection - supports "tell me more", "another", "what else", "continue", etc.
            if (IsFlexibleFollowUpRequest(lowerInput) && _conversationManager.CanInferFollowUp())
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.ContinuingConversation);
                HandleTipRequest(lowerInput, sentiment);
                return;
            }

            // If user said something like "go deeper", "deeper", "elaborate", etc. and we're in a topic
            if (IsDeepDiveRequest(lowerInput) && _conversationManager.HasCurrentTopic())
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.ExploringSameTopic);
                ProvideDeepDiveEducation(_conversationManager.GetCurrentTopic(), sentiment);
                return;
            }

            // Detect if switching topics
            string potentialNewTopic = DetectTopicFromInput(lowerInput);
            if (potentialNewTopic != null &&
                (string.IsNullOrEmpty(_currentTopic) || !potentialNewTopic.Equals(_currentTopic, StringComparison.OrdinalIgnoreCase)))
            {
                _conversationManager.UpdateTopic(potentialNewTopic);
                _currentTopic = potentialNewTopic;
                _tipCount = 0;
            }

            // Check for numeric menu selections
            switch (lowerInput)
            {
                case "1":
                case "phishing":
                case "phishing attacks":
                case "tell me about phishing":
                case "tell me about phishing attacks":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("phishing");
                    ProvideEducationWithSentiment("phishing", sentiment);
                    return;
                case "2":
                case "password":
                case "passwords":
                case "tell me about passwords":
                case "tell me about strong passwords":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("passwords");
                    ProvideEducationWithSentiment("passwords", sentiment);
                    return;
                case "3":
                case "2fa":
                case "two-factor":
                case "two factor":
                case "authentication":
                case "tell me about authentication":
                case "tell me about two factor authentication":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("2fa");
                    ProvideEducationWithSentiment("2fa", sentiment);
                    return;
                case "4":
                case "privacy":
                case "data privacy":
                case "tell me about data privacy":
                case "tell me about privacy":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("privacy");
                    ProvideEducationWithSentiment("privacy", sentiment);
                    return;
                case "5":
                case "browsing":
                case "secure browsing":
                case "tell me about secure browsing":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("browsing");
                    ProvideEducationWithSentiment("browsing", sentiment);
                    return;
                case "6":
                case "ransomware":
                case "tell me about ransomware":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("ransomware");
                    ProvideEducationWithSentiment("ransomware", sentiment);
                    return;
                case "7":
                case "social engineering":
                case "tell me about social engineering":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("social engineering");
                    ProvideEducationWithSentiment("social engineering", sentiment);
                    return;
                case "8":
                case "software updates":
                case "patch management":
                case "tell me about patch management":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("patch management");
                    ProvideEducationWithSentiment("patch management", sentiment);
                    return;
                case "9":
                case "wifi":
                case "wi-fi":
                case "public wifi":
                case "public wi-fi":
                case "vpn":
                case "tell me about wifi":
                case "tell me about wi-fi":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("wifi");
                    ProvideEducationWithSentiment("wifi", sentiment);
                    return;
                case "10":
                case "password manager":
                case "password managers":
                case "tell me about password managers":
                    _conversationManager.UpdateIntent(ConversationManager.UserIntent.RequestingEducation);
                    _conversationManager.UpdateTopic("password manager");
                    ProvideEducationWithSentiment("password manager", sentiment);
                    return;
                case "0":
                case "exit":
                case "quit":
                case "bye":
                case "goodbye":
                case "farewell":
                    _isRunning = false;
                    Console.WriteLine();
                    UIHelper.DisplayBotMessage($"Take care, {_userName}! Remember to stay secure online! 🔒", ConsoleColor.Green);
                    return;
            }

            // Check for conversational keywords using Contains for flexibility
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how are ya") || lowerInput.Contains("how are you doing"))
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.AskingQuestion);
                RespondToGreeting();
                return;
            }

            if (lowerInput.Contains("what") && (lowerInput.Contains("purpose") || lowerInput.Contains("do you do") || lowerInput.Contains("help")))
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.AskingQuestion);
                RespondToPurpose();
                return;
            }

            if (lowerInput.Contains("help") || lowerInput.Contains("options") || lowerInput.Contains("topics"))
            {
                _conversationManager.UpdateIntent(ConversationManager.UserIntent.AskingQuestion);
                DisplayHelpMessage();
                return;
            }

            // If input doesn't match any known pattern, provide a friendly suggestion
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"I'm not sure how to respond to that, {_userName}. Try asking about one of the topics above,", ConsoleColor.Yellow);
            UIHelper.DisplayBotMessage("or type 'help' for a list of available options.", ConsoleColor.Yellow);
        }

        /// <summary>
        /// STEP 3 & 4: Tip System and Conversation Flow Helper Methods
        /// These methods handle random tip selection and conversation state tracking.
        /// </summary>

        /// <summary>
        /// IsTipRequest() - Detects if the user is asking for a tip.
        /// Examples: "give me a phishing tip", "password tip", "any 2fa advice"
        /// </summary>
        private bool IsTipRequest(string input)
        {
            return (input.Contains("tip") || input.Contains("advice")) &&
                   (input.Contains("give") || input.Contains("any") || input.Contains("tell") || input.Contains("about"));
        }

        /// <summary>
        /// STEP 5 & 6: Memory, Recall, and Sentiment Detection Helper Methods
        /// These methods handle user memory, interest detection, and sentiment-aware responses.
        /// </summary>

        /// <summary>
        /// CheckAndStoreInterests() - Detects and stores user interests with natural responses (ISSUE 1).
        /// Looks for patterns like "I'm interested in", "I like learning about", "I care about"
        /// 
        /// Examples:
        /// - "I'm interested in privacy"
        /// - "I like learning about phishing"
        /// - "privacy is important to me"
        /// </summary>
        private void CheckAndStoreInterests(string input)
        {
            string lowerInput = input.ToLower();

            // Interest statement patterns
            var interestPatterns = new List<string>
            {
                "interested in",
                "interested about",
                "like learning about",
                "like to learn about",
                "care about",
                "is important to me",
                "concerned about",
                "want to know about",
                "want to learn about",
                "learn more about",
                "understand more about",
                "need to know about"
            };

            // Check if input contains interest patterns
            bool hasInterestPattern = interestPatterns.Any(pattern => lowerInput.Contains(pattern));

            if (hasInterestPattern)
            {
                // Extract topic from the input
                string topic = MapKeywordToTopic(input);
                if (topic != null)
                {
                    // Check if this is a new interest or updating existing
                    bool isNewInterest = !_userMemory.IsInterested(topic);

                    // Store the interest in memory
                    _userMemory.AddInterest(topic);

                    // Provide natural, personalized response
                    Console.WriteLine();
                    if (isNewInterest)
                    {
                        // New interest
                        UIHelper.DisplayBotMessage($"Great! I'll remember that you're interested in {topic}, {_userName}. 📌", ConsoleColor.Green);

                        // If first interest, set as favorite
                        if (!_userMemory.HasFavoriteTopic())
                        {
                            _userMemory.SetFavoriteTopic(topic);
                            UIHelper.DisplayBotMessage($"I'll focus on {topic} as your primary interest. You can always explore other topics too! 🎯", ConsoleColor.Green);
                        }
                    }
                    else
                    {
                        // Interest already recorded - acknowledge update/reiteration
                        UIHelper.DisplayBotMessage($"Yes, {topic} is definitely an important area to master! I'll keep focusing on that for you. 💪", ConsoleColor.Green);
                    }
                }
            }
        }

        /// <summary>
        /// ProvideEducationWithSentiment() - Routes to education method with sentiment awareness.
        /// Adds sentiment-aware introduction before full education.
        /// </summary>
        private void ProvideEducationWithSentiment(string topic, string sentiment)
        {
            // Record the topic as discussed
            _userMemory.AddDiscussedTopic(topic);

            // Phase 1: Update conversation context for education flow
            _conversationManager.SetIntentType("Education");
            _conversationManager.SetResponseCategory("Education");
            _conversationManager.UpdateTopic(topic);
            _conversationManager.ResetTipsShown();

            Console.WriteLine();

            // STEP 6: Add sentiment-aware introduction
            string intro = GetEducationIntroduction(sentiment, topic);
            if (!string.IsNullOrEmpty(intro))
            {
                UIHelper.DisplayBotMessage(intro, ConsoleColor.Green);
                Console.WriteLine();
            }

            // STEP 5: Add memory-based personalization if applicable
            string memoryPersonalization = GetMemoryPersonalization(topic);
            if (!string.IsNullOrEmpty(memoryPersonalization))
            {
                UIHelper.DisplayBotMessage(memoryPersonalization, ConsoleColor.Cyan);
                Console.WriteLine();
            }

            // Call the appropriate education method
            switch (topic.ToLower())
            {
                case "phishing":
                    ProvidePhishingEducation();
                    break;
                case "passwords":
                    ProvidePasswordEducation();
                    break;
                case "2fa":
                    Provide2FAEducation();
                    break;
                case "privacy":
                    ProvidePrivacyEducation();
                    break;
                case "browsing":
                    ProvideBrowsingEducation();
                    break;
                case "ransomware":
                    ProvideRansomwareEducation();
                    break;
                case "social engineering":
                    ProvideSocialEngineeringEducation();
                    break;
                case "patch management":
                    ProvidePatchManagementEducation();
                    break;
                case "wifi":
                    ProvidePublicWifiEducation();
                    break;
                case "password manager":
                    ProvidePasswordManagerEducation();
                    break;
            }
        }

        /// <summary>
        /// GetSentimentAwarePrefix() - Returns a sentiment-aware prefix for tips.
        /// Adjusted tone based on the user's emotional state.
        /// </summary>
        private string GetSentimentAwarePrefix(string sentiment, string topic)
        {
            // Avoid repeating the same sentiment response too often
            if (_lastSentimentResponse == sentiment)
            {
                _sentimentResponseCount++;
                if (_sentimentResponseCount > 2)
                {
                    return string.Empty;  // Don't repeat the same intro
                }
            }
            else
            {
                _lastSentimentResponse = sentiment;
                _sentimentResponseCount = 0;
            }

            return sentiment switch
            {
                "worried" => $"I understand {topic} might feel overwhelming, but I'm here to help you understand it better. 💚",
                "frustrated" => $"I know {topic} can seem complicated, but we'll break it down step by step. 😊",
                "curious" => $"I love your curiosity about {topic}! Let's dive in. 🚀",
                "positive" => $"Glad to see your enthusiasm about {topic}! 👍",
                _ => string.Empty
            };
        }

        /// <summary>
        /// GetEducationIntroduction() - Returns a sentiment-aware introduction for full education.
        /// Sets the tone for the detailed explanation.
        /// </summary>
        private string GetEducationIntroduction(string sentiment, string topic)
        {
            return sentiment switch
            {
                "worried" => $"I understand {topic} might feel concerning. Let me explain this clearly so you feel prepared and confident. 💡",
                "frustrated" => $"I know {topic} can be confusing. I'll explain this in the simplest way possible. 📚",
                "curious" => $"Excellent! Let me provide you with detailed insights into {topic}. 🔍",
                "positive" => $"I'm thrilled to explore {topic} with you in depth! 🎯",
                _ => string.Empty
            };
        }

        /// <summary>
        /// GetEducationSentimentAwareness() - Returns sentiment-aware encouragement during full education offer.
        /// Combines sentiment detection with topic interest.
        /// </summary>
        private string GetEducationSentimentAwareness(string sentiment, string topic)
        {
            return sentiment switch
            {
                "worried" => $"Don't worry, {_userName}. The more you understand {topic}, the better equipped you'll be to protect yourself. 🛡️",
                "frustrated" => $"I know this might feel overwhelming, {_userName}, but once you understand {topic}, it becomes much clearer. 🌟",
                "curious" => $"Your curiosity is your strength, {_userName}. Let's explore {topic} thoroughly. 🧠",
                "positive" => $"Your positive attitude will help you master {topic}! Let's get into the details. 🎓",
                _ => string.Empty
            };
        }

        /// <summary>
        /// GetMemoryPersonalization() - Uses stored memory to personalize education responses (ISSUE 1).
        /// References user's interests and previously discussed topics for natural conversation.
        /// </summary>
        private string GetMemoryPersonalization(string currentTopic)
        {
            var interests = _userMemory.GetInterests();

            // If this is a favorite topic, acknowledge it with natural language
            if (_userMemory.HasFavoriteTopic() && _userMemory.GetFavoriteTopic().Equals(currentTopic, StringComparison.OrdinalIgnoreCase))
            {
                return $"Since {currentTopic} is a primary interest for you, let me give you comprehensive coverage. 🎯";
            }

            // If the current topic is in their interests list, acknowledge it
            if (_userMemory.IsInterested(currentTopic))
            {
                return $"I know you're interested in {currentTopic}—let's explore this thoroughly! 🚀";
            }

            // If we've discussed related topics, make a natural connection
            var discussedTopics = _userMemory.GetDiscussedTopics();
            if (discussedTopics.Count > 1)
            {
                string previousTopic = discussedTopics[discussedTopics.Count - 2];  // Get the one before current
                if (!previousTopic.Equals(currentTopic, StringComparison.OrdinalIgnoreCase))
                {
                    return $"Building on what you learned about {previousTopic}, {currentTopic} shares some important connections. 🔗";
                }
            }

            // If user has multiple interests, reference them
            if (interests.Count > 1)
            {
                return $"You've shown interest in {string.Join(" and ", interests)}, so this knowledge about {currentTopic} will complement your cybersecurity foundation. 💡";
            }

            return string.Empty;
        }

        /// <summary>
        /// GetInterestAcknowledgment() - Creates a natural acknowledgment of user's interests (ISSUE 1).
        /// Used to make responses feel personalized and attentive to user preferences.
        /// </summary>
        private string GetInterestAcknowledgment(string topic)
        {
            if (_userMemory.IsInterested(topic))
            {
                return $"Great! You're interested in {topic}—";
            }

            var interests = _userMemory.GetInterests();
            if (interests.Count > 0)
            {
                string interestsList = string.Join(", ", interests);
                return $"While your main interests are {interestsList}, learning about {topic} will also help protect you—";
            }

            return string.Empty;
        }

        /// <summary>
        /// RespondToGreeting() - Responds conversationally to greetings.
        /// Enhanced with user memory reference if available.
        /// Uses typing animation and color for a natural, engaging conversation feel.
        /// </summary>
        private void RespondToGreeting()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();
            UIHelper.DisplayBotMessage($"✨ I'm functioning perfectly, {_userName}! Thank you for asking.", ConsoleColor.Yellow);
            UIHelper.DisplayBotMessage("I'm here to help you learn about cybersecurity and stay safe online.", ConsoleColor.Yellow);
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// IsContinuationRequest() - Detects if user wants another tip from the same topic.
        /// Examples: "another tip", "another one", "more tips", "more advice"
        /// </summary>
        private bool IsContinuationRequest(string input)
        {
            return (input.Contains("another") || input.Contains("more") || input.Contains("one more")) &&
                   (input.Contains("tip") || input.Contains("advice") || input.Contains("tips") || input.Contains("one"));
        }

        /// <summary>
        /// IsFlexibleFollowUpRequest() - Improved detection for follow-up phrases (ISSUE 4).
        /// Supports natural conversation continuations like:
        /// - "tell me more"
        /// - "another one"
        /// - "what else?"
        /// - "continue"
        /// - "go on"
        /// - "more info"
        /// </summary>
        private bool IsFlexibleFollowUpRequest(string input)
        {
            var followUpPhrases = new List<string>
            {
                "tell me more",
                "another one",
                "another tip",
                "more tips",
                "what else",
                "anything else",
                "continue",
                "go on",
                "more info",
                "more information",
                "more details",
                "elaborate",
                "explain more",
                "keep going",
                "one more",
                "just one more",
                "give me another",
                "tell me another",
                "any more"
            };

            return followUpPhrases.Any(phrase => input.Contains(phrase));
        }

        /// <summary>
        /// IsDeepDiveRequest() - Detects requests to explore a topic more deeply (ISSUE 4).
        /// Supports phrases like:
        /// - "go deeper"
        /// - "deeper"
        /// - "tell me more details"
        /// - "elaborate"
        /// - "break it down"
        /// </summary>
        private bool IsDeepDiveRequest(string input)
        {
            var deepDivePhrases = new List<string>
            {
                "go deeper",
                "deeper",
                "more details",
                "break it down",
                "elaborate",
                "explain in detail",
                "detailed explanation",
                "take me through",
                "walk me through",
                "step by step",
                "more comprehensive"
            };

            return deepDivePhrases.Any(phrase => input.Contains(phrase));
        }

        /// <summary>
        /// DetectTopicFromInput() - Extracts topic from user input for context tracking (ISSUE 4).
        /// Useful for updating ConversationManager with the current topic being discussed.
        /// </summary>
        private string DetectTopicFromInput(string input)
        {
            return MapKeywordToTopic(input);
        }

        /// <summary>
        /// MapKeywordToTopic() - Extracts the topic from user input.
        /// Maps keywords like "phishing", "password", "2fa", etc. to their topic names.
        /// </summary>
        private string MapKeywordToTopic(string input)
        {
            // Define keyword-to-topic mappings
            var keywordMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "phishing", "phishing" },
                { "password", "passwords" },
                { "passwd", "passwords" },
                { "2fa", "2fa" },
                { "two-factor", "2fa" },
                { "two factor", "2fa" },
                { "authentication", "2fa" },
                { "auth", "2fa" },
                { "privacy", "privacy" },
                { "private", "privacy" },
                { "browsing", "browsing" },
                { "browser", "browsing" },
                { "ransomware", "ransomware" },
                { "ransom", "ransomware" },
                { "social engineering", "social engineering" },
                { "engineer", "social engineering" },
                { "patch", "patch management" },
                { "update", "patch management" },
                { "software", "patch management" },
                { "wifi", "wifi" },
                { "wi-fi", "wifi" },
                { "network", "wifi" },
                { "public", "wifi" },
                { "vpn", "wifi" },
                { "password manager", "password manager" },
                { "manager", "password manager" },
                { "vault", "password manager" }
            };

            // Search for keywords in the input
            foreach (var kvp in keywordMap)
            {
                if (input.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// HandleTipRequest() - Processes tip requests with enhanced tracking and flow (ISSUE 3).
        /// 
        /// Features:
        /// - Tracks used tips per topic using TipTracker
        /// - Prevents showing the same tip twice
        /// - Handles the 3-tip -> full education transition
        /// - Provides remaining tips after education
        /// - Prevents tip exhaustion loops
        /// - Sentiment-aware tone (ISSUE 2)
        /// </summary>
        private void HandleTipRequest(string input, string sentiment)
        {
            string requestedTopic = MapKeywordToTopic(input);

            // Edge case: User asked "another tip" but no topic was set
            if (IsFlexibleFollowUpRequest(input) && string.IsNullOrEmpty(_currentTopic))
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"I'd like to give you another tip, {_userName}, but I need to know which topic first.", ConsoleColor.Yellow);
                UIHelper.DisplayBotMessage("Which topic interests you? (e.g., phishing, passwords, 2fa, privacy, browsing, ransomware, social engineering, patch management, wifi, password manager)", ConsoleColor.Yellow);
                return;
            }

            // Use current topic if continuing, otherwise use requested topic
            string? activeTopic = (IsFlexibleFollowUpRequest(input) || IsContinuationRequest(input)) ? _currentTopic : requestedTopic;

            // Edge case: Unknown topic
            if (activeTopic == null)
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"I'm not sure which topic you're interested in, {_userName}.", ConsoleColor.Yellow);
                UIHelper.DisplayBotMessage("Try asking about: phishing, passwords, 2fa, or any other topics on the menu.", ConsoleColor.Yellow);
                return;
            }

            // Validate topic exists in repository
            if (!_tipRepository.HasTopic(activeTopic))
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"I don't have tips for '{activeTopic}' yet, {_userName}. Try another topic.", ConsoleColor.Yellow);
                return;
            }

            // Get all available tips for the topic
            var allTips = _tipRepository.GetAllTips(activeTopic);
            int totalTips = allTips?.Count ?? 0;

            // Check if all tips have been shown
            if (_tipTracker.HasAllTipsBeenShown(activeTopic, totalTips))
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"You've already learned all {totalTips} tips about {activeTopic}! 🎓", ConsoleColor.Green);
                UIHelper.DisplayBotMessage("Would you like to:", ConsoleColor.Green);
                UIHelper.DisplayBotMessage($"  • Explore a different topic (e.g., 'tell me about privacy')", ConsoleColor.White);
                UIHelper.DisplayBotMessage($"  • Get a comprehensive breakdown of {activeTopic} (e.g., 'full education on {activeTopic}')", ConsoleColor.White);
                return;
            }

            // Get the next unused tip from TipTracker
            string tip = _tipTracker.GetNextUnusedTip(activeTopic, allTips);

            if (tip != null)
            {
                Console.WriteLine();

                // ISSUE 2: Add sentiment-aware prefix
                string sentimentPrefix = GetSentimentAwarePrefix(sentiment, activeTopic);
                if (!string.IsNullOrEmpty(sentimentPrefix))
                {
                    UIHelper.DisplayBotMessage(sentimentPrefix, ConsoleColor.Green);
                }

                UIHelper.DisplayTypingIndicator();
                UIHelper.DisplayBotMessage(tip, ConsoleColor.Cyan);

                // Phase 1: Update conversation context after response
                _conversationManager.SetIntentType("TipRequest");
                _conversationManager.SetResponseCategory("Tip");
                _conversationManager.IncrementTipsShown();
                _conversationManager.UpdateTopic(activeTopic);

                // Update conversation state
                _currentTopic = activeTopic;
                _tipCount = _tipTracker.GetTipCount(activeTopic);

                // ISSUE 3: Enhanced tip flow logic
                int remainingTips = _tipTracker.GetRemainingTipCount(activeTopic, totalTips);

                if (_tipCount >= 3 && remainingTips > 0)
                {
                    // Offer full education after 3 tips
                    OfferFullEducationBeforeMoreTips(activeTopic, sentiment, remainingTips);
                }
                else if (remainingTips > 0)
                {
                    // Encourage more tips
                    Console.WriteLine();
                    int tipsUntilEducation = Math.Max(0, 3 - _tipCount);
                    if (tipsUntilEducation > 0)
                    {
                        UIHelper.DisplayBotMessage($"Want another tip about {activeTopic}? ({tipsUntilEducation} more tips before the full breakdown)", ConsoleColor.Green);
                    }
                    else
                    {
                        UIHelper.DisplayBotMessage($"You have {remainingTips} more tip(s) about {activeTopic}. Want another?", ConsoleColor.Green);
                    }
                }
                else
                {
                    // Last tip - offer comprehensive coverage
                    Console.WriteLine();
                    UIHelper.DisplayBotMessage($"That's the last tip I have for {activeTopic}! 🎯", ConsoleColor.Green);
                    UIHelper.DisplayBotMessage("Would you like a comprehensive breakdown of this topic or explore something else?", ConsoleColor.Green);
                }

                UIHelper.AutoScroll();
            }
            else
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"Sorry, I couldn't retrieve a tip for {activeTopic} right now.", ConsoleColor.Yellow);
            }
        }

        /// <summary>
        /// OfferFullEducationBeforeMoreTips() - Offers comprehensive education after initial tips (ISSUE 3).
        /// Provides an enhanced transition after 3 tips.
        /// </summary>
        private void OfferFullEducationBeforeMoreTips(string topic, string sentiment, int remainingTips)
        {
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"You've collected {_tipCount} great tips about {topic}! 💡", ConsoleColor.White);
            Console.WriteLine();

            // ISSUE 2: Add sentiment-aware encouragement
            string sentimentAwareness = GetEducationSentimentAwareness(sentiment, topic);
            if (!string.IsNullOrEmpty(sentimentAwareness))
            {
                UIHelper.DisplayBotMessage(sentimentAwareness, ConsoleColor.Green);
                Console.WriteLine();
            }

            UIHelper.DisplayBotMessage("I can give you a more detailed breakdown so you really master this topic.", ConsoleColor.White);
            UIHelper.DisplayBotMessage($"After that, there will be {remainingTips} more tip(s) to explore if you want them!", ConsoleColor.White);
            Console.WriteLine();
            UIHelper.DisplayBotMessage("Would you like the comprehensive breakdown now? (yes/no)", ConsoleColor.Green);

            // Get user response (simplified - in full implementation, might loop until valid response)
            string? response = Console.ReadLine();
            if (response != null && (response.ToLower().Contains("yes") || response.ToLower().Contains("y")))
            {
                // Provide full education
                ProvideEducationWithSentiment(topic, sentiment);
            }
            else
            {
                Console.WriteLine();
                UIHelper.DisplayBotMessage($"No problem! Feel free to ask for more tips about {topic} whenever you're ready. 🚀", ConsoleColor.Green);
            }
        }

        /// <summary>
        /// ProvideDeepDiveEducation() - Provides deeper insights into a topic (ISSUE 4).
        /// Called when user requests deeper exploration with phrases like "go deeper".
        /// </summary>
        private void ProvideDeepDiveEducation(string topic, string sentiment)
        {
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"Let me dive deeper into {topic} for you, {_userName}. 🔍", ConsoleColor.Green);
            Console.WriteLine();

            // Phase 1: Mark this as a deep-dive/education mode in context
            _conversationManager.SetIntentType("DeepDiveEducation");
            var context = _conversationManager.GetContext();
            if (context != null)
            {
                context.IsInEducationMode = true;
                context.EducationDepth = 3; // Comprehensive depth
            }

            // Use the standard education but with extra emphasis on depth
            ProvideEducationWithSentiment(topic, sentiment);
        }

        /// <summary>
        /// OfferFullEducation() - Offers comprehensive education after tips (legacy method - deprecated).
        /// This has been replaced by OfferFullEducationBeforeMoreTips() for better flow.
        /// Kept for backward compatibility.
        /// </summary>
        private void OfferFullEducation(string topic, string sentiment)
        {
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"You seem really interested in {topic}, {_userName}! 💡", ConsoleColor.White);
            Console.WriteLine();

            // ISSUE 2: Add sentiment-aware encouragement
            string sentimentAwareness = GetEducationSentimentAwareness(sentiment, topic);
            if (!string.IsNullOrEmpty(sentimentAwareness))
            {
                UIHelper.DisplayBotMessage(sentimentAwareness, ConsoleColor.Green);
                Console.WriteLine();
            }

            UIHelper.DisplayBotMessage("Let me give you a more detailed breakdown of this topic so you can really master it.", ConsoleColor.White);
            Console.WriteLine();

            // Call the appropriate full education method based on topic
            switch (topic.ToLower())
            {
                case "phishing":
                    ProvidePhishingEducation();
                    break;
                case "passwords":
                    ProvidePasswordEducation();
                    break;
                case "2fa":
                    Provide2FAEducation();
                    break;
                case "privacy":
                    ProvidePrivacyEducation();
                    break;
                case "browsing":
                    ProvideBrowsingEducation();
                    break;
                case "ransomware":
                    ProvideRansomwareEducation();
                    break;
                case "social engineering":
                    ProvideSocialEngineeringEducation();
                    break;
                case "patch management":
                    ProvidePatchManagementEducation();
                    break;
                case "wifi":
                    ProvidePublicWifiEducation();
                    break;
                case "password manager":
                    ProvidePasswordManagerEducation();
                    break;
            }

            // Reset conversation state after full education
            ResetConversationState();
        }

        /// <summary>
        /// ResetConversationState() - Resets the conversation state for a new topic.
        /// Called when switching topics or after providing full education.
        /// </summary>
        private void ResetConversationState()
        {
            _currentTopic = null;
            _tipCount = 0;
        }

        /// <summary>
        /// RespondToPurpose() - Explains the chatbot's purpose when asked.
        /// Uses typing animation and color for a natural response.
        /// </summary>
        private void RespondToPurpose()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();
            UIHelper.DisplayBotMessage($"🎯 My purpose, {_userName}, is to educate you about cybersecurity best practices.", ConsoleColor.Yellow);
            UIHelper.DisplayBotMessage("I can teach you about phishing, passwords, two-factor authentication, data privacy, and more.", ConsoleColor.Yellow);
            UIHelper.DisplayBotMessage("Together, we'll help protect South African citizens online!", ConsoleColor.Yellow);
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// DisplayHelpMessage() - Shows available topics and commands with typing effect.
        /// Uses color-coded formatting and typing animation for consistent user experience.
        /// </summary>
        private void DisplayHelpMessage()
        {
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"📚 Here's how I can help, {_userName}:", ConsoleColor.Green);
            Console.WriteLine();
            UIHelper.DisplayBotMessage("🔒 Cybersecurity Topics:", ConsoleColor.Cyan);
            UIHelper.DisplayBotMessage("   • Type '1' or 'phishing' - Learn about phishing attacks", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '2' or 'password' - Learn about strong passwords", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '3' or '2fa' - Learn about two-factor authentication", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '4' or 'privacy' - Learn about data privacy", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '5' or 'browsing' - Learn about secure browsing", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '6' or 'ransomware' - Learn about ransomware", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '7' or 'social engineering' - Learn about social engineering", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '8' or 'patch management' - Learn about software updates", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '9' or 'public wi-fi' - Learn about public WiFi safety", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type '10' or 'password manager' - Learn about password managers", ConsoleColor.White);

            Console.WriteLine();
            UIHelper.DisplayBotMessage("💬 Conversational:", ConsoleColor.Cyan);
            UIHelper.DisplayBotMessage("   • Ask 'How are you?' or 'What's your purpose?'", ConsoleColor.White);
            UIHelper.DisplayBotMessage("   • Type 'exit' or 'quit' to leave", ConsoleColor.White);
        }

        /// <summary>
        /// DisplayWelcomeMessage() - Displays initial welcome message with typing effect.
        /// Sets the tone for the interactive session with consistent bot response styling.
        /// </summary>
        private void DisplayWelcomeMessage()
        {
            Console.WriteLine();
            UIHelper.DisplayBotMessage($"🎓 Welcome to the Cybersecurity Awareness Bot, {_userName}!", ConsoleColor.Green);
            UIHelper.DisplayBotMessage("Learn about important security practices to protect yourself and others online.", ConsoleColor.Green);
            Console.WriteLine();

            // Try to load audio greeting if available
            TryPlayAudioGreeting();
        }

        /// <summary>
        /// TryPlayAudioGreeting() - Attempts to load and play greeting audio from Audio folder.
        /// If the audio file doesn't exist, the chatbot continues without it.
        /// </summary>
        private void TryPlayAudioGreeting()
        {
            try
            {
                string greetingFile = Path.Combine(_audioPath, "greeting.wav");
                if (File.Exists(greetingFile))
                {
                    Console.WriteLine("🔊 [Audio greeting available - Audio playback feature coming soon]");
                }
            }
            catch
            {
                // Silent failure - audio is optional
            }
        }

        /// <summary>
        /// ProvidePhishingEducation() - Educates about phishing attacks with personalized greeting.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePhishingEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] phishingContent = new string[]
            {
                "┌────────────── PHISHING ATTACKS EDUCATION ──────────────┐",
                $"│ User: {_userName.PadRight(48)} │",
                "├────────────────────────────────────────────────────────┤",
                "│ Phishing is a social engineering attack where attackers│",
                "│ impersonate trusted entities to steal sensitive info.  │",
                "│                                                        │",
                "│ RED FLAGS:                                             │",
                "│ • Urgent requests for passwords or personal info       │",
                "│ • Suspicious sender email addresses                    │",
                "│ • Links that don't match the displayed text            │",
                "│ • Grammar and spelling errors                          │",
                "│                                                        │",
                "│ PREVENTION:                                            │",
                "│ • Never click links in unexpected emails               │",
                "│ • Verify sender identity independently                 │",
                "│ • Use email filtering and anti-phishing tools          │",
                "│ • Report suspicious emails to your IT department       │",
                "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in phishingContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvidePasswordEducation() - Educates about creating strong passwords with personalized greeting.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePasswordEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] passwordContent = new string[]
            {
                "┌────────── STRONG PASSWORD EDUCATION ───────────────────┐",
                $"│ User: {_userName.PadRight(48)} │",
                "├────────────────────────────────────────────────────────┤",
                "│ A strong password is your first defense against        │",
                "│ unauthorized access to your accounts.                  │",
                "│                                                        │",
                "│ STRONG PASSWORD CRITERIA:                              │",
                "│ • At least 12 characters long                          │",
                "│ • Mix of uppercase and lowercase letters               │",
                "│ • Include numbers and special characters (!@#$%^&*)    │",
                "│ • Avoid common words, dates, and personal info         │",
                "│ • Use unique passwords for each account                │",
                "│                                                        │",
                "│ BEST PRACTICES:                                        │",
                "│ • Use a password manager to securely store passwords   │",
                "│ • Change passwords if compromised                      │",
                "│ • Never share your password with anyone                │",
                "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in passwordContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// Provide2FAEducation() - Educates about two-factor authentication with personalized greeting.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void Provide2FAEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] twoFAContent = new string[]
            {
                "┌─────── TWO-FACTOR AUTHENTICATION (2FA) ────────────────┐",
                $"│ User: {_userName.PadRight(48)} │",
                "├────────────────────────────────────────────────────────┤",
                "│ 2FA adds an extra layer of security by requiring       │",
                "│ a second verification method in addition to password.  │",
                "│                                                        │",
                "│ COMMON 2FA METHODS:                                    │",
                "│ • SMS codes sent to your phone                         │",
                "│ • Authenticator apps (Google Authenticator, etc.)      │",
                "│ • Hardware security keys (FIDO2)                       │",
                "│ • Biometric verification (fingerprint, face)           │",
                "│                                                        │",
                "│ RECOMMENDATIONS:                                       │",
                "│ • Enable 2FA on all critical accounts                  │",
                "│ • Prefer authenticator apps over SMS                   │",
                "│ • Store backup codes in a secure location              │",
                "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in twoFAContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvidePrivacyEducation() - Educates about data privacy with personalized greeting.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePrivacyEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] privacyContent = new string[]
            {
                "┌──────────── DATA PRIVACY EDUCATION ────────────────────┐",
                $"│ User: {_userName.PadRight(48)} │",
                "├────────────────────────────────────────────────────────┤",
                "│ Your personal data is valuable. Protect it by being    │",
                "│ mindful of what information you share online.          │",
                "│                                                        │",
                "│ PRIVACY RISKS:                                         │",
                "│ • Identity theft                                       │",
                "│ • Data breaches                                        │",
                "│ • Targeted advertisements and manipulation             │",
                "│ • Financial fraud                                      │",
                "│                                                        │",
                "│ PROTECTION MEASURES:                                   │",
                "│ • Review privacy settings on social media accounts     │",
                "│ • Limit the information you share publicly             │",
                "│ • Use VPNs on public Wi-Fi networks                    │",
                "│ • Regularly check your credit reports                  │",
                "│ • Read privacy policies before using new services      │",
                "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in privacyContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvideBrowsingEducation() - Educates about secure browsing with personalized greeting.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvideBrowsingEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] browsingContent = new string[]
            {
                "┌────────── SECURE BROWSING EDUCATION ───────────────────┐",
                $"│ User: {_userName.PadRight(48)} │",
                "├────────────────────────────────────────────────────────┤",
                "│ Safe browsing habits protect you from malware,         │",
                "│ phishing, and other online threats.                    │",
                "│                                                        │",
                "│ SECURE BROWSING PRACTICES:                             │",
                "│ • Look for HTTPS in the URL (padlock icon)             │",
                "│ • Keep your browser and extensions updated             │",
                "│ • Use reputable antivirus and anti-malware software    │",
                "│ • Disable plugins you don't actively use               │",
                "│ • Be cautious with file downloads                      │",
                "│                                                        │",
                "│ ADVANCED MEASURES:                                     │",
                "│ • Use browser privacy modes for sensitive activities   │",
                "│ • Install browser extensions for tracking prevention   │",
                "│ • Use DNS filtering to block malicious sites           │",
                "│ • Consider using a privacy-focused browser             │",
                "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in browsingContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// ADDITIONAL TOPIC HANDLERS HERE
        /// THESE WERE ADDED LATER TO EXPAND THE CHATBOT'S EDUCATIONAL CONTENT

        /// <summary>
        /// /// ProvideRansomwareEducation() - Educates about ransomware risks and backup strategies.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>

        private void Null()
        {

        }
        private void ProvideRansomwareEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] ransomwareContent = new string[]
            {
        "┌───────── RANSOMWARE & BACKUP STRATEGIES ───────────────┐",
        $"│ User: {_userName.PadRight(48)} │",
        "├────────────────────────────────────────────────────────┤",
        "│ Ransomware is malware that encrypts your files         │",
        "│ and demands payment for decryption.                    │",
        "│                                                        │",
        "│ HOW IT SPREADS:                                        │",
        "│ • Malicious email attachments                          │",
        "│ • Compromised websites or drive-by downloads           │",
        "│ • Remote Desktop Protocol (RDP) attacks                │",
        "│                                                        │",
        "│ PROTECTION STRATEGIES:                                 │",
        "│ • Regular offline/cloud backups                        │",
        "│ • Keep software and antivirus updated                  │",
        "│ • Never pay the ransom (no guarantee of recovery)      │",
        "│ • Use application whitelisting                         │",
        "│                                                        │",
        "│ BEST BACKUP PRACTICE (3-2-1 Rule):                     │",
        "│ • 3 copies of your data                                │",
        "│ • 2 different storage media                            │",
        "│ • 1 copy stored off-site                               │",
        "└────────────────────────────────────────────────────────┘"
    };

            foreach (var line in ransomwareContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvideSocialEngineeringEducation() - Educates about social engineering attacks beyond phishing.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvideSocialEngineeringEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] socialEngineeringContent = new string[]
            {
        "┌─────── SOCIAL ENGINEERING (BEYOND PHISHING) ────────────┐",
        $"│ User: {_userName.PadRight(48)}  │",
        "├─────────────────────────────────────────────────────────┤",
        "│ Social engineering manipulates people into giving       │",
        "│ up confidential information or access.                  │",
        "│                                                         │",
        "│ COMMON TACTICS:                                         │",
        "│ • Vishing (voice calls pretending to be IT/support)     │",
        "│ • Smishing (fraudulent SMS messages)                    │",
        "│ • Pretexting (creating a false scenario)                │",
        "│ • Baiting (leaving infected USB drives)                 │",
        "│ • Tailgating (following someone into secure area)       │",
        "│                                                         │",
        "│ HOW TO STAY SAFE:                                       │",
        "│ • Verify unexpected requests via another channel        │",
        "│ • Never share OTPs or passwords over phone/email        │",
        "│ • Be skeptical of urgent or threatening language        │",
        "│ • Report suspicious contact to security team            │",
        "└─────────────────────────────────────────────────────────┘"
            };

            foreach (var line in socialEngineeringContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvidePatchManagementEducation() - Educates about software updates and patch management.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePatchManagementEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] patchContent = new string[]
            {
        "┌───── SOFTWARE UPDATES & PATCH MANAGEMENT ──────────────┐",
        $"│ User: {_userName.PadRight(48)} │",
        "├────────────────────────────────────────────────────────┤",
        "│ Updates fix security vulnerabilities that              │",
        "│ attackers exploit to compromise systems.               │",
        "│                                                        │",
        "│ WHY UPDATES MATTER:                                    │",
        "│ • WannaCry ransomware spread via unpatched             │",
        "│   Windows SMB vulnerability (EternalBlue)              │",
        "│ • Zero‑day exploits are patched after discovery        │",
        "│ • Outdated software is a top attack vector             │",
        "│                                                        │",
        "│ BEST PRACTICES:                                        │",
        "│ • Enable automatic updates where possible              │",
        "│ • Regularly update OS, browsers, and plugins           │",
        "│ • Don't ignore update reminders                        │",
        "│ • Remove unsupported software (e.g., Win 7)            │",
        "│ • Use a vulnerability scanner for businesses           │",
        "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in patchContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvidePublicWifiEducation() - Educates about public Wi-Fi risks and VPNs.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePublicWifiEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] wifiContent = new string[]
            {
        "┌────────── PUBLIC WI-FI RISKS & VPNS ───────────────────┐",
        $"│ User: {_userName.PadRight(48)} │",
        "├────────────────────────────────────────────────────────┤",
        "│ Public Wi‑Fi networks are often insecure and           │",
        "│ expose your data to attackers.                         │",
        "│                                                        │",
        "│ COMMON ATTACKS:                                        │",
        "│ • Man‑in‑the‑middle (intercepting traffic)             │",
        "│ • Evil twin (rogue hotspot pretending to be            │",
        "│   legitimate)                                          │",
        "│ • Packet sniffing (capturing unencrypted data)         │",
        "│                                                        │",
        "│ PROTECTION TIPS:                                       │",
        "│ • Use a VPN (Virtual Private Network) to               │",
        "│   encrypt all traffic                                  │",
        "│ • Stick to HTTPS websites (look for padlock)           │",
        "│ • Disable auto‑connect to Wi‑Fi                        │",
        "│ • Use mobile hotspot instead of public Wi‑Fi           │",
        "│ • Turn off file sharing and AirDrop                    │",
        "└────────────────────────────────────────────────────────┘"
            };

            foreach (var line in wifiContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// ProvidePasswordManagerEducation() - Educates about using password managers.
        /// Uses typing animation and color to display educational content in an engaging way.
        /// </summary>
        private void ProvidePasswordManagerEducation()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();

            string[] managerContent = new string[]
            {
        "┌─────────────── PASSWORD MANAGERS ─────────────────────┐",
        $"│ User: {_userName.PadRight(48)}│",
        "├───────────────────────────────────────────────────────┤",
        "│ Password managers generate, store, and autofill       │",
        "│ strong unique passwords for all your accounts.        │",
        "│                                                       │",
        "│ WHY USE ONE:                                          │",
        "│ • Remember only one master password                   │",
        "│ • Create long, random passwords (e.g.,                │",
        "│   &92#kLp$8mNq@4) easily                              │",
        "│ • Eliminates password reuse across sites              │",
        "│ • Protects against keyloggers (auto‑fill)             │",
        "│                                                       │",
        "│ RECOMMENDATIONS:                                      │",
        "│ • Choose reputable managers (Bitwarden,               │",
        "│    1Password, KeePass, Apple/Google built‑in)         │",
        "│ • Use a strong, memorable master password             │",
        "│ • Enable 2FA on your password manager                 │",
        "│ • Never store master password digitally               │",
        "│ • Regularly back up the encrypted vault               │",
        "└───────────────────────────────────────────────────────┘"
            };

            foreach (var line in managerContent)
            {
                UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
            }
            UIHelper.AutoScroll();
        }

        ///ADDIONAL TOPIC HANDLERS END
        /// These additional handlers can be called from HandleUserInput() by adding new cases for keywords like "ransomware", "social engineering", "patch management", "public wifi", and "password manager".

        /// <summary>
        /// ProcessMessage - Public method for GUI integration.
        /// Processes user input and returns a response string without printing to console.
        /// 
        /// This method:
        /// - Analyzes sentiment of the input
        /// - Checks memory and interests
        /// - Routes to appropriate topic handler
        /// - Returns response as a string (ideal for GUI)
        /// </summary>
        public string ProcessMessage(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return $"I didn't catch that, {_userName}. Please enter something or type 'help' for options.";

            string lowerInput = userInput.Trim().ToLower();

            // Sentiment analysis
            string sentiment = _sentimentAnalyzer.DetectSentiment(lowerInput);

            // Check for interest statements
            CheckAndStoreInterests(userInput);

            // Check for exit command
            if (lowerInput == "0" || lowerInput == "exit" || lowerInput == "quit" || lowerInput == "bye")
            {
                return $"Take care, {_userName}! Remember to stay secure online! 🔒";
            }

            // Check for help command
            if (lowerInput == "help")
            {
                return GetHelpResponse();
            }

            // Check for tip requests
            if (IsTipRequest(lowerInput))
            {
                return GenerateTipResponse(lowerInput, sentiment);
            }

            // Check for continuation requests
            if (IsContinuationRequest(lowerInput) && !string.IsNullOrEmpty(_currentTopic))
            {
                return GenerateTipResponse(lowerInput, sentiment);
            }

            // Route to topic-specific responses
            return RouteToTopicResponse(lowerInput, sentiment);
        }

        private string GetHelpResponse()
        {
            return $"Hi {_userName}! I'm here to teach you about cybersecurity. You can:\n" +
                   "• Ask about topics: phishing, passwords, 2FA, privacy, ransomware, etc.\n" +
                   "• Type numbers 1-10 for specific topics\n" +
                   "• Ask for tips on any security topic\n" +
                   "• Say 'exit' to quit\n" +
                   "What would you like to learn about?";
        }

        private string RouteToTopicResponse(string lowerInput, string sentiment)
        {
            // Route to appropriate topic handler
            switch (lowerInput)
            {
                case "1":
                case "phishing":
                case "phishing attacks":
                    return GetTopicResponse("phishing");
                case "2":
                case "password":
                case "passwords":
                    return GetTopicResponse("passwords");
                case "3":
                case "2fa":
                case "two-factor":
                case "two factor":
                case "authentication":
                    return GetTopicResponse("2fa");
                case "4":
                case "privacy":
                case "data privacy":
                    return GetTopicResponse("privacy");
                case "5":
                case "browsing":
                case "secure browsing":
                    return GetTopicResponse("browsing");
                case "6":
                case "ransomware":
                    return GetTopicResponse("ransomware");
                case "7":
                case "social engineering":
                case "social engineer":
                    return GetTopicResponse("social_engineering");
                case "8":
                case "patch":
                case "patch management":
                case "update":
                case "updates":
                    return GetTopicResponse("patch_management");
                case "9":
                case "wifi":
                case "public wifi":
                case "public wi-fi":
                    return GetTopicResponse("wifi");
                case "10":
                case "password manager":
                case "password managers":
                    return GetTopicResponse("password_manager");

                // General conversational queries
                case "hello":
                case "hi":
                case "hey":
                    return $"Hello {_userName}! I'm here to help you learn about cybersecurity. What topic interests you?";
                case "how are you?":
                case "how are you":
                case "what's your purpose?":
                case "what's your purpose":
                    return $"I'm doing great! My purpose is to teach you about cybersecurity awareness and best practices. What would you like to know?";
                default:
                    return $"That's interesting! Try asking me about: phishing, passwords, 2FA, privacy, ransomware, or type 'help' for more options.";
            }
        }

        private string GetTopicResponse(string topic)
        {
            _userMemory.AddDiscussedTopic(topic);
            _currentTopic = topic;

            return topic switch
            {
                "phishing" => "PHISHING ATTACKS\n\n" +
                    "Phishing is a type of social engineering attack where attackers impersonate legitimate organizations to steal credentials or data.\n\n" +
                    "KEY RISKS:\n" +
                    "• Email spoofing - emails appear to come from trusted sources\n" +
                    "• Credential theft - tricking you into entering your password\n" +
                    "• Malware delivery - links containing malicious software\n\n" +
                    "PROTECTION TIPS:\n" +
                    "✓ Check sender email address carefully\n" +
                    "✓ Look for spelling errors or urgent language\n" +
                    "✓ Never click links from unknown senders\n" +
                    "✓ Verify requests by contacting the organization directly\n" +
                    "✓ Use email filtering and security tools\n" +
                    "✓ Enable multi-factor authentication",

                "passwords" => "STRONG PASSWORDS\n\n" +
                    "Strong passwords are your first line of defense against unauthorized access.\n\n" +
                    "WHAT MAKES A PASSWORD STRONG:\n" +
                    "• At least 12-16 characters (longer is better)\n" +
                    "• Mix of uppercase and lowercase letters\n" +
                    "• Include numbers and special characters (!@#$%^&*)\n" +
                    "• Avoid dictionary words, names, or birthdates\n" +
                    "• Unique for each account\n\n" +
                    "BEST PRACTICES:\n" +
                    "✓ Use a password manager (1Password, KeePass, etc.)\n" +
                    "✓ Enable two-factor authentication\n" +
                    "✓ Change passwords if breached\n" +
                    "✓ Never share your password\n" +
                    "✓ Use passphrases for accounts you use frequently",

                "2fa" => "TWO-FACTOR AUTHENTICATION (2FA)\n\n" +
                    "2FA requires two forms of verification to access your account, making it much harder for attackers to gain access.\n\n" +
                    "HOW IT WORKS:\n" +
                    "1. You know (password)\n" +
                    "2. You have (phone, authenticator app, security key)\n\n" +
                    "TYPES OF 2FA:\n" +
                    "• SMS/Text messages (least secure)\n" +
                    "• Authenticator apps (Google Authenticator, Microsoft Authenticator)\n" +
                    "• Hardware security keys (YubiKey, most secure)\n" +
                    "• Biometric (fingerprint, face recognition)\n\n" +
                    "BENEFITS:\n" +
                    "✓ Dramatically increases security\n" +
                    "✓ Protects against phishing\n" +
                    "✓ Prevents unauthorized access even if password is stolen",

                "privacy" => "DATA PRIVACY\n\n" +
                    "Protecting your personal data is crucial in the digital age.\n\n" +
                    "TYPES OF DATA TO PROTECT:\n" +
                    "• Personal identifiers (name, SSN, DOB)\n" +
                    "• Financial information (bank accounts, credit cards)\n" +
                    "• Health records\n" +
                    "• Online activity and browsing history\n\n" +
                    "PRIVACY TIPS:\n" +
                    "✓ Review privacy settings on social media\n" +
                    "✓ Limit information you share online\n" +
                    "✓ Use privacy-focused browsers (Firefox, Brave)\n" +
                    "✓ Use VPNs on public Wi-Fi\n" +
                    "✓ Check data breaches at haveibeenpwned.com\n" +
                    "✓ Read privacy policies before sharing data",

                "browsing" => "SECURE BROWSING\n\n" +
                    "Safe web browsing practices protect you from malware, phishing, and data theft.\n\n" +
                    "ESSENTIAL PRACTICES:\n" +
                    "• Look for HTTPS and padlock icon\n" +
                    "• Keep browser and extensions updated\n" +
                    "• Use reputable antivirus software\n" +
                    "• Be cautious with downloads\n\n" +
                    "AVOID:\n" +
                    "✗ Clicking suspicious links\n" +
                    "✗ Downloading from untrusted sites\n" +
                    "✗ Ignoring security warnings\n" +
                    "✗ Installing unknown browser extensions\n\n" +
                    "RECOMMENDED:\n" +
                    "✓ Use browser security extensions\n" +
                    "✓ Clear cookies and cache regularly\n" +
                    "✓ Disable JavaScript on untrusted sites\n" +
                    "✓ Use private/incognito browsing mode",

                "ransomware" => "RANSOMWARE\n\n" +
                    "Ransomware encrypts your files and demands payment for decryption. Prevention is critical.\n\n" +
                    "COMMON DELIVERY METHODS:\n" +
                    "• Malicious email attachments\n" +
                    "• Compromised websites\n" +
                    "• Unpatched software vulnerabilities\n" +
                    "• Remote desktop access (RDP)\n\n" +
                    "PROTECTION STRATEGIES:\n" +
                    "✓ Keep backups offline and regularly updated\n" +
                    "✓ Update software and OS immediately\n" +
                    "✓ Use multi-factor authentication\n" +
                    "✓ Train staff on phishing awareness\n" +
                    "✓ Use reputable security software\n" +
                    "✓ Restrict file sharing permissions",

                "social_engineering" => "SOCIAL ENGINEERING\n\n" +
                    "Social engineering exploits human psychology to manipulate people into divulging confidential information.\n\n" +
                    "COMMON TACTICS:\n" +
                    "• Pretexting - creating a fabricated scenario\n" +
                    "• Baiting - offering something enticing\n" +
                    "• Tailgating - following someone into secure areas\n" +
                    "• Phishing - fraudulent emails\n" +
                    "• Vishing - voice phishing over phone\n\n" +
                    "DEFENSE MEASURES:\n" +
                    "✓ Verify identities before sharing information\n" +
                    "✓ Be skeptical of unsolicited requests\n" +
                    "✓ Don't trust caller ID alone\n" +
                    "✓ Follow organizational security policies\n" +
                    "✓ Report suspicious activity\n" +
                    "✓ Stay educated on tactics",

                "patch_management" => "SOFTWARE UPDATES & PATCH MANAGEMENT\n\n" +
                    "Updates fix security vulnerabilities that attackers exploit to compromise systems.\n\n" +
                    "WHY UPDATES MATTER:\n" +
                    "• Fix security vulnerabilities\n" +
                    "• Prevent zero-day exploits\n" +
                    "• Improve stability and performance\n" +
                    "• Protect against known malware\n\n" +
                    "BEST PRACTICES:\n" +
                    "✓ Enable automatic updates\n" +
                    "✓ Update OS regularly\n" +
                    "✓ Update browsers and plugins\n" +
                    "✓ Remove unsupported software\n" +
                    "✓ Test updates on non-critical systems first\n" +
                    "✓ Schedule updates during low-usage times",

                "wifi" => "PUBLIC WI-FI SAFETY\n\n" +
                    "Public Wi-Fi networks pose significant security risks due to lack of encryption and authentication.\n\n" +
                    "RISKS:\n" +
                    "• Man-in-the-middle attacks\n" +
                    "• Packet sniffing - intercepting data\n" +
                    "• Rogue hotspots - fake Wi-Fi networks\n" +
                    "• Malware distribution\n\n" +
                    "PROTECTION METHODS:\n" +
                    "✓ Use a VPN (Virtual Private Network)\n" +
                    "✓ Avoid sensitive transactions on public Wi-Fi\n" +
                    "✓ Disable file sharing\n" +
                    "✓ Turn off auto-connect features\n" +
                    "✓ Use HTTPS websites only\n" +
                    "✓ Use your phone's hotspot instead",

                "password_manager" => "PASSWORD MANAGERS\n\n" +
                    "Password managers securely store and generate strong passwords for all your accounts.\n\n" +
                    "HOW THEY HELP:\n" +
                    "• Generate strong, unique passwords\n" +
                    "• Eliminate password reuse\n" +
                    "• Auto-fill login information securely\n" +
                    "• Sync across devices\n\n" +
                    "POPULAR OPTIONS:\n" +
                    "• Bitwarden (free, open-source)\n" +
                    "• 1Password (premium, user-friendly)\n" +
                    "• KeePass (local, self-hosted)\n" +
                    "• Apple/Google built-in managers\n\n" +
                    "BEST PRACTICES:\n" +
                    "✓ Use a strong master password\n" +
                    "✓ Enable 2FA on your password manager\n" +
                    "✓ Never store master password digitally\n" +
                    "✓ Regularly backup encrypted vault",

                _ => $"I can help you with {topic}. Would you like more details?"
            };
        }

        private string GenerateTipResponse(string userInput, string sentiment)
        {
            // Use current topic or pick a random one
            string topic = _currentTopic ?? "phishing";

            // Get a random tip for the topic
            string tip = _tipRepository.GetRandomTip(topic);

            string tipMessage = $"💡 TIP: {tip}";
            if (sentiment != "neutral")
            {
                tipMessage = (sentiment switch
                {
                    "worried" => $"I understand this might feel overwhelming, but here's a helpful tip:\n\n",
                    "frustrated" => $"Don't worry, security doesn't have to be complicated. Here's a tip:\n\n",
                    "curious" => $"Great question! Here's an interesting tip:\n\n",
                    "positive" => $"Glad to see your enthusiasm! Here's a tip:\n\n",
                    _ => ""
                }) + tipMessage;
            }

            return tipMessage;
        }

    }
}
