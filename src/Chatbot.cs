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
        /// </summary>
        private void InteractionLoop()
        {
            while (_isRunning)
            {
                DisplayMenuOptions();
                Console.Write($"\n{_userName}, what would you like to know? ");

                string? userInput = _inputHelper.GetValidInput();

                // Handle empty input gracefully
                if (string.IsNullOrWhiteSpace(userInput))
                {
                    Console.WriteLine($"I didn't catch that, {_userName}. Please enter something or type 'help' for options.");
                    continue;
                }

                // Process the user's input
                HandleUserInput(userInput);
            }
        }

        /// <summary>
        /// DisplayMenuOptions() - Shows the available topics and commands in a user-friendly format.
        /// Provides both numeric menu options and text-based keywords for conversational input.
        /// </summary>
        private void DisplayMenuOptions()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║             Cybersecurity Topics (Numeric Menu)            ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  1. Phishing Attacks   | 2. Strong Passwords               ║");
            Console.WriteLine("║  3. Two-Factor Auth    | 4. Data Privacy                   ║");
            Console.WriteLine("║  5. Secure Browsing    | 0. Exit                           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ Or just ask naturally:                                     ║");
            Console.WriteLine("║ • \"How are you?\"  • \"What can you help with?\"              ║");
            Console.WriteLine("║ • \"Tell me about phishing\"  • \"help\"                       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
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
                    Console.WriteLine($"\nTake care, {_userName}! Remember to stay secure online! 🔒");
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
            Console.WriteLine($"\nI'm not sure how to respond to that, {_userName}. Try asking about one of the topics above,");
            Console.WriteLine("or type 'help' for a list of available options.");
        }

        /// <summary>
        /// RespondToGreeting() - Responds conversationally to greetings like "How are you?"
        /// </summary>
        private void RespondToGreeting()
        {
            Console.WriteLine($"\n✨ I'm functioning perfectly, {_userName}! Thank you for asking.");
            Console.WriteLine("I'm here to help you learn about cybersecurity and stay safe online.");
        }

        /// <summary>
        /// RespondToPurpose() - Explains the chatbot's purpose when asked.
        /// </summary>
        private void RespondToPurpose()
        {
            Console.WriteLine($"\n🎯 My purpose, {_userName}, is to educate you about cybersecurity best practices.");
            Console.WriteLine("I can teach you about phishing, passwords, two-factor authentication, data privacy, and secure browsing.");
            Console.WriteLine("Together, we'll help protect South African citizens online!");
        }

        /// <summary>
        /// DisplayHelpMessage() - Shows available topics and commands when user asks for help.
        /// </summary>
        private void DisplayHelpMessage()
        {
            Console.WriteLine($"\n📚 Here's how I can help, {_userName}:");
            Console.WriteLine("\n🔒 Cybersecurity Topics:");
            Console.WriteLine("   • Type '1' or 'phishing' - Learn about phishing attacks");
            Console.WriteLine("   • Type '2' or 'password' - Learn about strong passwords");
            Console.WriteLine("   • Type '3' or '2fa' - Learn about two-factor authentication");
            Console.WriteLine("   • Type '4' or 'privacy' - Learn about data privacy");
            Console.WriteLine("   • Type '5' or 'browsing' - Learn about secure browsing");
            Console.WriteLine("\n💬 Conversational:");
            Console.WriteLine("   • Ask 'How are you?' or 'What's your purpose?'");
            Console.WriteLine("   • Type 'exit' or 'quit' to leave");
        }

        /// <summary>
        /// DisplayWelcomeMessage() - Displays initial welcome and tries to play audio greeting.
        /// </summary>
        private void DisplayWelcomeMessage()
        {
            Console.WriteLine($"\n🎓 Welcome to the Cybersecurity Awareness Bot, {_userName}!");
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
        /// </summary>
        private void ProvidePhishingEducation()
        {
            Console.WriteLine($"\n┌─ PHISHING ATTACKS EDUCATION FOR {_userName.ToUpper()} ─┐");
            Console.WriteLine("│ Phishing is a social engineering attack where attackers");
            Console.WriteLine("│ impersonate trusted entities to steal sensitive information.");
            Console.WriteLine("│");
            Console.WriteLine("│ RED FLAGS:");
            Console.WriteLine("│ • Urgent requests for passwords or personal information");
            Console.WriteLine("│ • Suspicious sender email addresses");
            Console.WriteLine("│ • Links that don't match the displayed text");
            Console.WriteLine("│ • Grammar and spelling errors");
            Console.WriteLine("│");
            Console.WriteLine("│ PREVENTION:");
            Console.WriteLine("│ • Never click links in unexpected emails");
            Console.WriteLine("│ • Verify sender identity independently");
            Console.WriteLine("│ • Use email filtering and anti-phishing tools");
            Console.WriteLine("│ • Report suspicious emails to your IT department");
            Console.WriteLine("└───────────────────────────────┘");
        }

        /// <summary>
        /// ProvidePasswordEducation() - Educates about creating strong passwords with personalized greeting.
        /// </summary>
        private void ProvidePasswordEducation()
        {
            Console.WriteLine($"\n┌─ STRONG PASSWORD EDUCATION FOR {_userName.ToUpper()} ─┐");
            Console.WriteLine("│ A strong password is your first defense against");
            Console.WriteLine("│ unauthorized access to your accounts.");
            Console.WriteLine("│");
            Console.WriteLine("│ STRONG PASSWORD CRITERIA:");
            Console.WriteLine("│ • At least 12 characters long");
            Console.WriteLine("│ • Mix of uppercase and lowercase letters");
            Console.WriteLine("│ • Include numbers and special characters (!@#$%^&*)");
            Console.WriteLine("│ • Avoid common words, dates, and personal information");
            Console.WriteLine("│ • Use unique passwords for each account");
            Console.WriteLine("│");
            Console.WriteLine("│ BEST PRACTICES:");
            Console.WriteLine("│ • Use a password manager to securely store passwords");
            Console.WriteLine("│ • Change passwords if compromised");
            Console.WriteLine("│ • Never share your password with anyone");
            Console.WriteLine("└───────────────────────────────┘");
        }

        /// <summary>
        /// Provide2FAEducation() - Educates about two-factor authentication with personalized greeting.
        /// </summary>
        private void Provide2FAEducation()
        {
            Console.WriteLine($"\n┌─ TWO-FACTOR AUTHENTICATION FOR {_userName.ToUpper()} ─┐");
            Console.WriteLine("│ 2FA adds an extra layer of security by requiring");
            Console.WriteLine("│ a second verification method in addition to your password.");
            Console.WriteLine("│");
            Console.WriteLine("│ COMMON 2FA METHODS:");
            Console.WriteLine("│ • SMS codes sent to your phone");
            Console.WriteLine("│ • Authenticator apps (Google Authenticator, Microsoft Authenticator)");
            Console.WriteLine("│ • Hardware security keys (FIDO2)");
            Console.WriteLine("│ • Biometric verification (fingerprint, face recognition)");
            Console.WriteLine("│");
            Console.WriteLine("│ RECOMMENDATIONS:");
            Console.WriteLine("│ • Enable 2FA on all critical accounts");
            Console.WriteLine("│ • Prefer authenticator apps over SMS when possible");
            Console.WriteLine("│ • Store backup codes in a secure location");
            Console.WriteLine("└───────────────────────────────┘");
        }

        /// <summary>
        /// ProvidePrivacyEducation() - Educates about data privacy with personalized greeting.
        /// </summary>
        private void ProvidePrivacyEducation()
        {
            Console.WriteLine($"\n┌─ DATA PRIVACY EDUCATION FOR {_userName.ToUpper()} ─┐");
            Console.WriteLine("│ Your personal data is valuable. Protect it by being");
            Console.WriteLine("│ mindful of what information you share online.");
            Console.WriteLine("│");
            Console.WriteLine("│ PRIVACY RISKS:");
            Console.WriteLine("│ • Identity theft");
            Console.WriteLine("│ • Data breaches");
            Console.WriteLine("│ • Targeted advertisements and manipulation");
            Console.WriteLine("│ • Financial fraud");
            Console.WriteLine("│");
            Console.WriteLine("│ PROTECTION MEASURES:");
            Console.WriteLine("│ • Review privacy settings on social media accounts");
            Console.WriteLine("│ • Limit the information you share publicly");
            Console.WriteLine("│ • Use VPNs on public Wi-Fi networks");
            Console.WriteLine("│ • Regularly check your credit reports");
            Console.WriteLine("│ • Read privacy policies before using new services");
            Console.WriteLine("└───────────────────────────────┘");
        }

        /// <summary>
        /// ProvideBrowsingEducation() - Educates about secure browsing with personalized greeting.
        /// </summary>
        private void ProvideBrowsingEducation()
        {
            Console.WriteLine($"\n┌─ SECURE BROWSING EDUCATION FOR {_userName.ToUpper()} ─┐");
            Console.WriteLine("│ Safe browsing habits protect you from malware,");
            Console.WriteLine("│ phishing, and other online threats.");
            Console.WriteLine("│");
            Console.WriteLine("│ SECURE BROWSING PRACTICES:");
            Console.WriteLine("│ • Look for HTTPS in the URL (padlock icon)");
            Console.WriteLine("│ • Keep your browser and extensions updated");
            Console.WriteLine("│ • Use reputable antivirus and anti-malware software");
            Console.WriteLine("│ • Disable plugins you don't actively use");
            Console.WriteLine("│ • Be cautious with file downloads");
            Console.WriteLine("│");
            Console.WriteLine("│ ADVANCED MEASURES:");
            Console.WriteLine("│ • Use browser privacy modes for sensitive activities");
            Console.WriteLine("│ • Install browser extensions for tracking prevention");
            Console.WriteLine("│ • Use DNS filtering to block malicious sites");
            Console.WriteLine("│ • Consider using a privacy-focused browser");
            Console.WriteLine("└───────────────────────────────┘");
        }
    }
}
