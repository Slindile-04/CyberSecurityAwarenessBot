using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the cybersecurity quiz game: question bank, quiz state (score, remaining questions),
/// answer validation, and user commands (start, pause, resume, restart, score).
/// Integrates with the main bot via the HandleInput method.
/// </summary>
///
/// <summary>
/// The complete collection of 20 questions (2 per topic, 10 topics).
/// This is initialised once and never modified.
/// </summary>
///
/// <summary>
/// The queue of questions yet to be answered in the current quiz session.
/// After a question is answered, it is dequeued.
/// The order is randomised at the start of each quiz.
/// </summary>
///
/// <summary>
/// Number of correctly answered questions so far in the current quiz session.
/// Resets when a new quiz is started or restarted.
/// </summary>
///
/// <summary>
/// Total number of questions that have been answered (correct + incorrect) in the current quiz.
/// Used to show progress (e.g., "Question 5 of 20").
/// </summary>
///
/// <summary>
/// Whether a quiz is currently active (i.e., not finished, paused, or stopped).
/// When true, the manager expects the next user input to be an answer to _currentQuestion
/// or a control command (pause, restart, score, etc.).
/// </summary>
///
/// <summary>
/// The question that is currently awaiting an answer from the user.
/// Null when no quiz is active or after a quiz has finished.
/// </summary>
///
/// <summary>
/// Indicates whether the quiz is currently active (waiting for an answer or control command).
/// Exposed so that the main bot can check quiz state if needed.
/// </summary>
///
/// <summary>
/// Initialises a new quiz manager by building the static question bank
/// and resetting all state variables to their default (no active quiz).
/// </summary>
///
/// <summary>
/// Creates and returns the complete list of 20 quiz questions.
/// Two questions per topic: one multiple choice and one true/false.
/// Each question includes an explanation to educate the user.
/// </summary>
/// <returns>List of QuizQuestion objects representing the entire question bank.</returns>
///
/// <summary>
/// Resets the quiz state: shuffles the question bank, clears score and progress,
/// and deactivates the quiz. Call before starting a fresh quiz.
/// </summary>
///
/// <summary>
/// Begins a new quiz (or resumes one) by dequeuing the first question and activating the quiz.
/// Called by StartQuiz() and when resuming with remaining questions.
/// </summary>
/// <returns>The formatted question string to present to the user.</returns>
///
/// <summary>
/// Fetches the next question from the queue, updates _currentQuestion,
/// and returns it formatted. If no questions remain, finishes the quiz
/// and returns final feedback.
/// </summary>
/// <returns>The next question or final score feedback.</returns>
///
/// <summary>
/// Validates the user's answer, updates score and progress,
/// and returns appropriate feedback (correct/incorrect) followed by
/// either the next question or the final result.
/// </summary>
/// <param name="userAnswer">Raw user input (e.g., "A", "true", "C").</param>
/// <returns>Feedback string containing result explanation and next question / final score.</returns>
///
/// <summary>
/// Main entry point from the bot's ProcessMessage method.
/// Handles quiz control commands (start, resume, stop, restart, score)
/// and user answers when a quiz is active.
/// </summary>
/// <param name="userInput">Normalised lower‑case user input.</param>
/// <returns>
/// A string response to send back to the user (question, feedback, score, or error message),
/// or null if the input does not relate to the quiz (allowing the bot to handle other intents).
/// </returns>
///
/// <summary>
/// Formats a quiz question for display to the user, including the question number,
/// topic, question text, and answer options.
/// </summary>
/// <param name="q">The QuizQuestion object to format.</param>
/// <returns>A user‑ready string.</returns>
///
/// <summary>
/// Generates personalised feedback based on final quiz score.
/// Called only when all questions have been answered.
/// </summary>
/// <returns>A congratulatory or encouraging message with the score.</returns>

namespace CyberSecurityAwarenessBot
{
    public class QuizManager
    {
        private readonly List<QuizQuestion> _fullQuestionBank;
        private Queue<QuizQuestion> _remainingQuestions;
        private int _currentScore;
        private int _totalAnswered;
        private bool _isActive;
        private QuizQuestion _currentQuestion;

        public bool IsActive => _isActive;

        public QuizManager()
        {
            _fullQuestionBank = BuildQuestionBank();
            Reset();
        }

        private List<QuizQuestion> BuildQuestionBank()
        {
            var bank = new List<QuizQuestion>();

            // 1. Phishing
            bank.Add(new QuizQuestion("Phishing", QuestionType.MultipleChoice,
                "What should you do if you receive an email asking for your password?",
                new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                "C", "Reporting phishing emails helps prevent scams and protects other users."));
            bank.Add(new QuizQuestion("Phishing", QuestionType.TrueFalse,
                "Phishing attacks only happen via email.",
                null, "False", "Phishing can also occur via text messages (smishing), phone calls (vishing), and social media."));

            // 2. Passwords
            bank.Add(new QuizQuestion("Passwords", QuestionType.MultipleChoice,
                "Which of the following is the strongest password?",
                new List<string> { "A) password123", "B) Qw3rtY!9$2kLp", "C) mybirthday", "D) admin" },
                "B", "A strong password is long, random, and includes uppercase, lowercase, numbers, and symbols."));
            bank.Add(new QuizQuestion("Passwords", QuestionType.TrueFalse,
                "You should use the same password for multiple accounts to make it easier to remember.",
                null, "False", "Reusing passwords is dangerous because if one account is breached, all others become vulnerable."));

            // 3. Two-Factor Authentication (2FA)
            bank.Add(new QuizQuestion("2FA", QuestionType.MultipleChoice,
                "What is the most secure form of 2FA?",
                new List<string> { "A) SMS text message", "B) Authenticator app", "C) Hardware security key (e.g., YubiKey)", "D) Email code" },
                "C", "Hardware security keys are not vulnerable to phishing or SIM swapping, making them the most secure option."));
            bank.Add(new QuizQuestion("2FA", QuestionType.TrueFalse,
                "Two-factor authentication adds an extra layer of security even if your password is stolen.",
                null, "True", "2FA requires a second factor (something you have or are), so a stolen password alone is not enough."));

            // 4. Privacy
            bank.Add(new QuizQuestion("Privacy", QuestionType.MultipleChoice,
                "Which of the following is NOT a good practice for protecting your privacy online?",
                new List<string> { "A) Using a VPN on public Wi-Fi", "B) Posting your full address on social media", "C) Reviewing privacy settings", "D) Using encrypted messaging apps" },
                "B", "Sharing your address publicly can lead to stalking, identity theft, or burglary."));
            bank.Add(new QuizQuestion("Privacy", QuestionType.TrueFalse,
                "Incognito mode in your browser makes you completely anonymous online.",
                null, "False", "Incognito only prevents local history storage; your ISP and websites can still see your activity."));

            // 5. Secure Browsing
            bank.Add(new QuizQuestion("Secure Browsing", QuestionType.MultipleChoice,
                "What does the padlock icon in your browser address bar indicate?",
                new List<string> { "A) The website is safe to visit", "B) The connection is encrypted", "C) The website has no viruses", "D) Your identity is hidden" },
                "B", "The padlock means the connection between your browser and the website is encrypted (HTTPS)."));
            bank.Add(new QuizQuestion("Secure Browsing", QuestionType.TrueFalse,
                "You should always keep your browser and extensions updated to the latest version.",
                null, "True", "Updates include security patches that fix vulnerabilities exploited by attackers."));

            // 6. Ransomware
            bank.Add(new QuizQuestion("Ransomware", QuestionType.MultipleChoice,
                "What is the best way to protect your files from ransomware?",
                new List<string> { "A) Pay the ransom immediately", "B) Keep offline backups", "C) Disable your firewall", "D) Use the same password everywhere" },
                "B", "Offline backups (disconnected from your computer) cannot be encrypted by ransomware."));
            bank.Add(new QuizQuestion("Ransomware", QuestionType.TrueFalse,
                "Ransomware only affects large companies, not individual users.",
                null, "False", "Anyone can be a target; ransomware attacks home users, small businesses, and large organisations alike."));

            // 7. Social Engineering
            bank.Add(new QuizQuestion("Social Engineering", QuestionType.MultipleChoice,
                "Which of the following is an example of social engineering?",
                new List<string> { "A) A hacker guessing your password", "B) A caller pretending to be tech support to get your login", "C) A virus deleting your files", "D) A firewall blocking traffic" },
                "B", "Social engineering manipulates people, not technical systems. Pretexting calls are a common tactic."));
            bank.Add(new QuizQuestion("Social Engineering", QuestionType.TrueFalse,
                "Tailgating is when someone follows an authorised person into a secure area without a badge.",
                null, "True", "Tailgating exploits human courtesy – always verify strangers before letting them in."));

            // 8. Patch Management
            bank.Add(new QuizQuestion("Patch Management", QuestionType.MultipleChoice,
                "Why is it important to install software updates promptly?",
                new List<string> { "A) They fix security vulnerabilities", "B) They add new features only", "C) They slow down your computer intentionally", "D) They are never important" },
                "A", "Updates patch known security holes that attackers could use to compromise your system."));
            bank.Add(new QuizQuestion("Patch Management", QuestionType.TrueFalse,
                "Once a software reaches end-of-life (no more updates), it is still safe to use indefinitely.",
                null, "False", "Unsupported software accumulates unpatched vulnerabilities and should be replaced."));

            // 9. Public WiFi Safety
            bank.Add(new QuizQuestion("Public WiFi Safety", QuestionType.MultipleChoice,
                "What is the safest way to use public Wi‑Fi?",
                new List<string> { "A) Turn off your firewall", "B) Use a VPN", "C) Share your location", "D) Disable HTTPS" },
                "B", "A VPN encrypts your entire internet traffic, protecting you from eavesdropping on public networks."));
            bank.Add(new QuizQuestion("Public WiFi Safety", QuestionType.TrueFalse,
                "It is safe to do online banking on any public Wi‑Fi as long as you see the padlock icon.",
                null, "False", "Even with HTTPS, network‑level attacks (e.g., evil twin) can intercept traffic; avoid sensitive transactions on public Wi‑Fi."));

            // 10. Password Managers
            bank.Add(new QuizQuestion("Password Managers", QuestionType.MultipleChoice,
                "What is a key benefit of using a password manager?",
                new List<string> { "A) It remembers all your passwords so you can reuse the same one", "B) It generates and stores strong, unique passwords for each site", "C) It makes your passwords visible to anyone", "D) It only works on one device" },
                "B", "Password managers eliminate password reuse and make it easy to have strong, random passwords everywhere."));
            bank.Add(new QuizQuestion("Password Managers", QuestionType.TrueFalse,
                "Your master password for a password manager should be very strong and never shared.",
                null, "True", "The master password unlocks all other passwords; it must be long, unique, and kept secret."));

            return bank;
        }

        public void Reset()
        {
            // Create a shuffled copy of the full question bank
            var shuffled = _fullQuestionBank.OrderBy(x => Guid.NewGuid()).ToList();
            _remainingQuestions = new Queue<QuizQuestion>(shuffled);
            _currentScore = 0;
            _totalAnswered = 0;
            _isActive = false;
            _currentQuestion = null;
        }

        public string HandleInput(string userInput)
        {
            userInput = userInput.Trim().ToLowerInvariant();

            // --- Quiz control commands ---
            if (userInput == "start quiz")
            {
                Reset();
                _isActive = true;
                return StartQuiz();
            }

            if (userInput == "resume quiz" || userInput == "continue quiz")
            {
                if (_remainingQuestions.Count == 0)
                    return "No quiz in progress. Type 'start quiz' to begin a new quiz.";

                if (!_isActive && _remainingQuestions.Count > 0)
                {
                    _isActive = true;
                    return GetNextQuestion();
                }
                return "Quiz is already active. Answer the current question or type 'stop quiz'.";
            }

            if (userInput == "stop quiz" || userInput == "pause quiz")
            {
                if (_isActive)
                {
                    _isActive = false;
                    return $"Quiz paused. You have answered {_totalAnswered} out of {_fullQuestionBank.Count} questions. Type 'resume quiz' to continue.";
                }
                return "No active quiz to pause.";
            }

            if (userInput == "restart quiz")
            {
                Reset();
                _isActive = true;
                return "Quiz restarted! " + StartQuiz();
            }

            if (userInput == "quiz score")
            {
                if (_totalAnswered == 0)
                    return "You haven't answered any quiz questions yet. Start a quiz with 'start quiz'.";
                return $"Current score: {_currentScore} / {_totalAnswered}";
            }

            // --- If quiz is active and we are waiting for an answer ---
            if (_isActive && _currentQuestion != null)
            {
                return ProcessAnswer(userInput);
            }

            return null; // Not a quiz command or answer
        }

        private string StartQuiz()
        {
            if (_remainingQuestions.Count == 0)
                return "No questions available. Please restart the quiz.";

            _currentQuestion = _remainingQuestions.Dequeue();
            return FormatQuestion(_currentQuestion);
        }

        private string GetNextQuestion()
        {
            if (_remainingQuestions.Count == 0)
            {
                // Quiz finished
                _isActive = false;
                return GetFinalFeedback();
            }

            _currentQuestion = _remainingQuestions.Dequeue();
            return FormatQuestion(_currentQuestion);
        }

        private string ProcessAnswer(string userAnswer)
        {
            bool isCorrect = false;
            string normalizedAnswer = userAnswer.Trim();

            // Normalise true/false answers
            if (_currentQuestion.Type == QuestionType.TrueFalse)
            {
                if (normalizedAnswer == "true" || normalizedAnswer == "t" || normalizedAnswer == "a")
                    normalizedAnswer = "True";
                else if (normalizedAnswer == "false" || normalizedAnswer == "f" || normalizedAnswer == "b")
                    normalizedAnswer = "False";
                else
                    normalizedAnswer = userAnswer; // keep as is, will compare later
            }

            // Compare ignoring case
            if (string.Equals(normalizedAnswer, _currentQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                isCorrect = true;

            string result;
            if (isCorrect)
            {
                _currentScore++;
                result = $"✅ Correct!\n{_currentQuestion.Explanation}\n";
            }
            else
            {
                result = $"❌ Incorrect.\nThe correct answer is: {_currentQuestion.CorrectAnswer}\n{_currentQuestion.Explanation}\n";
            }

            _totalAnswered++;

            // Check if quiz finished after this answer
            if (_remainingQuestions.Count == 0 && _totalAnswered == _fullQuestionBank.Count)
            {
                _isActive = false;
                _currentQuestion = null;
                return result + "\n" + GetFinalFeedback();
            }

            // Prepare next question
            string nextQuestion = GetNextQuestion();
            return result + "\n" + nextQuestion;
        }

        private string FormatQuestion(QuizQuestion q)
        {
            string output = $"Question {_totalAnswered + 1} of {_fullQuestionBank.Count} (Topic: {q.Topic})\n{q.QuestionText}\n\n";
            if (q.Type == QuestionType.MultipleChoice && q.Options != null)
            {
                foreach (var opt in q.Options)
                    output += opt + "\n";
            }
            else if (q.Type == QuestionType.TrueFalse)
            {
                output += "A) True\nB) False\n";
            }
            return output.TrimEnd();
        }

        private string GetFinalFeedback()
        {
            int total = _fullQuestionBank.Count;
            string message = $"Quiz completed! Your final score: {_currentScore} / {total}\n\n";

            if (_currentScore == total)
                message += "🏆 Excellent! You're a Cybersecurity Pro!";
            else if (_currentScore >= 16)
                message += "🔐 Great job! You have strong cybersecurity awareness.";
            else if (_currentScore >= 12)
                message += "📚 Good work! You understand many cybersecurity concepts, but there's still room to improve.";
            else if (_currentScore >= 8)
                message += "💡 Not bad, but I recommend reviewing some cybersecurity topics to strengthen your knowledge.";
            else
                message += "🚀 Keep learning! Cybersecurity is an important skill, and practice makes perfect.";

            return message;
        }
    }
}