namespace CyberSecurityAwarenessBot.Helpers
{
    /// <summary>
    /// InputHelper.cs - Handles user input validation and processing
    /// </summary>
    public class InputHelper
    {
        /// <summary>
        /// Gets valid user input from the console
        /// </summary>
        /// <returns>User input as a string, or null if input is invalid</returns>
        public string? GetValidInput()
        {
            string? input = Console.ReadLine();
            return input;
        }
    }
}
