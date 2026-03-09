using CyberSecurityAwarenessBot.Core;
using CyberSecurityAwarenessBot.Helpers;
using System.Media;

namespace CyberSecurityAwarenessBot
{
    /// <summary>
    /// Program.cs - Entry point for the Cyber Security Awareness Bot application
    /// 
    /// Responsibilities:
    /// - Display loading sequence for immersive startup experience
    /// - Display the ASCII title banner with animation
    /// - Play welcome audio greeting
    /// - Display application greeting message
    /// - Collect user's name
    /// - Initialize and start the chatbot with the user's name
    /// 
    /// Enhanced Features:
    /// - Loading sequence with animated initialization messages
    /// - Animated ASCII art display (line-by-line animation)
    /// - Color-coded console output for better visual appeal
    /// - Professional startup experience
    /// 
    /// The chatbot logic itself is separated into Chatbot.cs for clean separation of concerns.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Enable UTF-8 encoding to support Unicode emoji display in the terminal
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Step 0: Display the startup loading sequence
            UIHelper.DisplayLoadingSequence();

            // Step 1: Display the ASCII title banner at startup
            DisplayTitleScreen();

            // Step 2: Play the welcome audio greeting
            PlayWelcomeAudio();

            // Step 3: Display the greeting message
            DisplayGreetingMessage();

            // Step 4: Collect the user's name
            string userName = GetUserName();

            // Step 5: Initialize the chatbot with audio folder path and user's name
            string audioPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Audio");
            CyberSecurityAwarenessBot.Core.CyberSecurityAwarenessBot bot = new CyberSecurityAwarenessBot.Core.CyberSecurityAwarenessBot(audioPath, userName);

            // Step 6: Start the chatbot interaction loop
            bot.Start();

            // Step 7: Display farewell message
            UIHelper.PrintColoredLine($"\nThank you for learning about cybersecurity, {userName}! Stay safe online.", ConsoleColor.Cyan);
        }

        /// <summary>
        /// DisplayHeader() - Displays the ASCII art header that persists throughout the session.
        /// This method can be called whenever the header needs to be shown.
        /// Creates a professional cybersecurity-themed banner at the top of the console.
        /// </summary>
        static void DisplayHeader()
        {
            string asciiArt = @"               ,,ggddY""""Ybbgg,,
          ,agd888b,_ ""Y8, ___`""Ybga,
       ,gdP""""88888888baa,.""8b    ""888g,
     ,dP""     ]888888888P'  ""Y     `888Yb,
   ,dP""      ,88888888P""  db,       ""8P""Yb,
  ,8""       ,888888888b, d8888a           ""8,
 ,8'        d88888888888,88P""' a,          `8,
,8'         88888888888888PP""  """"           `8,
d'          I88888888888P""                   `b
8           `8""88P""""Y8P'                      8
8            Y 8[  _ ""                        8
8              ""Y8d8b  ""Y a                   8
8                 `""""8d,   __                 8
Y,                    `""8bd888b,             ,P
`8,                     ,d8888888baaa       ,8'
 `8,                    888888888888'      ,8'
  `8a                   ""8888888888I      a8'
   `Yba                  `Y8888888P'    adP'
     ""Yba                 `888888P'   adY""
       `""Yba,             d8888P' ,adP""""  Stay Alert.
          `""Y8baa,      ,d888P,ad8P""""     Stay Secure.
               ``""""YYba8888P""""''         Stay Informed.";

            // Display ASCII art header with animation and green color
            UIHelper.DisplayAnimatedAscii(asciiArt, ConsoleColor.Green);
            Console.WriteLine(); // Add spacing after header
        }

        /// <summary>
        /// DisplayTitleScreen() - Displays the cybersecurity-themed ASCII banner at startup.
        /// Shows the intro header and ASCII art that persists throughout the session.
        /// </summary>
        static void DisplayTitleScreen()
        {
            // Display the intro header section with green color to match ASCII art
            UIHelper.PrintColoredLine("===========================================================", ConsoleColor.Green);
            UIHelper.PrintColoredLine("        CYBER SECURITY AWARENESS BOT", ConsoleColor.Green);
            UIHelper.PrintColoredLine("     Protecting South African Citizens Online", ConsoleColor.Green);
            UIHelper.PrintColoredLine("===========================================================\n", ConsoleColor.Green);

            // Display the persistent header with ASCII art
            DisplayHeader();

            // Brief pause to show the splash screen effect
            Thread.Sleep(1000);
        }

        /// <summary>
        /// PlayWelcomeAudio() - Plays a welcome audio file using System.Media.SoundPlayer.
        /// 
        /// What does SoundPlayer do?
        /// - SoundPlayer is a built-in .NET class that plays WAV and other audio formats
        /// - It's part of the System.Media namespace and requires no external dependencies
        /// - It's simple, lightweight, and perfect for console applications
        /// - It directly supports the Windows API for reliable audio playback
        /// 
        /// Why WAV instead of MP3?
        /// - WAV is an uncompressed audio format that provides superior sound quality
        /// - SoundPlayer has native support for WAV files out of the box
        /// - MP3 requires additional codec support and external libraries
        /// - WAV files are ideal for short audio clips like welcome greetings
        /// - WAV ensures consistent playback across all Windows systems
        /// 
        /// Error Handling:
        /// - Try-catch block handles file not found errors gracefully
        /// - Catches any audio device issues or playback errors
        /// - Application continues normally if audio cannot be played
        /// - The user is not blocked by audio playback failures
        /// </summary>
        static void PlayWelcomeAudio()
        {
            try
            {
                // Construct the audio file path using a RELATIVE PATH for better portability
                // Benefits of relative paths:
                // - Application works on any computer without modifying hardcoded paths
                // - Supports development, testing, and production environments
                // - Enables easier deployment and distribution
                // 
                // AppDomain.CurrentDomain.BaseDirectory points to bin/Debug/net8.0/ (or Release equivalent)
                // We navigate up 3 directories to reach the project root, then into the Audio folder
                string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Audio");
                string audioFileName = "ElevenLabs_2026-03-04T09_09_24_Rachel_pre_sp100_s50_sb75_se0_b_m2.wav";
                string audioFilePath = Path.Combine(audioPath, audioFileName);
                
                // Normalize the path to resolve ".." and ensure it's in the correct format
                audioFilePath = Path.GetFullPath(audioFilePath);

                // Check if the audio file exists before attempting to play it
                if (!File.Exists(audioFilePath))
                {
                    return;
                }

                // Verify we're running on Windows (SoundPlayer is Windows-only)
                if (OperatingSystem.IsWindows())
                {
                    // Create a new SoundPlayer instance to handle WAV playback
                    // SoundPlayer is the standard .NET way to play system sounds and WAV files
#pragma warning disable CA1416 // Suppress platform-specific warning for Windows-only API
                    using (SoundPlayer player = new SoundPlayer(audioFilePath))
                    {
                        // PlaySync() plays the audio file and waits for it to complete
                        // This ensures the audio finishes before the application continues
                        player.PlaySync();
                    }
#pragma warning restore CA1416
                    Console.WriteLine("🔊 Welcome audio played successfully.\n");
                }
            }
            catch (FileNotFoundException)
            {
                // Handles the case where the audio file cannot be found
                // This prevents the application from crashing if the audio is missing
                // The application continues with the chatbot normally
            }
            catch (Exception)
            {
                // Catches any other exceptions that might occur during playback
                // Examples: audio device issues, corrupted audio file, insufficient permissions
                // The application continues normally even if audio playback fails
                // Silent failure is acceptable here as audio is supplementary to the experience
            }
        }

        /// <summary>
        /// DisplayGreetingMessage() - Displays a welcoming message to the user with color styling.
        /// Explains the purpose of the chatbot and sets a friendly, educational tone.
        /// Uses cyan color for the header to match the title screen styling.
        /// </summary>
        static void DisplayGreetingMessage()
        {
            UIHelper.PrintColoredLine("\n╔════════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("║         Welcome to the Cybersecurity Awareness Bot         ║", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("╚════════════════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);
            Console.WriteLine("Welcome if you're new, welcome back if you're not.");
            Console.WriteLine("This is the Cybersecurity Awareness Bot, protecting South African citizens online.\n");
        }

        /// <summary>
        /// GetUserName() - Prompts the user to enter their name with color styling.
        /// The name is collected and stored to personalize all future responses.
        /// 
        /// Uses white color for the user prompt to clearly distinguish user input requests.
        /// Ensures the name is not empty and validates input before returning.
        /// </summary>
        static string GetUserName()
        {
            UIHelper.PrintColoredLine("Before we begin, what is your name? ", ConsoleColor.White, false);
            string? name = Console.ReadLine();

            // Ensure the user provides a valid name (not empty or whitespace)
            while (string.IsNullOrWhiteSpace(name))
            {
                UIHelper.PrintColoredLine("Please enter your name: ", ConsoleColor.White, false);
                name = Console.ReadLine();
            }

            UIHelper.PrintColoredLine($"\nGreat to meet you, {name}! Let's learn about cybersecurity together.\n", ConsoleColor.Green);
            return name.Trim();
        }
    }
}
