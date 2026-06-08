using CyberSecurityAwarenessBot;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CyberSecurityAwarenessBot.Helpers;
using CyberSecurityAwarenessBot.Services;
using CyberSecurityAwarenessBot.Models;

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

        // Conversation state tracking
        private string? _currentTopic;
        private int _tipCount;
        // Conversation state for breakdown confirmation
        private bool _awaitingBreakdownConfirmation;
        private string? _pendingTopic;
        private string? _lastIntent;
        private string? _lastUserInput;

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

        // Activity log services
        private readonly ActivityLogService _activityLogService;
        private int _activityLogPage;
        private bool _activityLogMode;

        // Task assistant services
        private readonly TaskService _taskService;
        private int? _pendingReminderTaskId;
        private bool _awaitingReminderResponse;
        private bool _awaitingTaskDescription;
        private string? _pendingTaskTitle;

        // Quiz Manager for quiz functionality
        private QuizManager _quizManager;

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

            // Initialize task assistant service
            _taskService = new TaskService();
            _pendingReminderTaskId = null;
            _awaitingReminderResponse = false;
            _awaitingTaskDescription = false;
            _pendingTaskTitle = null;

            // Initialize activity logging service
            _activityLogService = new ActivityLogService();
            _activityLogPage = 0;
            _activityLogMode = false;

            // Initialize quiz manager
            _quizManager = new QuizManager();
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

            // Activity log command support
            if (IsShowActivityLogCommand(lowerInput))
            {
                _activityLogPage = 1;
                _activityLogMode = true;
                _activityLogService.AddEntry("ActivityLog", "Viewed activity log.");
                string pageResponse = GetActivityLogPageResponse(_activityLogPage);
                Console.WriteLine();
                UIHelper.DisplayBotMessage(pageResponse, ConsoleColor.Green);
                return;
            }

            if (_activityLogMode && IsShowMoreActivityLogCommand(lowerInput))
            {
                _activityLogPage++;
                _activityLogService.AddEntry("ActivityLog", $"Viewed more activity log (page {_activityLogPage}).");
                string pageResponse = GetActivityLogPageResponse(_activityLogPage);
                Console.WriteLine();
                UIHelper.DisplayBotMessage(pageResponse, ConsoleColor.Green);
                return;
            }

            // Reset activity log pagination when the user leaves log mode
            if (_activityLogMode)
            {
                ResetActivityLogPagination();
            }

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
        /// Examples: "give me a phishing tip", "password tip", "any 2fa advice", "a phishing tip", "tips on privacy"
        /// Enhanced to catch more natural language patterns without being too restrictive.
        /// </summary>
        private bool IsTipRequest(string input)
        {
            // Check if input contains "tip" or "advice"
            bool hasTipKeyword = input.Contains("tip") || input.Contains("advice");

            if (!hasTipKeyword)
                return false;

            // Pattern 1: Contains action words (give, tell, show, any, want, need, etc.)
            bool hasActionWord = input.Contains("give") || input.Contains("tell") || input.Contains("show") ||
                                input.Contains("any") || input.Contains("want") || input.Contains("need") ||
                                input.Contains("about") || input.Contains("on") || input.Contains("for");

            if (hasActionWord)
                return true;

            // Pattern 2: If we can detect a topic keyword, it's likely a tip request
            // e.g., "A phishing tip", "Password tips", "Privacy advice"
            string detectedTopic = MapKeywordToTopic(input);
            if (detectedTopic != null)
                return true;

            return false;
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
            _activityLogService.AddEntry("Topic", $"Started education on topic '{topic}'.");
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
                _activityLogService.AddEntry("Tip", $"Delivered tip for {activeTopic}.");
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

                if (remainingTips > 0)
                {
                    Console.WriteLine();
                    UIHelper.DisplayBotMessage($"You have {remainingTips} more tip(s) about {activeTopic}. Ask for another tip whenever you're ready.", ConsoleColor.Green);
                }
                else
                {
                    Console.WriteLine();
                    UIHelper.DisplayBotMessage($"You've now received all available tips for {activeTopic}! 🎯", ConsoleColor.Green);
                    UIHelper.DisplayBotMessage($"Try exploring another cybersecurity topic or ask for the full {activeTopic} overview.", ConsoleColor.Green);
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
        /// OfferFullEducationBeforeMoreTips() - Legacy helper for a breakdown prompt after the first few tips.
        /// This method is currently retained for backwards compatibility, but the automatic
        /// 3-tip breakdown flow has been removed from both CLI and GUI tip responses.
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
        /// NOW INCLUDES FULL ADVANCED CONVERSATION INTELLIGENCE:
        /// - Analyzes sentiment of the input
        /// - Checks memory and interests
        /// - Detects flexible follow-ups and continuations
        /// - Tracks conversation context (topic, intent, state)
        /// - Uses tip progression tracking (3-tip education transition)
        /// - Provides sentiment-aware responses
        /// - Routes to appropriate topic handler
        /// - Returns response as a string (ideal for GUI)
        /// </summary>
        public string ProcessMessage(string userInput)
        {
            // 1. Basic validation
            if (string.IsNullOrWhiteSpace(userInput))
                return $"I didn't catch that, {_userName}. Please enter something or type 'help' for options.";

            string rawInput = userInput.Trim();
            string lowerInput = rawInput.ToLowerInvariant();
            _lastUserInput = rawInput;

            // 2. Exit command
            if (IsExitCommand(lowerInput))
            {
                _isRunning = false;
                return $"Take care, {_userName}! Remember to stay secure online! 🔒";
            }

            // 2.a Activity log commands
            if (IsShowActivityLogCommand(lowerInput))
            {
                _activityLogPage = 1;
                _activityLogMode = true;
                _activityLogService.AddEntry("ActivityLog", "Viewed activity log.");
                return GetActivityLogPageResponse(_activityLogPage);
            }

            if (_activityLogMode && IsShowMoreActivityLogCommand(lowerInput))
            {
                _activityLogPage++;
                _activityLogService.AddEntry("ActivityLog", $"Viewed more activity log (page {_activityLogPage}).");
                return GetActivityLogPageResponse(_activityLogPage);
            }

            // Reset activity log pagination when the user leaves log mode
            if (_activityLogMode)
            {
                ResetActivityLogPagination();
            }

            // 2.1 Pending task description follow-up
            if (_awaitingTaskDescription)
            {
                return HandlePendingTaskDescription(rawInput);
            }

            // 2.2 Pending reminder follow-up
            if (_awaitingReminderResponse)
            {
                return HandlePendingReminderResponse(lowerInput);
            }

            // 2.3 Task assistant commands
            if (IsTaskCommand(lowerInput))
            {
                _activityLogService.AddEntry("Task", $"Task command received: {lowerInput}");
                return HandleTaskCommand(lowerInput, userInput);
            }

            // 2.3 Tip request – PRIORITIZED early (before conversational replies that might interfere)
            if (IsTipRequest(lowerInput) || IsFlexibleFollowUpRequest(lowerInput))
            {
                // Try to extract topic from the input first
                string extractedTopic = MapKeywordToTopic(lowerInput);

                if (extractedTopic != null)
                {
                    // Topic was specified in the input, use it
                    _currentTopic = extractedTopic;
                }
                else if (string.IsNullOrEmpty(_currentTopic))
                {
                    // No topic in input and no current topic set
                    return "Which topic would you like a tip about? (e.g., phishing, passwords, privacy)";
                }

                // Sentiment detection for tip response context
                string tipSentiment = _sentimentAnalyzer.DetectSentiment(lowerInput);
                _conversationManager.UpdateSentiment(tipSentiment);

                // Delegate to the tracking method (handles 3-tip breakdown offer)
                return GenerateTipResponseWithTracking(userInput, tipSentiment);
            }

            // 2.5 Conversational replies (greetings, small talk, thanks, etc.)
            string? conversationalReply = GetConversationalReply(lowerInput);
            if (conversationalReply != null)
                return conversationalReply;

            // 2.6 Quiz Game integration – handles quiz commands and active quiz answers
            string? quizResponse = _quizManager.HandleInput(lowerInput);
            if (quizResponse != null)
            {
                _activityLogService.AddEntry("Quiz", $"Quiz interaction: {lowerInput}");
                return quizResponse;
            }



            // 3. Handle pending breakdown confirmation
            if (_awaitingBreakdownConfirmation)
                return HandleBreakdownConfirmation(lowerInput);

            // 4. Memory recall request
            if (IsMemoryRecallRequest(lowerInput))
            {
                _activityLogService.AddEntry("Memory", "Recalled stored user interests.");
                return GetMemoryRecallResponse();
            }

            // 5. Interest detection & storage
            if (DetectAndStoreInterest(lowerInput))
            {
                _activityLogService.AddEntry("Interest", $"Stored interest in topic: {_currentTopic}");
                return $"Got it! I'll remember that you're interested in {_currentTopic}.";
            }

            // 6. Sentiment detection (with optional topic association)
            string sentiment = _sentimentAnalyzer.DetectSentiment(lowerInput);
            _conversationManager.UpdateSentiment(sentiment);
            if (TryHandleSentimentWithTopic(lowerInput, sentiment, out string sentimentResponse))
                return sentimentResponse;

            // 7. Explicit topic request (full breakdown)
            if (TryGetTopicFromInput(lowerInput, out string topic))
            {
                _activityLogService.AddEntry("Topic", $"Requested topic breakdown for '{topic}'.");
                _currentTopic = topic;
                _userMemory.AddDiscussedTopic(topic);
                return GetTopicResponse(topic);
            }

            // 8. Help command
            if (lowerInput.Contains("help"))
                return GetHelpResponse();

            // 9. Fallback
            return $"I'm not sure how to respond to that, {_userName}. Try asking about a cybersecurity topic (e.g., 'phishing'), or type 'help' for options.";
        }

        private static readonly string[] _taskCommandTriggers =
        {
    "add task", "create task", "new task",
    "show tasks", "list tasks", "view tasks",
    "pending tasks",
    "complete task", "complete ",
    "delete task", "remove task", "delete ", "remove ",
    "remind me", "remind "
};

        private bool IsTaskCommand(string lowerInput)
        {
            return _taskCommandTriggers.Any(trigger => lowerInput.Contains(trigger));
        }

        private string HandleTaskCommand(string lowerInput, string rawInput)
        {
            // Show all tasks
            if (lowerInput.Contains("show tasks") || lowerInput.Contains("list tasks") || lowerInput.Contains("view tasks"))
                return FormatTaskList(_taskService.GetAllTasks());

            // Show only pending tasks
            if (lowerInput.Contains("pending tasks"))
                return FormatTaskList(_taskService.GetPendingTasks());

            // Complete a task
            if (lowerInput.Contains("complete task") || lowerInput.StartsWith("complete "))
                return HandleCompleteTaskCommand(lowerInput);

            // Delete/remove a task
            if (lowerInput.Contains("delete task") || lowerInput.Contains("remove task") ||
                lowerInput.StartsWith("delete ") || lowerInput.StartsWith("remove "))
                return HandleDeleteTaskCommand(lowerInput);

            // Add a new task
            if (lowerInput.Contains("add task") || lowerInput.Contains("create task") || lowerInput.Contains("new task"))
                return HandleAddTaskCommand(rawInput);

            // Set a reminder
            if (lowerInput.Contains("remind me"))
                return HandleSetReminderCommand(lowerInput);

            return "I can help manage tasks. Try commands like 'add task review privacy settings', 'show tasks', 'complete task 1', or 'remind me in 3 days'.";
        }

        private string HandleAddTaskCommand(string rawInput)
        {
            string lowerInput = rawInput.ToLowerInvariant();
            string titleText = rawInput;
            string[] patterns = { "add task", "create task", "new task" };

            foreach (string pattern in patterns)
            {
                int idx = lowerInput.IndexOf(pattern, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    titleText = rawInput.Substring(idx + pattern.Length).Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(titleText))
                return "Please tell me what task to add, for example: 'Add task review privacy settings'.";

            if (TryParseTaskTitleAndInlineDescription(titleText, out string title, out string? description))
            {
                title = TaskService.ConvertTitleToFriendlyText(title);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var task = _taskService.AddTask(title, description.Trim());
                    _pendingReminderTaskId = task.Id;
                    _awaitingReminderResponse = true;

                    return $"Task added:\nTitle: {task.Title}\nDescription: {task.Description}\nWould you like to set a reminder?";
                }

                _pendingTaskTitle = title;
                _awaitingTaskDescription = true;
                return $"Okay, what description should I use for '{title}'?";
            }

            return "Please tell me what task to add, for example: 'Add task review privacy settings'.";
        }

        private bool TryParseTaskTitleAndInlineDescription(string titleText, out string title, out string? description)
        {
            title = titleText.Trim();
            description = null;
            if (string.IsNullOrWhiteSpace(title))
                return false;

            string[] separators = { ":", " - ", " — ", " | " };
            foreach (string separator in separators)
            {
                int separatorIndex = title.IndexOf(separator, StringComparison.Ordinal);
                if (separatorIndex >= 0)
                {
                    description = title.Substring(separatorIndex + separator.Length).Trim();
                    title = title.Substring(0, separatorIndex).Trim();
                    break;
                }
            }

            return !string.IsNullOrWhiteSpace(title);
        }

        private string HandlePendingTaskDescription(string rawInput)
        {
            string cleanInput = rawInput.Trim();
            if (string.IsNullOrWhiteSpace(cleanInput))
                return "Please provide a short description for the task.";

            string lowerInput = cleanInput.ToLowerInvariant();
            if (IsPositiveResponse(lowerInput) && cleanInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 2)
                return "Sure. Please type a brief task description, for example: 'Check app permissions and adjust privacy settings'.";

            if (lowerInput == "skip" || lowerInput == "no" || lowerInput == "none")
            {
                cleanInput = BuildTaskDescriptionFromTitle(_pendingTaskTitle ?? "task");
            }

            string title = _pendingTaskTitle ?? "New Task";
            var task = _taskService.AddTask(title, cleanInput);
            _pendingReminderTaskId = task.Id;
            _awaitingReminderResponse = true;
            _awaitingTaskDescription = false;
            _pendingTaskTitle = null;

            return $"Task added:\nTitle: {task.Title}\nDescription: {task.Description}\nWould you like to set a reminder?";
        }

        private string HandleCompleteTaskCommand(string lowerInput)
        {
            if (!TryParseTaskId(lowerInput, out int taskId))
                return "Please specify which task to complete, such as 'complete task 1'.";

            return _taskService.MarkTaskCompleted(taskId)
                ? "Task marked as completed."
                : $"I couldn't find task {taskId}.";
        }

        private string HandleDeleteTaskCommand(string lowerInput)
        {
            if (!TryParseTaskId(lowerInput, out int taskId))
                return "Please specify which task to delete, such as 'delete task 1'.";

            return _taskService.DeleteTask(taskId)
                ? "Task removed successfully."
                : $"I couldn't find task {taskId}.";
        }

        private string HandleSetReminderCommand(string lowerInput)
        {
            if (!TryParseReminderDate(lowerInput, out DateTime reminderDate))
                return "Please tell me when to remind you, for example 'remind me tomorrow', 'remind me in 3 days', or 'remind me next week'.";

            int? taskId = null;
            if (TryParseTaskId(lowerInput, out int parsedId))
                taskId = parsedId;
            else if (_pendingReminderTaskId.HasValue)
                taskId = _pendingReminderTaskId.Value;
            else
            {
                var pendingTasks = _taskService.GetPendingTasks();
                if (pendingTasks.Count == 1)
                    taskId = pendingTasks[0].Id;
            }

            if (!taskId.HasValue)
                return "Which task should I attach that reminder to? For example: 'remind me in 3 days for task 1'.";

            if (!_taskService.SetReminder(taskId.Value, reminderDate))
                return $"I couldn't set a reminder for task {taskId.Value}.";

            _pendingReminderTaskId = null;
            _awaitingReminderResponse = false;
            return $"Reminder set successfully.\nI'll remind you {GetRelativeReminderText(reminderDate)}.";
        }

        private string HandlePendingReminderResponse(string lowerInput)
        {
            if (TryParseReminderDate(lowerInput, out DateTime reminderDate))
            {
                return SavePendingReminder(reminderDate);
            }

            if (IsNegativeResponse(lowerInput))
            {
                _awaitingReminderResponse = false;
                _pendingReminderTaskId = null;
                return "No problem. The task has been saved without a reminder.";
            }

            if (IsPositiveResponse(lowerInput))
            {
                return "Sure. When would you like me to remind you?";
            }

            return "Please tell me when to remind you, such as 'tomorrow', 'in 2 days', 'next week', or say 'no' if you do not want a reminder.";
        }

        private string SavePendingReminder(DateTime reminderDate)
        {
            int? taskId = _pendingReminderTaskId;
            if (!taskId.HasValue)
            {
                var pendingTasks = _taskService.GetPendingTasks();
                if (pendingTasks.Count == 1)
                    taskId = pendingTasks[0].Id;
            }

            if (!taskId.HasValue)
                return "I couldn't determine which task to attach that reminder to. Please say 'remind me in 2 days for task 1'.";

            if (!_taskService.SetReminder(taskId.Value, reminderDate))
                return $"I couldn't set a reminder for task {taskId.Value}.";

            _awaitingReminderResponse = false;
            _pendingReminderTaskId = null;
            return $"Reminder set successfully.\nI'll remind you {GetRelativeReminderText(reminderDate)}.";
        }

        private bool TryParseReminderDate(string lowerInput, out DateTime reminderDate)
        {
            var today = DateTime.Now.Date;
            if (lowerInput.Contains("tomorrow"))
            {
                reminderDate = today.AddDays(1);
                return true;
            }

            if (lowerInput.Contains("next month"))
            {
                reminderDate = today.AddMonths(1);
                return true;
            }

            if (lowerInput.Contains("next week") || lowerInput.Contains("a week"))
            {
                reminderDate = today.AddDays(7);
                return true;
            }

            if (lowerInput.Contains("today"))
            {
                reminderDate = today;
                return true;
            }

            var match = Regex.Match(lowerInput, @"\bin\s*(\d+)\s*days?\b");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            {
                reminderDate = today.AddDays(days);
                return true;
            }

            reminderDate = default;
            return false;
        }

        private bool TryParseTaskId(string lowerInput, out int taskId)
        {
            var match = Regex.Match(lowerInput, @"\b(?:task\s*)?(\d+)\b");
            if (match.Success && int.TryParse(match.Groups[1].Value, out taskId))
                return true;

            taskId = 0;
            return false;
        }

        private string BuildTaskDescriptionFromTitle(string title)
        {
            // Map keywords to pre‑written descriptions
            var descriptionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Privacy Settings", "Review your privacy settings to ensure your data is protected." },
        { "Password", "Review your password strategy and strengthen any weak or reused credentials." },
        { "backup", "Confirm your backups are current and stored securely." },
        { "two-factor", "Verify your two-factor authentication is enabled and working for important accounts." },
        { "2fa", "Verify your two-factor authentication is enabled and working for important accounts." }
    };

            foreach (var kvp in descriptionMap)
            {
                if (title.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            return $"Review {title.ToLowerInvariant()} to keep your cybersecurity strong.";
        }

        private string FormatTaskList(List<TaskItem> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return "You have no tasks yet. Add one by saying 'add task ...'.";

            var lines = new List<string> { "Your Tasks" };
            foreach (var task in tasks)
            {
                lines.Add($"{task.Id}. {task.Title}");
                lines.Add($"Description: {task.Description}");
                lines.Add(task.ReminderDate.HasValue
                    ? $"Reminder: {GetRelativeReminderText(task.ReminderDate.Value)}"
                    : "Reminder: none");
                lines.Add($"Status: {(task.IsCompleted ? "Completed" : "Pending")}");
                lines.Add(string.Empty);
            }

            return string.Join("\n", lines).TrimEnd();
        }

        private string GetRelativeReminderText(DateTime reminderDate)
        {
            int deltaDays = (reminderDate.Date - DateTime.Now.Date).Days;
            return deltaDays switch
            {
                < 0 => $"overdue by {Math.Abs(deltaDays)} day(s)",
                0 => "today",
                1 => "tomorrow",
                7 => "next week",
                > 1 => $"in {deltaDays} days",
            };
        }

        private string HandleBreakdownConfirmation(string lowerInput)
        {
            // Positive response: yes, sure, absolutely, etc.
            if (IsPositiveResponse(lowerInput))
            {
                _awaitingBreakdownConfirmation = false;
                string topicToShow = _pendingTopic ?? _currentTopic;
                _pendingTopic = null;

                return !string.IsNullOrEmpty(topicToShow)
                    ? GetTopicResponse(topicToShow)
                    : "Which topic would you like a full breakdown on?";
            }

            // Negative response: no, nope, not now, etc.
            if (IsNegativeResponse(lowerInput))
            {
                _awaitingBreakdownConfirmation = false;
                _pendingTopic = null;
                return $"No problem! Let me know if you'd like to learn about another cybersecurity topic.";
            }

            // Any other response (neither yes nor no) – stay in confirmation state and re‑prompt
            return "Please answer 'yes' or 'no'. Would you like the full breakdown?";
        }

        private string GetMemoryRecallResponse()
        {
            var favorite = _userMemory.HasFavoriteTopic() ? _userMemory.GetFavoriteTopic() : null;
            var interests = _userMemory.GetInterests();
            if (favorite != null)
                return $"You told me your favorite topic is {favorite}.";
            if (interests.Count > 0)
                return $"You've shown interest in: {string.Join(", ", interests)}.";
            return "I don't have any interests stored for you yet. Tell me what you're interested in!";
        }

        private bool TryHandleSentimentWithTopic(string lowerInput, string sentiment, out string response)
        {
            response = null;

            if (DetectSentimentWithTopic(lowerInput, sentiment, out string detectedTopic))
            {
                _currentTopic = detectedTopic;

                // Generate a tip using the full tracking method (replaces the removed GenerateSingleTip)
                string tipResponse = GenerateTipResponseWithTracking(lowerInput, sentiment);

                // Fallback in case tip generation returns an empty or null string
                if (string.IsNullOrWhiteSpace(tipResponse))
                    tipResponse = "Let's start with a practical tip to help you feel more secure.";

                response = $"I can see you're {sentiment} about {detectedTopic}. Let's address that together!\n{tipResponse}";
                _activityLogService.AddEntry("Sentiment", $"Detected sentiment '{sentiment}' about topic '{detectedTopic}'.");
                return true;
            }

            return false;
        }

        private bool TryGetTopicFromInput(string lowerInput, out string topic)
        {
            // Use the static alias map from previous refactoring
            return _topicAliasMap.TryGetValue(lowerInput, out topic);
        }

        private string GenerateSingleTip(string topic)
        {
            // Simplified tip generation – you can implement this as a small dictionary
            // or use a method that returns a random tip for the topic.
            // For now, we'll return a generic tip from the full response's first paragraph.
            if (_topicResponses.TryGetValue(topic, out string fullResponse))
            {
                // Extract first sentence or first few lines as a tip
                var firstLine = fullResponse.Split('\n')[0];
                return $"💡 Tip: {firstLine}\n\nWant more? Ask for another tip or type 'breakdown' for full details.";
            }
            return $"I don't have a tip for '{topic}' yet. Try asking about phishing, passwords, or 2FA.";
        }

        // --- You'll need these dictionaries from the previous refactoring ---
        // private static readonly Dictionary<string, string> _topicAliasMap = ...
        // private static readonly Dictionary<string, string> _topicResponses = ...
        // private string GetTopicResponse(string topic) => _topicResponses.GetValueOrDefault(...);

        // Helper: Exit command detection
        private bool IsExitCommand(string input)
        {
            var exitWords = new[] { "exit", "quit", "bye", "goodbye", "farewell" };
            var separators = new[] { ' ', '\t', '.', ',', '!', '?', ';', ':', '-', '_', '/', '\\', '"', '\'' };
            var tokens = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            // Only treat "0" as exit when it is a standalone token, not when part of "10".
            if (tokens.Contains("0"))
                return true;

            return tokens.Any(token => exitWords.Contains(token));
        }

        // Helper: Positive/Negative response detection for confirmations
        private bool IsPositiveResponse(string input)
        {
            var yesWords = new[] { "yes", "yep", "yeah", "y", "sure", "of course", "please do", "absolutely" };
            return yesWords.Any(word => input == word || input.StartsWith(word) || input.Contains(word));
        }
        private bool IsNegativeResponse(string input)
        {
            var noWords = new[] { "no", "nope", "n", "not now", "maybe later", "not really" };
            return noWords.Any(word => input == word || input.StartsWith(word) || input.Contains(word));
        }

        // Helper: Get education for a topic (full breakdown)
        private string GetEducationForTopic(string topic)
        {
            if (string.IsNullOrEmpty(topic))
                return "Which topic would you like a full breakdown on?";
            switch (topic.ToLower())
            {
                case "phishing": return GetComprehensiveEducation("phishing");
                case "passwords": return GetComprehensiveEducation("passwords");
                case "2fa": return GetComprehensiveEducation("2fa");
                case "privacy": return GetComprehensiveEducation("privacy");
                case "browsing": return GetComprehensiveEducation("browsing");
                case "ransomware": return GetComprehensiveEducation("ransomware");
                case "social engineering": return GetComprehensiveEducation("social engineering");
                case "patch management": return GetComprehensiveEducation("patch management");
                case "wifi": return GetComprehensiveEducation("wifi");
                case "password manager": return GetComprehensiveEducation("password manager");
                default: return $"Sorry, I don't have a full breakdown for {topic}.";
            }
        }

        // Helper: Detect and store interest, update _currentTopic
        private bool DetectAndStoreInterest(string input)
        {
            var interestPatterns = new List<string>
            {
                "interested in", "interested about", "like learning about", "like to learn about", "care about", "is important to me", "concerned about", "want to know about", "want to learn about", "learn more about", "understand more about", "need to know about"
            };
            if (interestPatterns.Any(pattern => input.Contains(pattern)))
            {
                string? topic = MapKeywordToTopic(input);
                if (topic != null)
                {
                    _userMemory.AddInterest(topic);
                    _currentTopic = topic;
                    return true;
                }
            }
            return false;
        }

        // Helper: Detect memory recall request
        // Pattern list for memory recall – static to avoid reallocation on every call
        private static readonly List<string> _memoryRecallPatterns = new()
        {
            "what topic am i interested in",
            "what do you remember about me",
            "what's my favourite topic",
            "what is my favourite topic",
            "what are my interests",
            "what do you know about me"
        };

        private bool IsMemoryRecallRequest(string input)
        {
            return _memoryRecallPatterns.Any(pattern => input.Contains(pattern));
        }

        /// <summary>
        /// Detects if the input expresses a sentiment (non‑neutral) about a specific cybersecurity topic.
        /// </summary>
        /// <param name="input">Normalised user input.</param>
        /// <param name="sentiment">Detected sentiment (positive, worried, frustrated, etc.).</param>
        /// <param name="topic">The topic mentioned in the input, if any.</param>
        /// <returns>True if a non‑neutral sentiment and a topic are both present; otherwise false.</returns>
        private bool DetectSentimentWithTopic(string input, string sentiment, out string? topic)
        {
            topic = null;

            // Only interested in non‑neutral sentiments
            if (string.IsNullOrEmpty(sentiment) || sentiment == "neutral")
                return false;

            // Try to extract a topic from the input
            topic = MapKeywordToTopic(input);
            return topic != null;
        }

        // Helper: Detect topic with aliases/synonyms
        private string? DetectTopicWithAliases(string input)
        {
            // Expand this dictionary for more flexible topic detection
            var aliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "email scam", "phishing" },
                { "scam email", "phishing" },
                { "hackers stealing passwords", "passwords" },
                { "safe browsing online", "browsing" },
                { "browsing safety", "browsing" },
                { "wifi security", "wifi" },
                { "public wifi", "wifi" },
                { "vpn", "wifi" },
                { "password vault", "password manager" },
                { "2 factor", "2fa" },
                { "two factor", "2fa" },
                { "authentication", "2fa" },
                { "social manipulation", "social engineering" },
                { "software update", "patch management" },
                { "patch", "patch management" },
                { "update", "patch management" },
                { "privacy", "privacy" },
                { "ransom", "ransomware" },
                { "ransomware", "ransomware" },
                { "password", "passwords" },
                { "passwords", "passwords" },
                { "browsing", "browsing" },
                { "phishing", "phishing" }
            };
            foreach (var kvp in aliasMap)
            {
                if (input.Contains(kvp.Key))
                    return kvp.Value;
            }
            return null;
        }
        private string GetHelpResponse()
        {
            return
            $"📚 Ready to learn about cybersecurity, {_userName}?\n\n" +

            "I'm here to help you stay safe online by teaching you about common cyber threats, security best practices, and ways to protect your personal information.\n\n" +

            "🔒 Cybersecurity Topics You Can Learn About:\n" +
            "   • Type '1' for 'phishing' - Learn how scammers trick people through fake emails and messages\n" +
            "   • Type '2' for 'passwords' - Learn how to create strong and secure passwords\n" +
            "   • Type '3' for '2FA' - Learn how two-factor authentication protects your accounts\n" +
            "   • Type '4' for 'privacy' - Learn how to keep your personal data private online\n" +
            "   • Type '5' for 'browsing' - Learn how to browse the internet safely\n" +
            "   • Type '6' for 'ransomware' - Learn how ransomware attacks work and how to avoid them\n" +
            "   • Type '7' for 'social engineering' - Learn how attackers manipulate people into giving away information\n" +
            "   • Type '8' for 'patch management' - Learn why updating software is important for security\n" +
            "   • Type '9' for 'public WiFi' - Learn how to stay safe while using public networks\n" +
            "   • Type '10' for 'password managers' - Learn how password managers improve security\n\n" +

            "🎮 Cybersecurity Quiz Game (NEW!):\n" +
            "   • Type 'start quiz' - Begin a 20-question quiz covering all topics\n" +
            "   • Type 'quiz score' - See your current score while playing\n" +
            "   • Type 'pause quiz' or 'stop quiz' - Pause the quiz (you can resume later)\n" +
            "   • Type 'resume quiz' or 'continue quiz' - Continue where you left off\n" +
            "   • Type 'restart quiz' - Start a completely new quiz (current progress lost)\n\n" +

            "📋 Task Assistant (NEW!):\n" +
            "   • Type 'add task review privacy settings' - Create a new cybersecurity task\n" +
            "   • Type 'show tasks' - List all your tasks\n" +
            "   • Type 'pending tasks' - Show only incomplete tasks\n" +
            "   • Type 'complete task 1' - Mark a task as completed\n" +
            "   • Type 'delete task 1' - Remove a task\n" +
            "   • Type 'remind me tomorrow' - Set a reminder for the most recent task\n" +
            "   • Type 'remind me in 3 days for task 2' - Attach reminder to a specific task\n\n" +

            "� Activity Log (NEW!):\n" +
            "   • Type 'show activity log' - View your recent activities (tips viewed, topics learned, tasks added, etc.)\n" +
            "   • Type 'show more' - View the next page of activity history\n" +
            "   • Your actions are automatically logged — tips, quizzes, tasks, interests, and more!\n\n" +

            "�💡 Interactive Features:\n" +
            "   • Ask for cybersecurity tips on any of the 10 topics I cover\n" +
            "   • Ask follow-up questions like 'Tell me more' or 'Give me another tip'\n" +
            "   • Share your interests so I can personalise your learning experience\n" +
            "   • Talk naturally — you don't always need exact keywords\n\n" +

            "💬 Conversational Commands:\n" +
            "   • Ask things like 'How are you?' or 'What's your purpose?'\n" +
            "   • Type 'help' anytime to see this menu again\n" +
            "   • Type 'exit' or 'quit' whenever you'd like to leave\n\n" +

            "🛡️ Your cybersecurity journey starts now.\n" +
            "What would you like to learn about today?";

        }

        private string RouteToTopicResponse(string lowerInput, string sentiment)
        {
            // sentiment is unused – consider removing from signature
            _ = sentiment;

            // Try to map alias to topic name
            if (_topicAliasMap.TryGetValue(lowerInput, out string topic))
            {
                return GetTopicResponse(topic);
            }

            // Handle conversational replies
            return GetConversationalReply(lowerInput);
        }

        // --- Dictionaries (initialise once, e.g. in constructor or statically) ---

        private static readonly Dictionary<string, string> _topicAliasMap = new(StringComparer.OrdinalIgnoreCase)
{
    // PHISHING
    { "1", "phishing" },
    { "phishing", "phishing" },
    { "phishing attacks", "phishing" },

    // PASSWORDS
    { "2", "passwords" },
    { "password", "passwords" },
    { "passwords", "passwords" },

    // 2FA
    { "3", "2fa" },
    { "2fa", "2fa" },
    { "two-factor", "2fa" },
    { "two factor", "2fa" },
    { "authentication", "2fa" },

    // PRIVACY
    { "4", "privacy" },
    { "privacy", "privacy" },
    { "data privacy", "privacy" },

    // BROWSING
    { "5", "browsing" },
    { "browsing", "browsing" },
    { "secure browsing", "browsing" },

    // RANSOMWARE
    { "6", "ransomware" },
    { "ransomware", "ransomware" },

    // SOCIAL ENGINEERING
    { "7", "social_engineering" },
    { "social engineering", "social_engineering" },
    { "social engineer", "social_engineering" },

    // PATCH MANAGEMENT
    { "8", "patch_management" },
    { "patch", "patch_management" },
    { "patch management", "patch_management" },
    { "update", "patch_management" },
    { "updates", "patch_management" },

    // WIFI
    { "9", "wifi" },
    { "wifi", "wifi" },
    { "public wifi", "wifi" },
    { "public wi-fi", "wifi" },

    // PASSWORD MANAGER
    { "10", "password_manager" },
    { "password manager", "password_manager" },
    { "password managers", "password_manager" }
};

        private static readonly Dictionary<string, string> _topicResponses = new()
        {
            ["phishing"] = "PHISHING ATTACKS\n\n" +
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

            ["passwords"] = "STRONG PASSWORDS\n\n" +
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

            ["2fa"] = "TWO-FACTOR AUTHENTICATION (2FA)\n\n" +
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

            ["privacy"] = "DATA PRIVACY\n\n" +
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

            ["browsing"] = "SECURE BROWSING\n\n" +
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

            ["ransomware"] = "RANSOMWARE\n\n" +
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

            ["social_engineering"] = "SOCIAL ENGINEERING\n\n" +
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

            ["patch_management"] = "SOFTWARE UPDATES & PATCH MANAGEMENT\n\n" +
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

            ["wifi"] = "PUBLIC WI-FI SAFETY\n\n" +
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

            ["password_manager"] = "PASSWORD MANAGERS\n\n" +
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
                "✓ Regularly backup encrypted vault"
        };

        private string GetTopicResponse(string topic)
        {
            // Record the interaction
            _userMemory.AddDiscussedTopic(topic);
            _currentTopic = topic;

            // Return the pre-defined response or a fallback
            return _topicResponses.GetValueOrDefault(topic, $"I can help you with {topic}. Would you like more details?");
        }

        private string? GetConversationalReply(string lowerInput)
        {
            // 1. GREETINGS (including time-specific)
            if (lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("hey") ||
                lowerInput.Contains("good morning") || lowerInput.Contains("morning") ||
                lowerInput.Contains("good afternoon") || lowerInput.Contains("afternoon") ||
                lowerInput.Contains("good evening") || lowerInput.Contains("evening") ||
                lowerInput.Contains("greetings") || lowerInput.Contains("howdy"))
            {
                // Time-appropriate greeting (optional)
                string timeGreeting = GetTimeBasedGreeting();
                return $"{timeGreeting} {_userName}! 👋 I'm your cybersecurity assistant. What would you like to learn about today?";
            }

            // 2. HOW ARE YOU / FEELING
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how do you do") ||
                lowerInput.Contains("how's it going") || lowerInput.Contains("how are things") ||
                lowerInput.Contains("you okay") || lowerInput.Contains("feeling"))
            {
                return $"I'm doing fantastic, {_userName}! 😊 Always ready to help you stay safe online. How can I assist you with cybersecurity today?";
            }

            // 3. THANKS / GRATITUDE
            if (lowerInput.Contains("thank") || lowerInput.Contains("thanks") || lowerInput.Contains("appreciate") ||
                lowerInput.Contains("grateful") || lowerInput.Contains("thx") || lowerInput == "ty")
            {
                return $"You're very welcome, {_userName}! 🙏 I'm glad to help. Feel free to ask me about any cybersecurity topic – phishing, passwords, 2FA, privacy, and more!";
            }


            // 4. PURPOSE / CAPABILITIES ("what can you do", "what is your purpose", "what do you do")
            if (lowerInput.Contains("what is your purpose") || lowerInput.Contains("what's your purpose") ||
                lowerInput.Contains("what can you do") || lowerInput.Contains("what do you do") ||
                lowerInput.Contains("how can you help") || lowerInput.Contains("your function") ||
                lowerInput.Contains("capabilities") || lowerInput.Contains("features"))
            {
                return $"My purpose is to teach you about cybersecurity awareness and best practices! 🛡️ I can explain topics like phishing, passwords, 2FA, privacy, ransomware, secure browsing, public Wi-Fi safety, and more. Just ask me about any of these, or say 'help' for the menu.";
            }

            // 5. IDENTITY / CREATOR ("who are you", "who made you", "what are you", "your name")
            if (lowerInput.Contains("who are you") || lowerInput.Contains("what are you") ||
                lowerInput.Contains("who made you") || lowerInput.Contains("who created you") ||
                lowerInput.Contains("your creator") || lowerInput.Contains("what is your name") ||
                lowerInput.Contains("what's your name") || lowerInput.Contains("called"))
            {
                return $"I'm your friendly cybersecurity awareness bot! 🤖 My name is SecuriBot (you can call me Securi). I was built by security educators to help people like you stay safe online. What can I teach you today?";
            }

            // 6. COMPLIMENTS / POSITIVE FEEDBACK
            if (lowerInput.Contains("you are awesome") || lowerInput.Contains("you're awesome") ||
                lowerInput.Contains("you are great") || lowerInput.Contains("you're great") ||
                lowerInput.Contains("good bot") || lowerInput.Contains("you help") ||
                lowerInput.Contains("i like you") || lowerInput.Contains("you are helpful"))
            {
                return $"Aww, thank you, {_userName}! 🥰 That means a lot. I'm here 24/7 to help you learn cybersecurity – just ask me anything!";
            }

            // 7. APOLOGIES
            if (lowerInput.Contains("sorry") || lowerInput.Contains("apologise") || lowerInput.Contains("apologize") ||
                lowerInput.Contains("my bad") || lowerInput.Contains("i'm sorry"))
            {
                return $"No worries at all, {_userName}! 😊 Everyone makes mistakes. How can I help you with cybersecurity today?";
            }

            // 8. CONFUSION / REPETITION ("what", "huh", "repeat", "again")
            if (lowerInput == "what" || lowerInput == "huh" || lowerInput == "repeat" ||
                lowerInput.Contains("say that again") || lowerInput.Contains("come again") ||
                lowerInput.Contains("i don't understand") || lowerInput.Contains("i didnt get that") ||
                lowerInput.Contains("confused"))
            {
                return $"I'm sorry if I wasn't clear, {_userName}. 😅 Feel free to ask me about specific cybersecurity topics like 'phishing', 'passwords', '2FA', or 'privacy'. You can also type 'help' for a full list of what I can do.";
            }

            // 9. JOKES / FUN (optional)
            if (lowerInput.Contains("tell me a joke") || lowerInput.Contains("make me laugh") ||
                lowerInput.Contains("funny") || lowerInput.Contains("joke") && !lowerInput.Contains("no joke"))
            {
                return $"Why do hackers wear leather jackets? Because they have to deal with a lot of firewalls! 😂";
            }


            // 10. DISAGREEMENT / NEGATION (gentle redirection)
            if (lowerInput == "no" || lowerInput == "nope" || lowerInput == "nah" || lowerInput == "not really" ||
                lowerInput.Contains("that's wrong") || lowerInput.Contains("incorrect"))
            {
                return $"I understand, {_userName}. 🙏 Let me know what you're looking for, and I'll do my best to help. Cybersecurity topics I cover: phishing, passwords, 2FA, privacy, ransomware, and more.";
            }

            // No conversational match
            return null;
        }

        // Optional helper for time-based greeting
        private string GetTimeBasedGreeting()
        {
            var hour = DateTime.Now.Hour;
            if (hour < 12) return "Good morning";
            if (hour < 18) return "Good afternoon";
            return "Good evening";
        }

        private void ResetActivityLogPagination()
        {
            _activityLogMode = false;
            _activityLogPage = 0;
        }

        private bool IsShowActivityLogCommand(string lowerInput)
        {
            return lowerInput.Contains("show activity log")
                   || lowerInput.Contains("activity log")
                   || lowerInput.Contains("show activity")
                   || lowerInput.Contains("view activity");
        }

        private bool IsShowMoreActivityLogCommand(string lowerInput)
        {
            return lowerInput.Contains("show more")
                   || lowerInput.Contains("more")
                   || lowerInput.Contains("next page")
                   || lowerInput.Contains("next");
        }

        private string GetActivityLogPageResponse(int page)
        {
            if (!_activityLogService.HasEntries)
                return "Your activity log is currently empty. Start using the bot and I will record your actions here.";

            var entries = _activityLogService.GetEntries(page);
            if (entries.Count == 0)
            {
                ResetActivityLogPagination();
                return "There are no more activity log entries to show.";
            }

            var responseBuilder = new System.Text.StringBuilder();
            responseBuilder.AppendLine($"Activity Log — Page {page} of {_activityLogService.GetTotalPages()}");
            responseBuilder.AppendLine("-----------------------------------");

            foreach (var entry in entries)
            {
                responseBuilder.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm}] {entry.ActionType}: {entry.Description}");
            }

            if (_activityLogService.HasMorePages(page))
            {
                responseBuilder.AppendLine();
                responseBuilder.AppendLine("Type 'show more' to see the next page of activity.");
            }

            return responseBuilder.ToString();
        }

        /// <summary>
        /// GenerateTipResponseWithTracking - Enhanced tip generation with full tracking and progression.
        /// 
        /// Features:
        /// - Uses TipTracker to prevent duplicate tips
        /// - Tracks tip count and progression
        /// - Implements 3-tip → education transition
        /// - Handles tip exhaustion gracefully
        /// - Provides sentiment-aware responses
        /// - Updates conversation state properly
        /// </summary>
        private string GenerateTipResponseWithTracking(string userInput, string sentiment)
        {
            // Guard clauses
            if (_tipRepository == null) return "Tip service is temporarily unavailable.";
            if (_tipTracker == null) return "Tip tracking is not initialised.";

            // Determine active topic
            string requestedTopic = MapKeywordToTopic(userInput);
            string? activeTopic = (IsFlexibleFollowUpRequest(userInput) || IsContinuationRequest(userInput)) ? _currentTopic : requestedTopic;
            if (activeTopic == null && !string.IsNullOrEmpty(_currentTopic))
                activeTopic = _currentTopic;

            if (activeTopic == null)
                return $"I'd like to help with a tip, {_userName}, but I need to know which topic. Try asking about: phishing, passwords, 2fa, privacy, browsing, ransomware, social engineering, patch management, wifi, or password manager.";

            if (!_tipRepository.HasTopic(activeTopic))
                return $"I don't have tips for '{activeTopic}' yet, {_userName}. Try another cybersecurity topic from the menu.";

            var allTips = _tipRepository.GetAllTips(activeTopic);
            int totalTips = allTips?.Count ?? 0;

            _currentTopic = activeTopic;
            _conversationManager.UpdateTopic(activeTopic);

            if (_tipTracker.HasAllTipsBeenShown(activeTopic, totalTips))
                return $"You've already learned all {totalTips} tips about {activeTopic}! 🎓\n\nWould you like to:\n  • Explore a different topic (e.g., 'tell me about privacy')\n  • Get a comprehensive breakdown of {activeTopic}";

            string tip = _tipTracker.GetNextUnusedTip(activeTopic, allTips);
            if (string.IsNullOrEmpty(tip))
                return $"Sorry, I couldn't retrieve a tip for {activeTopic} right now, {_userName}.";

            _activityLogService.AddEntry("Tip", $"Delivered tip for {activeTopic}.");

            string response = "";
            string sentimentPrefix = GetSentimentAwarePrefix(sentiment, activeTopic);
            if (!string.IsNullOrEmpty(sentimentPrefix))
                response += sentimentPrefix + "\n\n";

            response += $"💡 TIP: {tip}\n";

            _tipCount = _tipTracker.GetTipCount(activeTopic);
            int remainingTips = _tipTracker.GetRemainingTipCount(activeTopic, totalTips);
            _conversationManager.SetIntentType("TipRequest");
            _conversationManager.SetResponseCategory("Tip");
            _conversationManager.IncrementTipsShown();

            response += "\n";
            if (remainingTips > 0)
            {
                response += $"You have {remainingTips} more tip(s) about {activeTopic}. Ask for another tip when you're ready. 💬";
            }
            else
            {
                response += $"\nYou've now received all available tips for {activeTopic}! 🎯\nTry exploring another cybersecurity topic or ask for the full {activeTopic} overview.";
            }

            return response;
        }

        /// <summary>
        /// GenerateDeepDiveResponse - Provides deeper educational content for a topic.
        /// Called when user requests more details about current topic.
        /// </summary>
        private string GenerateDeepDiveResponse(string topic, string sentiment)
        {
            _userMemory.AddDiscussedTopic(topic);
            _conversationManager.SetIntentType("DeepDive");
            _conversationManager.SetResponseCategory("Education");

            string response = "";
            string intro = GetEducationIntroduction(sentiment, topic);
            if (!string.IsNullOrEmpty(intro))
                response += intro + "\n\n";

            string memoryPersonalization = GetMemoryPersonalization(topic);
            if (!string.IsNullOrEmpty(memoryPersonalization))
                response += memoryPersonalization + "\n\n";

            response += GetComprehensiveEducation(topic);
            return response;
        }

        /// <summary>
        /// GetComprehensiveEducation - Returns full educational content for a topic.
        /// </summary>
        private string GetComprehensiveEducation(string topic)
        {
            return topic switch
            {
                "phishing" => "PHISHING ATTACKS - COMPREHENSIVE GUIDE\n\n" +
                    "Phishing is a type of social engineering attack where attackers impersonate legitimate organizations to steal credentials or data.\n\n" +
                    "🔴 KEY RISKS:\n" +
                    "• Email spoofing - emails appear to come from trusted sources\n" +
                    "• Credential theft - tricking you into entering your password\n" +
                    "• Malware delivery - links containing malicious software\n" +
                    "• Financial fraud - impersonating banks or payment systems\n" +
                    "• Identity theft - collecting personal information\n\n" +
                    "🛡️ PROTECTION TIPS:\n" +
                    "✓ Check sender email address carefully (look for slight misspellings)\n" +
                    "✓ Look for spelling errors or urgent language\n" +
                    "✓ Never click links from unknown senders - hover over them first\n" +
                    "✓ Verify requests by contacting the organization directly\n" +
                    "✓ Use email filtering and security tools\n" +
                    "✓ Enable multi-factor authentication\n" +
                    "✓ Keep browser and OS updated\n" +
                    "✓ Report phishing attempts to the organization",

                "passwords" => "STRONG PASSWORDS - COMPREHENSIVE GUIDE\n\n" +
                    "Strong passwords are your first line of defense against unauthorized access.\n\n" +
                    "🎯 WHAT MAKES A PASSWORD STRONG:\n" +
                    "• At least 12-16 characters (longer is better)\n" +
                    "• Mix of uppercase and lowercase letters\n" +
                    "• Include numbers and special characters (!@#$%^&*)\n" +
                    "• Avoid dictionary words, names, or birthdates\n" +
                    "• Unique for each account\n" +
                    "• No personal information (pets, family, hobbies)\n\n" +
                    "📋 BEST PRACTICES:\n" +
                    "✓ Use a password manager (1Password, KeePass, Bitwarden)\n" +
                    "✓ Enable two-factor authentication on all accounts\n" +
                    "✓ Change passwords if a service is breached\n" +
                    "✓ Never share your password with anyone\n" +
                    "✓ Use passphrases for accounts you use frequently\n" +
                    "✓ Don't reuse passwords across sites\n" +
                    "✓ Consider using randomly generated passwords",

                "2fa" => "TWO-FACTOR AUTHENTICATION - COMPREHENSIVE GUIDE\n\n" +
                    "2FA requires two forms of verification to access your account, making it exponentially harder for attackers to gain access.\n\n" +
                    "⚙️ HOW IT WORKS:\n" +
                    "1. Something you know (password)\n" +
                    "2. Something you have (phone, authenticator app, security key)\n\n" +
                    "📱 TYPES OF 2FA (FROM LEAST TO MOST SECURE):\n" +
                    "• SMS/Text messages (vulnerable to SIM swapping)\n" +
                    "• Authenticator apps (Google Authenticator, Microsoft Authenticator, Authy)\n" +
                    "• Hardware security keys (YubiKey, Titan - most secure)\n" +
                    "• Biometric (fingerprint, face recognition)\n\n" +
                    "✅ BENEFITS:\n" +
                    "✓ Dramatically increases account security\n" +
                    "✓ Protection even if password is stolen\n" +
                    "✓ Prevents unauthorized access\n" +
                    "✓ Most major services now support it",

                "privacy" => "DATA PRIVACY - COMPREHENSIVE GUIDE\n\n" +
                    "Protecting your personal data is essential in today's digital world.\n\n" +
                    "🔐 WHAT TO PROTECT:\n" +
                    "• Personal identification (name, SSN, birthdate)\n" +
                    "• Financial information (bank accounts, credit cards)\n" +
                    "• Health information (medical records, conditions)\n" +
                    "• Location data (GPS, home address)\n" +
                    "• Browsing history and online activity\n\n" +
                    "🛡️ PRIVACY BEST PRACTICES:\n" +
                    "✓ Use VPN on public Wi-Fi networks\n" +
                    "✓ Check privacy settings on social media\n" +
                    "✓ Be cautious what information you share online\n" +
                    "✓ Use privacy-focused search engines\n" +
                    "✓ Disable location tracking when not needed\n" +
                    "✓ Read privacy policies before using services\n" +
                    "✓ Opt-out of data collection where possible",

                "browsing" => "SECURE BROWSING - COMPREHENSIVE GUIDE\n\n" +
                    "Safe internet browsing habits protect you from malware and phishing attacks.\n\n" +
                    "🌐 BROWSING SAFETY:\n" +
                    "• Look for HTTPS (not HTTP) in website URLs\n" +
                    "• Verify SSL/TLS certificates (lock icon)\n" +
                    "• Avoid clicking suspicious links\n" +
                    "• Be cautious with browser extensions\n" +
                    "• Update browser regularly\n\n" +
                    "🛡️ PROTECTION STRATEGIES:\n" +
                    "✓ Use a modern, updated browser (Chrome, Firefox, Safari, Edge)\n" +
                    "✓ Enable browser security features\n" +
                    "✓ Use ad blockers and anti-tracking tools\n" +
                    "✓ Clear cookies and cache regularly\n" +
                    "✓ Disable unnecessary plugins\n" +
                    "✓ Use private/incognito browsing mode\n" +
                    "✓ Be suspicious of pop-ups",

                "ransomware" => "RANSOMWARE - COMPREHENSIVE GUIDE\n\n" +
                    "Ransomware encrypts your files and demands payment for their return.\n\n" +
                    "⚠️ HOW RANSOMWARE WORKS:\n" +
                    "• Infiltrates system through malicious email or downloads\n" +
                    "• Encrypts files making them inaccessible\n" +
                    "• Demands payment (ransom) for decryption key\n" +
                    "• Can spread to network drives and backup systems\n" +
                    "• Criminals often steal data before encrypting\n\n" +
                    "🛡️ PREVENTION:\n" +
                    "✓ Keep all software updated with latest patches\n" +
                    "✓ Maintain offline backups of important data\n" +
                    "✓ Use comprehensive antivirus/antimalware software\n" +
                    "✓ Enable automatic backups\n" +
                    "✓ Use email filters to block suspicious attachments\n" +
                    "✓ Train employees on phishing\n" +
                    "✓ Never pay ransoms (funds crime)",

                "social engineering" => "SOCIAL ENGINEERING - COMPREHENSIVE GUIDE\n\n" +
                    "Social engineering manipulates people into divulging confidential information.\n\n" +
                    "🎭 COMMON TECHNIQUES:\n" +
                    "• Pretexting (creating false scenario)\n" +
                    "• Baiting (offering something enticing)\n" +
                    "• Quid pro quo (offering something in exchange)\n" +
                    "• Tailgating (following someone through secure door)\n" +
                    "• Phishing (covered separately)\n" +
                    "• Vishing (phishing via phone)\n\n" +
                    "🛡️ PROTECTION:\n" +
                    "✓ Be skeptical of unsolicited requests\n" +
                    "✓ Verify identities before sharing information\n" +
                    "✓ Never share passwords or sensitive data\n" +
                    "✓ Use security awareness training\n" +
                    "✓ Report suspicious behavior\n" +
                    "✓ Follow organizational security policies",

                "patch management" => "PATCH MANAGEMENT - COMPREHENSIVE GUIDE\n\n" +
                    "Software updates patch security vulnerabilities and fix bugs.\n\n" +
                    "🔧 WHY PATCHES MATTER:\n" +
                    "• Fix security vulnerabilities before exploited\n" +
                    "• Address known exploits used by attackers\n" +
                    "• Improve system stability\n" +
                    "• Add new security features\n\n" +
                    "✅ PATCH MANAGEMENT BEST PRACTICES:\n" +
                    "✓ Enable automatic updates on all devices\n" +
                    "✓ Update operating system regularly\n" +
                    "✓ Update all software and applications\n" +
                    "✓ Update browser plugins (Java, Flash)\n" +
                    "✓ Don't delay critical security patches\n" +
                    "✓ Test patches in controlled environment first\n" +
                    "✓ Keep firmware updated on routers/devices",

                "wifi" => "PUBLIC WI-FI SAFETY - COMPREHENSIVE GUIDE\n\n" +
                    "Public Wi-Fi networks are convenient but pose significant security risks.\n\n" +
                    "⚠️ PUBLIC WI-FI RISKS:\n" +
                    "• Unencrypted networks allow packet sniffing\n" +
                    "• Attackers can create fake hotspots\n" +
                    "• No authentication between you and network\n" +
                    "• Data can be intercepted easily\n\n" +
                    "🛡️ PROTECTION STRATEGIES:\n" +
                    "✓ Use a VPN (Virtual Private Network)\n" +
                    "✓ Avoid sensitive transactions on public Wi-Fi\n" +
                    "✓ Use HTTPS websites only\n" +
                    "✓ Disable auto-connect features\n" +
                    "✓ Use mobile hotspot instead when possible\n" +
                    "✓ Enable firewall on your device\n" +
                    "✓ Turn off file sharing on your device",

                "password manager" => "PASSWORD MANAGERS - COMPREHENSIVE GUIDE\n\n" +
                    "Password managers securely store and organize your passwords.\n\n" +
                    "🔐 HOW PASSWORD MANAGERS WORK:\n" +
                    "• Stores encrypted passwords in secure vault\n" +
                    "• Generates strong, unique passwords\n" +
                    "• Auto-fills passwords on websites\n" +
                    "• Syncs across devices securely\n\n" +
                    "✅ POPULAR OPTIONS:\n" +
                    "• 1Password - User-friendly, feature-rich\n" +
                    "• LastPass - Cloud-based, widely used\n" +
                    "• Bitwarden - Open-source, affordable\n" +
                    "• KeePass - Local storage, high control\n\n" +
                    "💡 BENEFITS:\n" +
                    "✓ Use unique strong passwords everywhere\n" +
                    "✓ Never forget passwords\n" +
                    "✓ Reduces credential reuse\n" +
                    "✓ Simplifies password management",

                _ => $"I don't have comprehensive coverage for {topic} at the moment, but I can help with other cybersecurity topics!"
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
