using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityAwarenessBot.Core
{
    /// <summary>
    /// TipRepository.cs - Centralized storage for cybersecurity tips
    /// 
    /// Responsibilities:
    /// - Store random cybersecurity tips for each topic
    /// - Provide methods to retrieve random tips
    /// - Support all topics in a scalable and maintainable way
    /// - Keep tips separate from chatbot logic
    /// 
    /// Design:
    /// - Uses Dictionary<string, List<string>> for topic-based organization
    /// - Easy to extend with new topics
    /// - Returns random tips for variety in responses
    /// </summary>
    public class TipRepository
    {
        private readonly Dictionary<string, List<string>> _topicTips;
        private readonly Random _random;

        /// <summary>
        /// Constructor - Initializes the tip repository with all topics and their tips.
        /// </summary>
        public TipRepository()
        {
            _random = new Random();
            _topicTips = InitializeTips();
        }

        /// <summary>
        /// InitializeTips() - Populates the dictionary with tips for all cybersecurity topics.
        /// Each topic has at least 5 different tips for variety.
        /// </summary>
        private Dictionary<string, List<string>> InitializeTips()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "phishing", new List<string>
                    {
                        "🎣 Tip: Always hover over links in emails before clicking to verify the actual URL destination.",
                        "🎣 Tip: Be suspicious of urgent requests for passwords or personal information in emails.",
                        "🎣 Tip: Check the sender's email address carefully—attackers often use addresses similar to legitimate ones.",
                        "🎣 Tip: Legitimate companies never ask for passwords via email. If unsure, call the company directly.",
                        "🎣 Tip: Look for poor grammar and spelling errors in emails—a common sign of phishing attempts.",
                        "🎣 Tip: Enable email filtering in your mail provider to catch suspicious messages automatically.",
                        "🎣 Tip: Report suspicious emails to your IT department rather than replying to the sender."
                    }
                },
                {
                    "passwords", new List<string>
                    {
                        "🔐 Tip: Use at least 12 characters combining uppercase, lowercase, numbers, and special characters.",
                        "🔐 Tip: Never reuse passwords across multiple accounts—if one site is breached, all accounts are at risk.",
                        "🔐 Tip: Avoid using personal information like birthdays, pet names, or sequential numbers in passwords.",
                        "🔐 Tip: Change your passwords immediately if you suspect they've been compromised.",
                        "🔐 Tip: Use a password manager to generate and store complex passwords securely.",
                        "🔐 Tip: Enable password expiration policies in work environments (every 90 days is recommended).",
                        "🔐 Tip: Never write down passwords—if you must, store them in an encrypted, locked location."
                    }
                },
                {
                    "2fa", new List<string>
                    {
                        "🔑 Tip: Authenticator apps (Google Authenticator, Microsoft Authenticator) are more secure than SMS.",
                        "🔑 Tip: Save backup codes in a secure location in case you lose access to your 2FA device.",
                        "🔑 Tip: Hardware security keys (FIDO2) provide the strongest 2FA protection available.",
                        "🔑 Tip: Enable 2FA on your most critical accounts first: email, banking, and password manager.",
                        "🔑 Tip: SMS-based 2FA is better than nothing, but phone numbers can be hijacked by attackers.",
                        "🔑 Tip: Never share your 2FA codes with anyone, even if they claim to be support staff.",
                        "🔑 Tip: Use biometric 2FA (fingerprint/face recognition) when available on your devices."
                    }
                },
                {
                    "privacy", new List<string>
                    {
                        "🔓 Tip: Review your social media privacy settings regularly to limit who can see your personal information.",
                        "🔓 Tip: Don't overshare location data—turn off location services for apps that don't need it.",
                        "🔓 Tip: Be cautious with public Wi-Fi—avoid accessing sensitive information like banking apps.",
                        "🔓 Tip: Check data collection policies before signing up for new services or downloading apps.",
                        "🔓 Tip: Use privacy browsers or add-ons that block tracking cookies and third-party trackers.",
                        "🔓 Tip: Regularly review and delete old accounts you no longer use to reduce your digital footprint.",
                        "🔓 Tip: Monitor your credit reports annually for suspicious activity using free services."
                    }
                },
                {
                    "browsing", new List<string>
                    {
                        "🌐 Tip: Always look for the padlock icon and 'HTTPS' in the URL before entering sensitive data.",
                        "🌐 Tip: Keep your browser and all extensions updated to patch security vulnerabilities.",
                        "🌐 Tip: Use browser extensions like uBlock Origin or Privacy Badger to block malicious trackers.",
                        "🌐 Tip: Disable plugins like Flash and Java if you don't actively use them.",
                        "🌐 Tip: Don't download files from untrusted websites—malware often disguises itself as legitimate software.",
                        "🌐 Tip: Clear your browser cache and cookies regularly to remove tracking data.",
                        "🌐 Tip: Use private/incognito mode when accessing sensitive information or making purchases."
                    }
                },
                {
                    "ransomware", new List<string>
                    {
                        "💾 Tip: Follow the 3-2-1 backup rule: 3 copies, 2 different media types, 1 off-site location.",
                        "💾 Tip: Never pay a ransom—there's no guarantee you'll recover your files.",
                        "💾 Tip: Disconnect infected devices from the network immediately to prevent spread.",
                        "💾 Tip: Test your backup and recovery process regularly to ensure it actually works.",
                        "💾 Tip: Use segmentation to isolate critical systems from the rest of your network.",
                        "💾 Tip: Enable file versioning so you can recover previous versions of encrypted files.",
                        "💾 Tip: Be cautious with email attachments and downloads—ransomware often enters through these vectors."
                    }
                },
                {
                    "social engineering", new List<string>
                    {
                        "🎭 Tip: Always verify requests through an independent channel before providing sensitive information.",
                        "🎭 Tip: Be suspicious of urgency in requests—attackers often create artificial time pressure.",
                        "🎭 Tip: Never share password reset codes or OTPs (one-time passwords) over the phone.",
                        "🎭 Tip: Educate yourself on what your company's IT department typically requests from employees.",
                        "🎭 Tip: Use strong authentication to prevent unauthorized account access from social engineering.",
                        "🎭 Tip: Report suspicious communication to your security team—they can help identify attacks.",
                        "🎭 Tip: Remember: legitimate support will never threaten you with account closure or legal action."
                    }
                },
                {
                    "patch management", new List<string>
                    {
                        "🔧 Tip: Enable automatic updates on your operating system and devices whenever possible.",
                        "🔧 Tip: Prioritize security updates over feature updates—they fix critical vulnerabilities.",
                        "🔧 Tip: Update your third-party applications (browsers, Office, antivirus) regularly.",
                        "🔧 Tip: Don't ignore or postpone update notifications—vulnerabilities can be exploited immediately.",
                        "🔧 Tip: Regularly check for BIOS and firmware updates for your computer and devices.",
                        "🔧 Tip: Set a regular day each week to check for and install updates across all your devices.",
                        "🔧 Tip: If using older devices, remove unsupported software and consider upgrading to a newer OS."
                    }
                },
                {
                    "wifi", new List<string>
                    {
                        "📶 Tip: Always use a VPN on public Wi-Fi to encrypt all traffic between you and your device.",
                        "📶 Tip: Don't connect to unsecured Wi-Fi networks—look for networks that require passwords.",
                        "📶 Tip: Verify the official Wi-Fi network name with staff before connecting at cafes or airports.",
                        "📶 Tip: Disable auto-connect features so your device doesn't automatically join unknown networks.",
                        "📶 Tip: Turn off file sharing, AirDrop, and Bluetooth when using public Wi-Fi.",
                        "📶 Tip: Avoid logging into financial accounts on public Wi-Fi—wait until you're on a secure network.",
                        "📶 Tip: Use your phone's hotspot instead of public Wi-Fi when possible for mobile device internet."
                    }
                },
                {
                    "password manager", new List<string>
                    {
                        "🧠 Tip: Choose reputable password managers like Bitwarden, 1Password, or built-in options from Apple/Google.",
                        "🧠 Tip: Enable 2FA on your password manager account itself for maximum security.",
                        "🧠 Tip: Use a strong, memorable master password—you'll only need to remember this one.",
                        "🧠 Tip: Let your password manager generate passwords (e.g., &92#kLp$8mNq@4) instead of creating them yourself.",
                        "🧠 Tip: Regularly back up your encrypted password vault to ensure you never lose access.",
                        "🧠 Tip: Use zero-knowledge password managers that can't access your data, even if they're hacked.",
                        "🧠 Tip: Consider using autofill features in your password manager for added protection against phishing."
                    }
                }
            };
        }

        /// <summary>
        /// GetRandomTip() - Retrieves a random tip for a given topic.
        /// 
        /// Parameters:
        /// - topic: The cybersecurity topic (e.g., "phishing", "passwords")
        /// 
        /// Returns:
        /// - A random tip string if the topic exists
        /// - A helpful message if the topic doesn't exist
        /// </summary>
        public string GetRandomTip(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return null;
            }

            if (_topicTips.TryGetValue(topic, out var tips))
            {
                if (tips.Count == 0)
                {
                    return null;
                }

                int randomIndex = _random.Next(tips.Count);
                return tips[randomIndex];
            }

            return null;
        }

        /// <summary>
        /// GetAllTopics() - Returns a list of all available topics.
        /// Useful for validation and display purposes.
        /// </summary>
        public List<string> GetAllTopics()
        {
            return _topicTips.Keys.ToList();
        }

        /// <summary>
        /// HasTopic() - Checks if a topic exists in the repository.
        /// </summary>
        public bool HasTopic(string topic)
        {
            return _topicTips.ContainsKey(topic);
        }

        /// <summary>
        /// GetTipCount() - Returns the number of tips available for a topic.
        /// Useful for tracking how many tips have been given.
        /// </summary>
        public int GetTipCount(string topic)
        {
            if (_topicTips.TryGetValue(topic, out var tips))
            {
                return tips.Count;
            }
            return 0;
        }
    }
}
