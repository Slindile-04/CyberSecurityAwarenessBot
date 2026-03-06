using System;

namespace CyberSecurityAwarenessBot.Helpers
{
    /// <summary>
    /// UIHelper.cs - Provides enhanced user interface helpers for better chatbot experience
    /// 
    /// Responsibilities:
    /// - Display typing indicators to inform user that bot is processing
    /// - Implement typing animation for bot responses
    /// - Improve overall user experience with visual feedback
    /// 
    /// Benefits:
    /// - Makes the bot feel more conversational and natural
    /// - Provides visual feedback during response generation
    /// - Creates a more engaging user experience
    /// </summary>
    public class UIHelper
    {
        /// <summary>
        /// TypingSpeed in milliseconds between character display.
        /// Adjustable to make typing appear faster or slower.
    /// 10-15ms provides smooth, fast animation while remaining readable.
    /// </summary>
    private const int TypingSpeed = 12; // milliseconds per character (faster, smoother animation)
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
