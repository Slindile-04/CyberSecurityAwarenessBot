using CyberSecurityAwarenessBot.Helpers;

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

        /// <summary>
        /// Constructor - Initializes the chatbot with audio path and user's name for personalization.
        /// </summary>
        public CyberSecurityAwarenessBot(string audioPath, string userName)
        {
            _audioPath = audioPath;
            _userName = userName;
            _inputHelper = new InputHelper();
            _isRunning = false;
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
                    UIHelper.PrintColoredLine($"I didn't catch that, {_userName}. Please enter something or type 'help' for options.", ConsoleColor.Yellow);
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
            UIHelper.PrintColoredLine("║  5. Secure Browsing    | 0. Exit                           ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╠════════════════════════════════════════════════════════════╣", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ Or just ask naturally:                                     ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"How are you?\"  • \"What can you help with?\"          ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║ • \"Tell me about phishing\"  • \"help\"                   ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╚════════════════════════════════════════════════════════════╝", ConsoleColor.Cyan);
        }

        /// <summary>
        /// HandleUserInput() - Processes user input and routes to appropriate handler.
        /// 
        /// Supports:
        /// - Numeric menu selections (1-5, 0)
        /// - Natural language questions (case-insensitive keyword matching)
        /// - Conversational queries like "How are you?" or "What's your purpose?"
        /// - Exit commands like "exit", "quit", "bye"
        /// - Help command
        /// </summary>
        private void HandleUserInput(string input)
        {
            string lowerInput = input.Trim().ToLower();

            // Check for numeric menu selections
            switch (lowerInput)
            {
                case "1":
                case "phishing":
                    ProvidePhishingEducation();
                    return;
                case "2":
                case "password":
                case "passwords":
                    ProvidePasswordEducation();
                    return;
                case "3":
                case "2fa":
                case "two-factor":
                case "two factor":
                case "authentication":
                    Provide2FAEducation();
                    return;
                case "4":
                case "privacy":
                case "data privacy":
                    ProvidePrivacyEducation();
                    return;
                case "5":
                case "browsing":
                case "secure browsing":
                    ProvideBrowsingEducation();
                    return;
                case "0":
                case "exit":
                case "quit":
                case "bye":
                case "goodbye":
                    _isRunning = false;
                    UIHelper.PrintColoredLine($"\nTake care, {_userName}! Remember to stay secure online! 🔒", ConsoleColor.Green);
                    return;
            }

            // Check for conversational keywords using Contains for flexibility
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how are ya"))
            {
                RespondToGreeting();
                return;
            }

            if (lowerInput.Contains("what") && (lowerInput.Contains("purpose") || lowerInput.Contains("do") || lowerInput.Contains("help")))
            {
                RespondToPurpose();
                return;
            }

            if (lowerInput.Contains("help") || lowerInput.Contains("options") || lowerInput.Contains("topics"))
            {
                DisplayHelpMessage();
                return;
            }

            // If input doesn't match any known pattern, provide a friendly suggestion
            UIHelper.PrintColoredLine($"\nI'm not sure how to respond to that, {_userName}. Try asking about one of the topics above,", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("or type 'help' for a list of available options.", ConsoleColor.Yellow);
        }

        /// <summary>
        /// RespondToGreeting() - Responds conversationally to greetings like "How are you?"
        /// Uses typing animation and color for a natural, engaging conversation feel.
        /// </summary>
        private void RespondToGreeting()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();
            Console.ForegroundColor = ConsoleColor.Yellow;
            UIHelper.DisplayWithTypingEffect($"✨ I'm functioning perfectly, {_userName}! Thank you for asking.");
            UIHelper.DisplayWithTypingEffect("I'm here to help you learn about cybersecurity and stay safe online.");
            Console.ResetColor();
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// RespondToPurpose() - Explains the chatbot's purpose when asked.
        /// Uses typing animation and color for a natural response.
        /// </summary>
        private void RespondToPurpose()
        {
            Console.WriteLine();
            UIHelper.DisplayTypingIndicator();
            Console.ForegroundColor = ConsoleColor.Yellow;
            UIHelper.DisplayWithTypingEffect($"🎯 My purpose, {_userName}, is to educate you about cybersecurity best practices.");
            UIHelper.DisplayWithTypingEffect("I can teach you about phishing, passwords, two-factor authentication, data privacy, and secure browsing.");
            UIHelper.DisplayWithTypingEffect("Together, we'll help protect South African citizens online!");
            Console.ResetColor();
            UIHelper.AutoScroll();
        }

        /// <summary>
        /// DisplayHelpMessage() - Shows available topics and commands when user asks for help.
        /// Uses color-coded formatting for better visual organization.
        /// </summary>
        private void DisplayHelpMessage()
        {
            UIHelper.PrintColoredLine($"\n📚 Here's how I can help, {_userName}:", ConsoleColor.Green);
            UIHelper.PrintColoredLine("\n🔒 Cybersecurity Topics:", ConsoleColor.Cyan);
            Console.WriteLine("   • Type '1' or 'phishing' - Learn about phishing attacks");
            Console.WriteLine("   • Type '2' or 'password' - Learn about strong passwords");
            Console.WriteLine("   • Type '3' or '2fa' - Learn about two-factor authentication");
            Console.WriteLine("   • Type '4' or 'privacy' - Learn about data privacy");
            Console.WriteLine("   • Type '5' or 'browsing' - Learn about secure browsing");
            UIHelper.PrintColoredLine("\n💬 Conversational:", ConsoleColor.Cyan);
            Console.WriteLine("   • Ask 'How are you?' or 'What's your purpose?'");
            Console.WriteLine("   • Type 'exit' or 'quit' to leave");
        }

        /// <summary>
        /// DisplayWelcomeMessage() - Displays initial welcome message with color styling.
        /// Sets the tone for the interactive session.
        /// </summary>
        private void DisplayWelcomeMessage()
        {
            UIHelper.PrintColoredLine($"\n🎓 Welcome to the Cybersecurity Awareness Bot, {_userName}!", ConsoleColor.Green);
            Console.WriteLine("Learn about important security practices to protect yourself and others online.\n");

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
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            string[] phishingContent = new string[]
            {
                $"┌─ PHISHING ATTACKS EDUCATION FOR {_userName.ToUpper()} ─────┐",
                "│ Phishing is a social engineering attack where attackers",
                "│ impersonate trusted entities to steal sensitive information.",
                "│",
                "│ RED FLAGS:",
                "│ • Urgent requests for passwords or personal information",
                "│ • Suspicious sender email addresses",
                "│ • Links that don't match the displayed text",
                "│ • Grammar and spelling errors",
                "│",
                "│ PREVENTION:",
                "│ • Never click links in unexpected emails",
                "│ • Verify sender identity independently",
                "│ • Use email filtering and anti-phishing tools",
                "│ • Report suspicious emails to your IT department",
                "└───────────────────────────────────────────────────────────┘"
            };
            
            UIHelper.DisplayWithTypingEffectMultiLine(phishingContent);
            Console.ResetColor();
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
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            string[] passwordContent = new string[]
            {
                $"┌─ STRONG PASSWORD EDUCATION FOR {_userName.ToUpper()} ─────┐",
                "│ A strong password is your first defense against",
                "│ unauthorized access to your accounts.",
                "│",
                "│ STRONG PASSWORD CRITERIA:",
                "│ • At least 12 characters long",
                "│ • Mix of uppercase and lowercase letters",
                "│ • Include numbers and special characters (!@#$%^&*)",
                "│ • Avoid common words, dates, and personal information",
                "│ • Use unique passwords for each account",
                "│",
                "│ BEST PRACTICES:",
                "│ • Use a password manager to securely store passwords",
                "│ • Change passwords if compromised",
                "│ • Never share your password with anyone",
                "└───────────────────────────────────────────────────────────┘"
            };
            
            UIHelper.DisplayWithTypingEffectMultiLine(passwordContent);
            Console.ResetColor();
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
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            string[] twoFAContent = new string[]
            {
                $"┌─ TWO-FACTOR AUTHENTICATION FOR {_userName.ToUpper()} ─────────────────┐",
                "│ 2FA adds an extra layer of security by requiring",
                "│ a second verification method in addition to your password.",
                "│",
                "│ COMMON 2FA METHODS:",
                "│ • SMS codes sent to your phone",
                "│ • Authenticator apps (Google Authenticator, Microsoft Authenticator)",
                "│ • Hardware security keys (FIDO2)",
                "│ • Biometric verification (fingerprint, face recognition)",
                "│",
                "│ RECOMMENDATIONS:",
                "│ • Enable 2FA on all critical accounts",
                "│ • Prefer authenticator apps over SMS when possible",
                "│ • Store backup codes in a secure location",
                "└───────────────────────────────────────────────────────────────────────┘"
            };
            
            UIHelper.DisplayWithTypingEffectMultiLine(twoFAContent);
            Console.ResetColor();
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
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            string[] privacyContent = new string[]
            {
                $"┌─ DATA PRIVACY EDUCATION FOR {_userName.ToUpper()} ─────────┐",
                "│ Your personal data is valuable. Protect it by being",
                "│ mindful of what information you share online.",
                "│",
                "│ PRIVACY RISKS:",
                "│ • Identity theft",
                "│ • Data breaches",
                "│ • Targeted advertisements and manipulation",
                "│ • Financial fraud",
                "│",
                "│ PROTECTION MEASURES:",
                "│ • Review privacy settings on social media accounts",
                "│ • Limit the information you share publicly",
                "│ • Use VPNs on public Wi-Fi networks",
                "│ • Regularly check your credit reports",
                "│ • Read privacy policies before using new services",
                "└─────────────────────────────────────────────────────────┘"
            };
            
            UIHelper.DisplayWithTypingEffectMultiLine(privacyContent);
            Console.ResetColor();
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
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            string[] browsingContent = new string[]
            {
                $"┌─ SECURE BROWSING EDUCATION FOR {_userName.ToUpper()} ─────┐",
                "│ Safe browsing habits protect you from malware,",
                "│ phishing, and other online threats.",
                "│",
                "│ SECURE BROWSING PRACTICES:",
                "│ • Look for HTTPS in the URL (padlock icon)",
                "│ • Keep your browser and extensions updated",
                "│ • Use reputable antivirus and anti-malware software",
                "│ • Disable plugins you don't actively use",
                "│ • Be cautious with file downloads",
                "│",
                "│ ADVANCED MEASURES:",
                "│ • Use browser privacy modes for sensitive activities",
                "│ • Install browser extensions for tracking prevention",
                "│ • Use DNS filtering to block malicious sites",
                "│ • Consider using a privacy-focused browser",
                "└─────────────────────────────────────────────────────────┘"
            };
            
            UIHelper.DisplayWithTypingEffectMultiLine(browsingContent);
            Console.ResetColor();
            UIHelper.AutoScroll();
        }
    }
}
