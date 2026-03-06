using CyberSecurityAwarenessBot.Core;
using System.Media;

namespace CyberSecurityAwarenessBot
{
    /// <summary>
    /// Program.cs - Entry point for the Cyber Security Awareness Bot application
    /// 
    /// Responsibilities:
    /// - Display the ASCII title banner
    /// - Play welcome audio greeting
    /// - Display application greeting message
    /// - Collect user's name
    /// - Initialize and start the chatbot with the user's name
    /// 
    /// The chatbot logic itself is separated into Chatbot.cs for clean separation of concerns.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
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
            Console.WriteLine($"\nThank you for learning about cybersecurity, {userName}! Stay safe online.");
        }

        /// <summary>
        /// DisplayTitleScreen() - Displays a cybersecurity-themed ASCII banner at application startup.
        /// Shows a visually appealing globe design with the application name and motivational messages.
        /// Waits for 2 seconds before clearing the screen for readability.
        /// 
        /// NOTE: Console.Clear() Fix for IOException
        /// =========================================
        /// The Console.Clear() call was causing a System.IO.IOException with message:
        /// "The handle is invalid"
        /// 
        /// ROOT CAUSE:
        /// When output is redirected (e.g., piped to a file, output capture in testing, or
        /// certain IDE/terminal environments), the console handle becomes invalid and
        /// Console.Clear() throws an exception because it cannot interact with a physical console.
        /// 
        /// SOLUTION:
        /// We check Console.IsOutputRedirected before calling Console.Clear().
        /// - If IsOutputRedirected is FALSE: We have a real console, so Clear() is safe.
        /// - If IsOutputRedirected is TRUE: Output is redirected, so we skip Clear().
        /// 
        /// This prevents crashes while maintaining intended behavior in normal console environments.
        /// </summary>
        static void DisplayTitleScreen()
        {
            // Multi-line verbatim string containing the ASCII globe banner
            string titleScreen = @"===========================================================
        CYBER SECURITY AWARENESS BOT
     Protecting South African Citizens Online
===========================================================




               ,,ggddY""""Ybbgg,,
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

            Console.WriteLine(titleScreen);
            Thread.Sleep(2000);
            
            // Safe console clear: Only clear if output is not redirected
            // This prevents System.IO.IOException when running in redirected/piped environments
            if (!Console.IsOutputRedirected)
            {
                Console.Clear();
            }
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
        /// DisplayGreetingMessage() - Displays a welcoming message to the user.
        /// Explains the purpose of the chatbot and sets a friendly, educational tone.
        /// </summary>
        static void DisplayGreetingMessage()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         Welcome to the Cybersecurity Awareness Bot         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("Welcome if you're new, welcome back if you're not.");
            Console.WriteLine("This is the Cybersecurity Awareness Bot, protecting South African citizens online.\n");
        }

        /// <summary>
        /// GetUserName() - Prompts the user to enter their name.
        /// The name is collected and stored to personalize all future responses.
        /// 
        /// Ensures the name is not empty and validates input before returning.
        /// </summary>
        static string GetUserName()
        {
            Console.Write("Before we begin, what is your name? ");
            string? name = Console.ReadLine();

            // Ensure the user provides a valid name (not empty or whitespace)
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Please enter your name: ");
                name = Console.ReadLine();
            }

            Console.WriteLine($"\nGreat to meet you, {name}! Let's learn about cybersecurity together.\n");
            return name.Trim();
        }
    }
}
