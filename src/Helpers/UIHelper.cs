using System;

namespace CyberSecurityAwarenessBot.Helpers
{
    /// <summary>
    /// UIHelper.cs - Provides enhanced user interface helpers for better chatbot experience
    /// 
    /// Responsibilities:
    /// - Display typing indicators to inform user that bot is processing
    /// - Implement typing animation for bot responses
    /// - Provide colored text output for visual consistency
    /// - Animate ASCII art displays
    /// - Display startup loading sequences
    /// - Improve overall user experience with visual feedback
    /// 
    /// Features:
    /// - Color-coded messages (Cyan for titles, Green for ASCII, Yellow for bot, Red for errors)
    /// - Animated ASCII art line-by-line display
    /// - Startup loading sequence with visual feedback
    /// - Reusable helper methods for consistent UI presentation
    /// 
    /// Benefits:
    /// - Makes the bot feel more conversational and natural
    /// - Provides visual feedback during response generation
    /// - Creates a more engaging and professional user experience
    /// </summary>
    public class UIHelper
    {
        /// <summary>
        /// TypingSpeed in milliseconds between character display.
        /// Adjustable to make typing appear faster or slower.
    /// 10-15ms provides smooth, fast animation while remaining readable.
    /// </summary>
    private const int TypingSpeed = 12; // milliseconds per character (faster, smoother animation)
        
        /// <summary>
        /// ASCII animation delay in milliseconds between each line.
        /// Creates a smooth, readable animation effect for ASCII art.
        /// </summary>
        private const int AsciiLineDelay = 30; // milliseconds between ASCII art lines

        /// <summary>
        /// PrintColoredLine() - Prints a message with specified color and resets the console color.
        /// 
        /// Purpose:
        /// - Provides a reusable method for consistent color formatting throughout the application
        /// - Ensures proper color reset to prevent color bleeding to subsequent text
        /// - Simplifies color output without boilerplate code
        /// 
        /// Parameters:
        /// - message: The text to display
        /// - color: The console color to use for the text
        /// - addNewLine: Whether to add a newline after the message (default: true)
        /// </summary>
        public static void PrintColoredLine(string message, ConsoleColor color, bool addNewLine = true)
        {
            Console.ForegroundColor = color;
            if (addNewLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ResetColor();
        }

        /// <summary>
        /// DisplayAnimatedAscii() - Displays ASCII art line-by-line with animation effect.
        /// 
        /// Purpose:
        /// - Creates a visual animation effect for ASCII art displays
        /// - Makes the startup more engaging and interactive
        /// - Each line appears with a small delay for smooth animation
        /// 
        /// Parameters:
        /// - asciiArt: The complete ASCII art string to animate
        /// - color: The console color for the ASCII art (default: Green)
        /// 
        /// Behavior:
        /// - Splits the ASCII art by newlines
        /// - Displays each line individually with a delay between them
        /// - Maintains the original formatting and spacing
        /// </summary>
        public static void DisplayAnimatedAscii(string asciiArt, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            string[] lines = asciiArt.Split('\n');
            
            foreach (var line in lines)
            {
                Console.WriteLine(line);
                System.Threading.Thread.Sleep(AsciiLineDelay);
            }
            
            Console.ResetColor();
        }

        /// <summary>
        /// DisplayLoadingSequence() - Shows a startup loading sequence with animated messages.
        /// 
        /// Purpose:
        /// - Creates an immersive startup experience that simulates system initialization
        /// - Sets the tone for a professional cybersecurity application
        /// - Provides visual feedback during startup process
        /// 
        /// Messages:
        /// - "Initializing CyberSecurity Awareness Bot..."
        /// - "Loading security modules..."
        /// - "Establishing secure environment..."
        /// 
        /// Behavior:
        /// - Each message appears with a delay to suggest system processing
        /// - Uses DarkYellow color for loading messages
        /// - Final message shows application ready status
        /// </summary>
        public static void DisplayLoadingSequence()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            
            Console.Write("Initializing CyberSecurity Awareness Bot");
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(400);
                Console.Write(".");
            }
            Console.WriteLine();
            System.Threading.Thread.Sleep(300);

            Console.Write("Loading security modules");
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(400);
                Console.Write(".");
            }
            Console.WriteLine();
            System.Threading.Thread.Sleep(300);

            Console.Write("Establishing secure environment");
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(400);
                Console.Write(".");
            }
            Console.WriteLine();
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("✅ System ready!\n");
            Console.ResetColor();
        }

        /// Creates a brief pause with animated dots to indicate the bot is processing
        /// 
        /// Workflow:
        /// 1. Display "Bot is typing" indicator on its own line
        /// 2. Animate dots for visual feedback
        /// 3. Clear the entire indicator line after approximately 1 second
        /// 4. Response appears on a fresh line without any overlap
        /// </summary>
        public static void DisplayTypingIndicator()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("🤖 Bot is typing");
            Console.ResetColor();
            
            // Animate dots for visual feedback (~1 second total)
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(333);
                Console.Write(".");
            }
            
            // Clear the entire typing indicator line and move to next line
            Console.Write("\r");
            Console.Write(new string(' ', 40)); // Clear the line completely
            Console.Write("\r\n"); // Move to next line for response
        }

        /// <summary>
        /// DisplayWithTypingEffect() - Displays text with character-by-character typing animation
        /// 
        /// Purpose:
        /// - Creates a natural, conversational feel to bot responses
        /// - Makes the bot appear as though it's "thinking" and "typing"
        /// - Engages the user with animated text display
        /// 
        /// Parameters:
        /// - text: The message to display with typing effect
        /// 
        /// Behavior:
        /// - Each character appears individually with a small delay
        /// - Respects newlines and maintains text formatting
        /// - Typing speed is consistent and configurable
        /// 
        /// Example:
        /// DisplayWithTypingEffect("Hello, this will appear character by character!");
        /// </summary>
        public static void DisplayWithTypingEffect(string text)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                System.Threading.Thread.Sleep(TypingSpeed);
            }
            Console.WriteLine(); // Add newline after the text
        }

        /// <summary>
        /// DisplayWithTypingEffectMultiLine() - Displays multi-line text blocks with typing animation
        /// 
        /// Purpose:
        /// - Displays longer responses (like education content) with typing effect
        /// - Preserves formatting (boxes, borders, etc.) while animating
        /// - Ideal for structured educational content
        /// 
        /// Parameters:
        /// - lines: Array of lines to display sequentially with typing effect
        /// 
        /// Behavior:
        /// - Each line is displayed with typing animation
        /// - Lines are displayed sequentially to build the complete response
        /// - Preserves the original formatting and structure
        /// </summary>
        public static void DisplayWithTypingEffectMultiLine(string[] lines)
        {
            foreach (var line in lines)
            {
                DisplayWithTypingEffect(line);
            }
        }

        /// <summary>
        /// AutoScroll() - Ensures the chat window scrolls to show the latest message
        /// 
        /// In a console application, auto-scrolling happens naturally as new lines
        /// are added. This method serves as a placeholder for consistency with UI/UX
        /// frameworks that require explicit scroll positioning.
        /// 
        /// Note: Console.WriteLine() automatically scrolls the buffer to the bottom.
        /// This method is included for:
        /// - Future GUI implementation compatibility
        /// - Clear indication of auto-scroll intent in code
        /// - Potential enhancement for buffered output scenarios
        /// </summary>
        public static void AutoScroll()
        {
            // Console naturally auto-scrolls as new lines are written
            // This is a conceptual placeholder for GUI implementations
            // In a WPF/WinForms UI, this would scroll the text control to bottom
        }
    }
}
