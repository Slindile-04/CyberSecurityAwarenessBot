using System.Collections.Generic;

/// <summary>
/// Specifies the type of a quiz question.
/// </summary>
/// <remarks>
/// Used to determine how the question is displayed and how answers are validated.
/// </remarks>
///
/// <summary>
/// A multiple‑choice question with several predefined options (A, B, C, D...).
/// </summary>
///
/// <summary>
/// A true/false question, presented with two options (True / False).
/// </summary>
///
/// <summary>
/// Represents a single question in the cybersecurity quiz.
/// Contains all necessary data to display the question, validate the user's answer,
/// and provide educational feedback.
/// </summary>
///
/// <summary>
/// The cybersecurity topic this question belongs to (e.g., "Phishing", "Passwords").
/// </summary>
///
/// <summary>
/// The type of question – either MultipleChoice or TrueFalse.
/// Determines how the question is formatted and how answers are validated.
/// </summary>
///
/// <summary>
/// The main text of the question (what the user reads and responds to).
/// </summary>
///
/// <summary>
/// The list of answer choices for multiple‑choice questions.
/// For true/false questions this list is null (the options are hardcoded as "True"/"False").
/// </summary>
///
/// <summary>
/// The correct answer as a string.
/// For multiple‑choice: typically "A", "B", "C", or "D".
/// For true/false: "True" or "False" (case‑insensitive when comparing).
/// </summary>
///
/// <summary>
/// An explanation shown after the user answers, reinforcing the correct concept.
/// Helps turn the quiz into a learning experience.
/// </summary>
///
/// <summary>
/// Creates a new quiz question with all required properties.
/// </summary>
/// <param name="topic">Cybersecurity topic (e.g., "Phishing").</param>
/// <param name="type">QuestionType.MultipleChoice or QuestionType.TrueFalse.</param>
/// <param name="questionText">The question text to display.</param>
/// <param name="options">List of answer choices (null for TrueFalse).</param>
/// <param name="correctAnswer">The correct answer string.</param>
/// <param name="explanation">Educational explanation for the answer.</param>

namespace CyberSecurityAwarenessBot
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class QuizQuestion
    {
        public string Topic { get; set; }
        public QuestionType Type { get; set; }
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } // Null for TrueFalse
        public string CorrectAnswer { get; set; }  // "A", "B", "True", "False"
        public string Explanation { get; set; }

        public QuizQuestion(string topic, QuestionType type, string questionText,
                           List<string> options, string correctAnswer, string explanation)
        {
            Topic = topic;
            Type = type;
            QuestionText = questionText;
            Options = options;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
        }
    }
}