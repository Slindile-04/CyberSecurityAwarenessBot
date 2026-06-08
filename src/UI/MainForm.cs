using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using CyberSecurityAwarenessBot.Services;
using CyberSecurityAwarenessBot.Core;

namespace CyberSecurityAwarenessBot.UI
{
    /// <summary>
    /// MainForm.cs - Main GUI for the Cybersecurity Awareness Bot
    /// 
    /// Responsibilities:
    /// - Display chat interface
    /// - Handle user input via GUI
    /// - Integrate with Chatbot.cs for responses
    /// - Manage message display and formatting
    /// - Handle onboarding flow
    /// 
    /// Architecture:
    /// - MainForm = GUI ONLY (display and input handling)
    /// - Chatbot.cs = LOGIC ONLY (conversation and responses)
    /// - Services = Business logic (voice, memory, sentiment)
    /// - Layout: Title -> Status -> Chat Display -> Input Box -> Buttons
    /// </summary>
    public partial class MainForm : Form
    {
        // Core services
        private readonly VoiceService _voiceService;
        private Core.CyberSecurityAwarenessBot _chatbot;
        private string _audioPath;
        private string _userName;
        private bool _isInitialized = false;

        // GUI Controls
        private TableLayoutPanel mainPanel;
        private Label titleLabel;
        private Label statusLabel;
        private RichTextBox chatDisplayRichTextBox;
        private TextBox userInputTextBox;
        private Button sendButton;
        private Button clearButton;

        // Colors for theme
        private readonly Color DARK_BG = Color.FromArgb(20, 20, 30);
        private readonly Color DARK_PANEL = Color.FromArgb(30, 30, 40);
        private readonly Color INPUT_BG = Color.FromArgb(40, 40, 50);
        private readonly Color CYAN_ACCENT = Color.FromArgb(0, 255, 150);
        private readonly Color BLUE_ACCENT = Color.FromArgb(0, 200, 255);
        private readonly Color TEXT_COLOR = Color.FromArgb(200, 200, 200);

        // ========== TYPING ANIMATION SYSTEM - CURRENTLY DISABLED ==========
        // 
        // DEVELOPER NOTES:
        // ================
        // The typing animation system (character-by-character reveal, typing indicator)
        // was temporarily disabled (May 25, 2026) to provide instant, smooth responses.
        // 
        // WHY DISABLED:
        // - Provides cleaner, faster user experience
        // - Eliminates any potential async overlap or UI jitter
        // - Responses display immediately and completely
        // 
        // HOW TO RE-ENABLE (3 SIMPLE STEPS):
        // ===================================
        // 1. In SendMessage() method (~line 305):
        //    CHANGE:  DisplayBotMessage(botResponse);  // Current instant display
        //    TO:      await DisplayBotMessageAnimatedAsync(botResponse);  // Re-enable animation
        //
        // 2. Change the method signature in SendMessage() to async:
        //    CHANGE:  private async void SendMessage()
        //    (It already is async)
        //
        // 3. Verify the typing configuration constants work as expected:
        //    - _typingIndicatorDelayMs (350ms for dot animation)
        //    - _typingResponseDelayMs (1500ms thinking time)
        //    - _characterRevealDelayMs (35ms base word delay)
        //
        // ANIMATION SYSTEM COMPONENTS:
        // ============================
        // - DisplayBotMessageAnimatedAsync() [Line ~440] - Main animation orchestrator
        // - ShowTypingIndicatorAsync() [Line ~475] - Shows "typing..." indicator with dots
        // - RemoveTypingIndicator() [Line ~510] - Cleanly removes typing indicator
        // - AppendMessageAnimatedAsync() [Line ~530] - Word-by-word reveal with smart timing
        // - TokenizeMessage() [Line ~620] - Splits message into word tokens
        //
        // FUTURE IMPROVEMENTS FOR SMOOTHER ANIMATION:
        // ===========================================
        // - Add configurable animation mode (instant, fast, normal, slow, cinematic)
        // - Implement variable pacing based on message type (tips vs education)
        // - Add audio cues (subtle beeps) with typing
        // - Consider particle effects or gradient reveal for premium feel
        // - Add user preference setting to toggle animation on/off
        //
        // The code remains fully intact and ready to use whenever desired.
        // All async/await patterns, timing logic, and rendering optimizations
        // are preserved for safe re-enablement without refactoring.
        // ========================================================================

        // ========== TYPING ANIMATION CONFIGURATION ==========
        // Controls the smooth typing effect for bot responses
        private bool _isAnimating = false;  // Prevents concurrent animations
        private int _typingIndicatorDelayMs = 350;  // Delay between typing indicator dots (ms) - slower for immersion
        private int _typingResponseDelayMs = 1500;  // How long to show "typing..." before revealing response (ms) - longer thinking time
        private int _characterRevealDelayMs = 35;  // Delay between character reveals for typewriter effect (ms) - slower typing for readability
        private const string TYPING_INDICATOR_BASE = "typing";
        private const string BOT_PREFIX = "Bot: ";  // Prefix for bot messages

        public MainForm()
        {
            _audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
            _voiceService = new VoiceService();
            _userName = string.Empty;

            InitializeComponent();
            ConfigureForm();
            ApplyCyberTheme();
        }

        private void ConfigureForm()
        {
            this.Text = "Cybersecurity Awareness Bot";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 700);
            this.MinimumSize = new Size(700, 500);
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = DARK_BG;
        }

        private void ApplyCyberTheme()
        {
            this.BackColor = DARK_BG;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!_isInitialized)
            {
                InitializeChatbot();
                _isInitialized = true;
            }
        }

        private void InitializeChatbot()
        {
            // Get user's name
            _userName = GetUserName();

            if (string.IsNullOrWhiteSpace(_userName))
                _userName = "User";

            // Initialize chatbot with user's name
            _chatbot = new Core.CyberSecurityAwarenessBot(_audioPath, _userName);

            // Update title and status
            titleLabel.Text = $"Welcome to the Cybersecurity Awareness Bot, {_userName}!";
            statusLabel.Text = "Ready to learn about cybersecurity?";

            // =========================
            // ASCII ART INTRO
            // =========================

            string[] asciiArtLines = new string[]
            {
                "⢀⣴⣶⠛⠋⠉⠉⠉⠉⠉⠙⠛⠒⠒⠒⠒⠒⠒⠒⠀⠠⠤⠤⠤⠤⠤⠤⠤⠤⠤⢤⣤⣄⣀⣀⣀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⣼⢻⡇⠀⠀⢀⣠⡤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⠤⣄⣀⣀⣀⣀⣀⣀⣀⣀⣈⠉⠉⠳⣆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⣇⢸⡇⠀⠀⣾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠻⣆⠀⢸⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⣿⠸⣧⠀⠀⢹⠀⠀          ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ ⠀⠀⠀⠀⠀⢸⡇⠐⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⢸⠀⣿⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⡇⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠸⡄⢸⡄⠀⠀⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣷⠀⢻⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⡇⠀⡇⠀⠀⢿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢹⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⣷⠀⢷⠀⠀⠸⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀                        ⢸⡄⠘⡇          Stay Alert.",
                "⠀⢻⠀⢸⠀⠀⠀⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀              ⠀⠀⠀⠀⠀⠀  ⠘⡇⠀⡇          Stay Secure.",
                "⠀⢸⡇⠸⡄⠀⠀⣿⡀⠀⠀⠀⠀⠀⠀⠀⠀                        ⠰⡇ ⡇          Stay Informed.",
                "⠀⠈⣿⠀⣇⠀⠀⢹⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇⠀⢧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⣿⡀⢹⠀⠀⠘⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢷⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠉⡇⠸⡄⠀⠀⢹⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠆⢸⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⢧⠀⣷⠀⠀⢸⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⡤⠴⠞⠋⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⢸⠀⢹⡀⠀⠈⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣠⠤⠴⠒⠛⠉⠁⢀⣀⣠⣤⣶⣾⣿⣶⠶⠤⣤⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠘⡆⠈⣇⠀⠀⢧⡀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣤⠤⠴⠒⠚⠋⠉⠉⢀⣀⣠⣤⣴⣺⣿⣿⣿⣿⣿⣿⣟⣿⣽⢷⡲⢦⣬⣉⡙⠒⠶⠦⣤⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀",
                "⠀⠀⠀⠀⢧⠀⢹⡀⠀⠘⢷⣤⠤⠤⠤⠖⠒⠚⠋⠉⠁⠀⠀⣀⣀⡤⠴⠖⢚⣯⡫⠼⣿⣿⣽⢿⣿⣿⣭⠼⣿⡛⢛⣦⣬⣟⠛⣿⣭⡽⠓⣶⣤⡀⠈⠉⠓⠲⠤⣤⣀⡀⠀⠀⠀",
                "⠀⠀⠀⠀⢸⡀⠈⣇⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⡤⠤⠖⠻⢋⣁⣈⣤⣶⣾⣯⡿⠟⢿⣉⣠⣿⠛⠋⠹⣧⡠⣾⣿⣻⣷⠴⠟⢿⣅⣠⣿⣾⡿⠟⠁⠀⠀⠀⠀⠀⠀⠈⠉⠓⠲⣤   ",
                "⠀⠀⠀⠀⠈⣧⠀⢹⡆⣀⣀⡤⠤⠖⠛⠋⠁⠁⣀⣠⣤⢖⢻⣿⣥⣴⡛⢉⣹⡷⠶⠿⣍⣀⣨⠷⠞⢻⣅⡠⠼⠟⠉⣹⣦⣴⣿⡿⠟⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡴⠚   ",
                "⠀⠀⠀⠀⠀⠸⠷⢾⢿⡍⠀⠀⠀⣀⡤⠴⢾⣏⣁⣤⢿⠛⣿⣥⠤⢾⡛⠉⣙⣦⠤⢶⣏⢉⣨⡷⠖⠛⢁⣠⡤⢾⣿⣿⣿⣿⡿⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡴⠟⠁⠀⣠     ",
                "⠀⠀⠀⠀⠀⠀⢠⣟⡀⠙⢦⡀⠀⠙⢶⡞⢻⣍⣠⡽⠟⠛⠛⢦⣄⣤⠟⠛⠻⣄⣠⠴⠛⠉⢀⣀⣤⣞⣏⡥⣞⣻⡿⠛⠋⠀⠀⠀⠙⠦⡄⠀⠀⠀⠀⢀⣠⠴⠛⠁⠀⣀⡴⠚⠁    ",
                "⠀⠀⠀⠀⠀⠀⠀⠻⣧⡀⠀⠙⢦⡀⠈⠙⢿⡇⠈⣳⣦⠴⠾⣝⢁⣨⠷⠖⠋⢧⣀⣠⠴⢺⣭⡼⠟⣫⡥⠞⠋⠁⠀⠀⠀⠀⠀⠀⢀⡴⠇⠀⢀⡠⠖⠉⠀⠀⣠⠴⠚⠁⠀⠀⠀     ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⣄⠀⠀⠙⢦⡀⠀⠙⢯⡁⣀⡠⣶⠋⠉⠈⣳⡤⣶⣫⣭⡴⠖⠉⢱⡶⠊⠉⠀⠀⠀⠀⠀⠀⠀⢀⡤⠞⠋⣀⡤⠞⠉⠀⢀⣠⠶⠋⠁⠀⠀⠀⠀⠀⠀       ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⣄⠀⠀⠙⢦⡀⠀⠙⢧⣀⣹⣷⣶⣿⡿⠿⠟⠉⠁⠀⠀⠀⠈⠻⣄⠀⠀⠀⠀⠀⣀⡴⠚⠁⣠⠴⠛⠁⠀⣀⡴⠞⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀        ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⣄⠀⠀⠙⠦⣀⠀⠉⠙⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⢤⣤⠖⠋⢁⡤⠞⠋⠁⢀⣠⠴⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀          ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⢦⡀⠀⠈⠲⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⠴⠚⠉⠀⢀⣤⠖⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀            ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⢦⣀⠀⠈⠳⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡴⠚⠋⠀⢀⣠⡴⠛⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀            ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⣄⡀⠈⠛⢦⡀⠀⠀⠀⢀⣠⠴⠚⠉⠁⠀⣠⡤⠖⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀           ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠦⣀⠀⠙⠓⠒⠛⠉⠀⠀⣀⡤⠖⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀            ",
                "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠳⢤⣄⣀⣤⠤⠖⠚⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀                                     ",

            };

            string asciiArt = string.Join(Environment.NewLine, asciiArtLines);

            // Display ASCII art first with smaller monospace font for better fit
            DisplayBotMessage(asciiArt, isAsciiArt: true);

            // =========================
            // WELCOME MESSAGE
            // =========================

            string welcomeMessage =
                $"\n👋 Hello {_userName}! Welcome to the Cybersecurity Awareness Bot.\n\n" +
                "Type 'help' to see available topics, or just ask me questions naturally. (◠‿◠) ✧˖°.";

            DisplayBotMessage(welcomeMessage);

            // =========================
            // PLAY VOICE GREETING
            // =========================

            try
            {
                _voiceService.PlayGreeting(_audioPath, _userName);
            }
            catch (Exception ex)
            {
                // Silently continue if voice fails
                System.Diagnostics.Debug.WriteLine($"Voice greeting failed: {ex.Message}");
            }

            // Focus input box
            userInputTextBox.Focus();
        }

        /// <summary>
        /// GetUserName - Shows a dialog to prompt for user's name.
        /// Uses cyber-themed styling to match the application aesthetic.
        /// </summary>
        private string GetUserName()
        {
            Form prompt = new Form()
            {
                Text = "Welcome! (◠‿◠) ✧˖°.",
                Width = 400,
                Height = 210,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = DARK_BG,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label welcomeLabel = new Label()
            {
                Left = 20,
                Top = 20,
                Text = "Welcome! What's your name?",
                Width = 350,
                Height = 30,
                ForeColor = CYAN_ACCENT,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            TextBox textBox = new TextBox()
            {
                Left = 20,
                Top = 60,
                Width = 350,
                Height = 30,
                BackColor = INPUT_BG,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            Button okButton = new Button()
            {
                Text = "Start",
                Left = 170,
                Width = 100,
                Top = 110,
                Height = 30,
                DialogResult = DialogResult.OK,
                BackColor = CYAN_ACCENT,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            Button cancelButton = new Button()
            {
                Text = "Exit",
                Left = 280,
                Width = 90,
                Top = 110,
                Height = 30,
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            prompt.Controls.Add(welcomeLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(okButton);
            prompt.Controls.Add(cancelButton);
            prompt.AcceptButton = okButton;
            prompt.CancelButton = cancelButton;
            textBox.Focus();

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }

        /// <summary>
        /// SendButton_Click - Handles Send button click and processes user message.
        /// Captures input, sends to chatbot, displays response, and clears input.
        /// </summary>
        private void SendButton_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// SendMessage - Core method to process and send user message.
        /// Called by Send button or Enter key in input box.
        /// Asynchronously handles chatbot response with animated typing effect.
        /// </summary>
        private async void SendMessage()
        {
            if (userInputTextBox == null || _chatbot == null) return;

            string userInput = userInputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            // Prevent concurrent animations
            if (_isAnimating) return;

            // Display user message instantly
            DisplayUserMessage(userInput);

            // Clear input box and disable to prevent multiple sends during animation
            userInputTextBox.Clear();
            userInputTextBox.Enabled = false;
            sendButton.Enabled = false;

            // Process message through chatbot and get response
            try
            {
                string botResponse = _chatbot.ProcessMessage(userInput);

                // TYPING ANIMATION DISABLED (May 25, 2026) - TO RE-ENABLE:
                // Replace line below with: await DisplayBotMessageAnimatedAsync(botResponse);
                // The animation system is fully preserved in DisplayBotMessageAnimatedAsync() method
                DisplayBotMessage(botResponse);  // Displays response INSTANTLY without animation

                // Update status
                statusLabel.Text = $"Last message: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                DisplayBotMessage($"⚠️ An error occurred: {ex.Message}");
                statusLabel.Text = "Error processing message";
            }
            finally
            {
                // Re-enable input controls
                userInputTextBox.Enabled = true;
                sendButton.Enabled = true;
                _isAnimating = false;
            }

            // Keep focus on input box
            userInputTextBox.Focus();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (chatDisplayRichTextBox != null)
            {
                chatDisplayRichTextBox.Clear();
                statusLabel.Text = "Chat cleared";
            }
        }

        /// <summary>
        /// DisplayUserMessage - Formats and displays user message in the chat area.
        /// User messages appear in white for crisp readability and strong contrast.
        /// </summary>
        private void DisplayUserMessage(string message)
        {
            AppendMessage($"You: {message}", Color.White);
        }

        /// <summary>
        /// DisplayBotMessage - Formats and displays bot response in the chat area.
        /// Bot messages appear in cyan (green-blue) to match theme.
        /// 
        /// Parameters:
        /// - message: The message text to display
        /// - color: Text color (defaults to CYAN_ACCENT)
        /// - isAsciiArt: If true, applies smaller monospace font for ASCII art (optional)
        /// </summary>
        private void DisplayBotMessage(string message, bool isAsciiArt = false)
        {
            if (isAsciiArt)
            {
                // For ASCII art, use a smaller monospace font for better fit
                Font asciiFont = new Font("Consolas", 6, FontStyle.Regular);
                AppendMessage(message, CYAN_ACCENT, asciiFont, isAsciiArt: true);
            }
            else
            {
                // Normal bot messages
                AppendMessage($"Bot: {message}", CYAN_ACCENT);
            }
        }

        /// <summary>
        /// AppendMessage - Core method to append colored text to chat display.
        /// Handles text formatting, coloring, and automatic scrolling to newest message.
        /// This is the base overload for normal messages.
        /// </summary>
        private void AppendMessage(string message, Color color)
        {
            if (chatDisplayRichTextBox == null) return;

            int startIndex = chatDisplayRichTextBox.TextLength;
            chatDisplayRichTextBox.AppendText(message + Environment.NewLine + Environment.NewLine);
            int endIndex = chatDisplayRichTextBox.TextLength;

            // Apply color to the new message
            chatDisplayRichTextBox.Select(startIndex, endIndex - startIndex);
            chatDisplayRichTextBox.SelectionColor = color;

            // Scroll to the end
            chatDisplayRichTextBox.SelectionStart = endIndex;
            chatDisplayRichTextBox.ScrollToCaret();
        }

        /// <summary>
        /// AppendMessage - Overloaded method to append colored text with custom font.
        /// Used specifically for ASCII art that needs a different font size/family.
        /// 
        /// Parameters:
        /// - message: The text to display
        /// - color: Text color
        /// - customFont: Custom font for this message
        /// - isAsciiArt: If true, adds extra spacing/padding after the message
        /// </summary>
        private void AppendMessage(string message, Color color, Font customFont, bool isAsciiArt = false)
        {
            if (chatDisplayRichTextBox == null) return;

            int startIndex = chatDisplayRichTextBox.TextLength;

            // For ASCII art, center it slightly and add padding
            if (isAsciiArt)
            {
                // Add top padding
                chatDisplayRichTextBox.AppendText("\n");

                // Add the ASCII art message
                chatDisplayRichTextBox.AppendText(message);

                // Add bottom padding before next content
                chatDisplayRichTextBox.AppendText("\n\n");
            }
            else
            {
                chatDisplayRichTextBox.AppendText(message + Environment.NewLine + Environment.NewLine);
            }

            int endIndex = chatDisplayRichTextBox.TextLength;

            // Apply custom font to the message
            chatDisplayRichTextBox.Select(startIndex, endIndex - startIndex);
            chatDisplayRichTextBox.SelectionColor = color;
            chatDisplayRichTextBox.SelectionFont = customFont;

            // Scroll to the end
            chatDisplayRichTextBox.SelectionStart = endIndex;
            chatDisplayRichTextBox.ScrollToCaret();
        }

        /// <summary>
        /// DisplayBotMessageAnimatedAsync - Displays bot message with smooth typing animation.
        /// 
        /// ⚠️ CURRENTLY DISABLED - NOT BEING CALLED (May 25, 2026)
        /// This method is fully preserved but not in the active code path.
        /// 
        /// TO RE-ENABLE:
        /// In SendMessage() method, replace:
        ///   DisplayBotMessage(botResponse);
        /// With:
        ///   await DisplayBotMessageAnimatedAsync(botResponse);
        /// 
        /// Animation Flow:
        /// 1. Shows typing indicator ("Bot is typing.", "Bot is typing..", "Bot is typing...")
        /// 2. Waits for configurable duration
        /// 3. Cleanly removes typing indicator (critical fix!)
        /// 4. Reveals message word-by-word with smooth timing
        /// 5. Adds proper spacing
        /// 
        /// This creates a natural, conversational feel similar to modern AI chat apps.
        /// The UI remains fully responsive throughout the animation.
        /// </summary>
        private async Task DisplayBotMessageAnimatedAsync(string message)
        {
            if (chatDisplayRichTextBox == null || string.IsNullOrEmpty(message))
                return;

            _isAnimating = true;

            try
            {
                // Step 1: Show typing indicator animation (returns start/end positions)
                (int startIndex, int endIndex) = await ShowTypingIndicatorAsync(_typingResponseDelayMs);

                // Step 2: Cleanly remove the typing indicator before displaying the actual message
                RemoveTypingIndicator(startIndex, endIndex);

                // Step 3: Reveal the actual message character by character with proper prefix
                await AppendMessageAnimatedAsync($"{BOT_PREFIX}{message}\n\n", CYAN_ACCENT);
            }
            finally
            {
                _isAnimating = false;
            }
        }

        /// <summary>
        /// ShowTypingIndicatorAsync - Animates a typing indicator (animated dots) for a specified duration.
        /// 
        /// ⚠️ CURRENTLY DISABLED - Part of disabled animation system
        /// Preserved for re-enablement. See DisplayBotMessageAnimatedAsync for usage.
        /// 
        /// Shows: "Bot: typing." -> "Bot: typing.." -> "Bot: typing..." (loops)
        /// This creates a natural "I'm thinking" effect before revealing the response.
        /// 
        /// Returns: Tuple of (startIndex, endIndex) so caller can cleanly remove the indicator.
        /// </summary>
        private async Task<(int startIndex, int endIndex)> ShowTypingIndicatorAsync(int durationMs)
        {
            if (chatDisplayRichTextBox == null)
                return (0, 0);

            // Track the position where typing indicator starts
            int typingStartIndex = chatDisplayRichTextBox.TextLength;

            // Append the initial typing indicator with prefix and spacing
            string initialMessage = $"{BOT_PREFIX}{TYPING_INDICATOR_BASE}.\n\n";
            chatDisplayRichTextBox.AppendText(initialMessage);
            int typingEndIndex = chatDisplayRichTextBox.TextLength;

            // Color the typing indicator
            chatDisplayRichTextBox.Select(typingStartIndex, typingEndIndex - typingStartIndex);
            chatDisplayRichTextBox.SelectionColor = CYAN_ACCENT;

            // Scroll to the typing indicator
            chatDisplayRichTextBox.SelectionStart = typingEndIndex;
            chatDisplayRichTextBox.ScrollToCaret();

            // Animate the dots
            int elapsedMs = 0;
            int dotPhase = 0;  // 0 = one dot, 1 = two dots, 2 = three dots

            while (elapsedMs < durationMs)
            {
                await Task.Delay(_typingIndicatorDelayMs);
                elapsedMs += _typingIndicatorDelayMs;

                // Update dots: cycle through 1, 2, 3 dots
                dotPhase = (dotPhase + 1) % 3;
                string dots = new string('.', dotPhase + 1);
                string updatedMessage = $"{BOT_PREFIX}{TYPING_INDICATOR_BASE}{dots}";

                // Update the typing indicator text in-place
                chatDisplayRichTextBox.Select(typingStartIndex, typingEndIndex - typingStartIndex);
                chatDisplayRichTextBox.SelectedText = updatedMessage;
                typingEndIndex = typingStartIndex + updatedMessage.Length;

                // Recolor the updated text
                chatDisplayRichTextBox.Select(typingStartIndex, updatedMessage.Length);
                chatDisplayRichTextBox.SelectionColor = CYAN_ACCENT;

                // Scroll to keep indicator visible
                chatDisplayRichTextBox.SelectionStart = typingEndIndex;
                chatDisplayRichTextBox.ScrollToCaret();
            }

            // Return the positions so caller can remove the indicator cleanly
            return (typingStartIndex, typingEndIndex);
        }

        /// <summary>
        /// RemoveTypingIndicator - Cleanly removes the typing indicator from the chat display.
        ///
        /// ⚠️ CURRENTLY DISABLED - Part of disabled animation system (May 25, 2026)
        /// Preserved for re-enablement. Called by DisplayBotMessageAnimatedAsync.
        ///
        /// This is critical to prevent the "Bot: typing..." message from staying permanently.
        ///
        /// TO RE-ENABLE: See DisplayBotMessageAnimatedAsync and SendMessage() for instructions.
        /// </summary>
        private void RemoveTypingIndicator(int startIndex, int endIndex)
        {
            if (chatDisplayRichTextBox == null || startIndex < 0 || endIndex <= startIndex)
                return;

            try
            {
                // Select the entire typing indicator including trailing whitespace
                chatDisplayRichTextBox.Select(startIndex, endIndex - startIndex);

                // Delete it by setting selected text to empty
                chatDisplayRichTextBox.SelectedText = string.Empty;

                // Ensure cursor is at the end of remaining text
                chatDisplayRichTextBox.SelectionStart = chatDisplayRichTextBox.TextLength;
            }
            catch (Exception ex)
            {
                // Silently handle any edge cases (shouldn't happen, but safety first)
                System.Diagnostics.Debug.WriteLine($"Error removing typing indicator: {ex.Message}");
            }
        }

        /// <summary>
        /// AppendMessageAnimatedAsync - Reveals a message word-by-word with smooth animation.
        /// 
        /// CRITICAL OPTIMIZATION: Word-by-word reveals instead of character-by-character.
        /// This drastically reduces RichTextBox redraws and eliminates jittering.
        /// 
        /// Features:
        /// - Word-by-word reveal (smooth, no character-level flicker)
        /// - Variable timing based on punctuation context
        /// - Batch color application (not per-character)
        /// - Throttled scrolling (not after every word)
        /// - Proper handling of newlines and spacing
        /// - Professional typing animation feel
        /// </summary>
        private async Task AppendMessageAnimatedAsync(string message, Color color)
        {
            if (chatDisplayRichTextBox == null || string.IsNullOrEmpty(message))
                return;

            // Split message into tokens (words + punctuation + newlines)
            // This allows us to process meaningful chunks instead of individual characters
            var tokens = TokenizeMessage(message);

            int startIndex = chatDisplayRichTextBox.TextLength;
            int wordCount = 0;
            int lastScrollIndex = startIndex;

            // Append tokens with word-level animation
            for (int i = 0; i < tokens.Count; i++)
            {
                string token = tokens[i];

                // Append the token
                chatDisplayRichTextBox.AppendText(token);
                int currentEndIndex = chatDisplayRichTextBox.TextLength;

                // Apply color to the newly added text (batch operation)
                chatDisplayRichTextBox.Select(currentEndIndex - token.Length, token.Length);
                chatDisplayRichTextBox.SelectionColor = color;

                // CRITICAL OPTIMIZATION: Only scroll every 3-5 words or at line breaks
                // This prevents jittering from constant ScrollToCaret() calls
                wordCount++;
                if (wordCount % 4 == 0 || token.Contains("\n"))
                {
                    chatDisplayRichTextBox.SelectionStart = currentEndIndex;
                    chatDisplayRichTextBox.ScrollToCaret();
                    lastScrollIndex = currentEndIndex;
                }

                // Determine timing based on token content
                // Word-level delays are proportional to character base delay for consistency
                int delayMs = _characterRevealDelayMs * 2;  // Base delay for words (slightly longer than 1:1 char timing)

                // Newlines appear immediately
                if (token.Contains("\n"))
                {
                    delayMs = 60;  // Slightly longer pause for line break (more intentional)
                }
                // Longer pause after sentence-ending punctuation (very noticeable)
                else if (token.EndsWith(".") || token.EndsWith("!") || token.EndsWith("?"))
                {
                    delayMs = _characterRevealDelayMs * 5;  // Extended pause at sentence end for natural reading rhythm
                }
                // Moderate pause after commas (reader breath)
                else if (token.EndsWith(","))
                {
                    delayMs = _characterRevealDelayMs * 3;  // Comma pause for pacing
                }
                // Slight pause after colons and semicolons
                else if (token.EndsWith(":") || token.EndsWith(";"))
                {
                    delayMs = _characterRevealDelayMs * 2 + 10;  // Brief pause for flow
                }

                // Non-blocking delay (ensures UI remains responsive)
                if (delayMs > 0)
                {
                    await Task.Delay(Math.Max(1, delayMs));
                }
            }

            // Final scroll to bottom (if not already scrolled recently)
            int finalIndex = chatDisplayRichTextBox.TextLength;
            if (finalIndex > lastScrollIndex + 50)
            {
                chatDisplayRichTextBox.SelectionStart = finalIndex;
                chatDisplayRichTextBox.ScrollToCaret();
            }
        }

        /// <summary>
        /// TokenizeMessage - Splits message into words and meaningful punctuation tokens.
        /// 
        /// Instead of processing character-by-character (creates jitter),
        /// this breaks the message into words which animate smoothly.
        /// 
        /// Example: "Hello, world!\nHow are you?"
        /// Tokens: ["Hello", ", ", "world", "! ", "\n", "How", " ", "are", " ", "you", "?"]
        /// 
        /// This reduces animation operations by ~5x and eliminates flickering.
        /// </summary>
        private List<string> TokenizeMessage(string message)
        {
            var tokens = new List<string>();
            var currentToken = new StringBuilder();

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];

                // Newlines are separate tokens
                if (c == '\n')
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                    tokens.Add("\n");
                }
                // Spaces separate words
                else if (c == ' ')
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                    // Combine space with next punctuation if present
                    int nextIdx = i + 1;
                    if (nextIdx < message.Length && char.IsPunctuation(message[nextIdx]))
                    {
                        tokens.Add(" " + message[nextIdx]);
                        i++;  // Skip the punctuation we just processed
                    }
                    else
                    {
                        tokens.Add(" ");
                    }
                }
                // Accumulate word characters
                else
                {
                    currentToken.Append(c);
                }
            }

            // Don't forget the last token
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }

            return tokens;
        }

        /// <summary>
        /// InitializeComponent - Creates and configures all GUI controls.
        /// 
        /// Layout Structure (Top to Bottom):
        /// 1. Title Bar (titleLabel)
        /// 2. Status Bar (statusLabel)  
        /// 3. Chat Display (RichTextBox) - Main area
        /// 4. Input TextBox - User message input
        /// 5. Button Panel - Send and Clear buttons
        /// 
        /// This layout ensures controls display correctly regardless of form size.
        /// </summary>
        private void InitializeComponent()
        {
            // Create main layout panel using TableLayoutPanel for proper control ordering
            this.mainPanel = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = DARK_BG,
                RowCount = 6,
                ColumnCount = 1,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0)
            };

            // Configure row styles (percentages, fixed sizes, or auto)
            mainPanel.RowStyles.Clear();
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));      // Title bar - fixed 35px
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));      // Status bar - fixed 25px
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));      // Chat display - fill remaining
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));      // Input box - fixed 50px
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));      // Send button - fixed 35px
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));      // Clear button - fixed 35px

            // ========== TITLE LABEL ==========
            this.titleLabel = new Label()
            {
                Dock = DockStyle.Fill,
                Text = "Cybersecurity Awareness Bot",
                ForeColor = CYAN_ACCENT,
                BackColor = DARK_PANEL,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };
            mainPanel.Controls.Add(titleLabel, 0, 0);

            // ========== STATUS LABEL ==========
            this.statusLabel = new Label()
            {
                Dock = DockStyle.Fill,
                Text = "Ready",
                ForeColor = BLUE_ACCENT,
                BackColor = DARK_BG,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9),
                Padding = new Padding(10, 0, 0, 0)
            };
            mainPanel.Controls.Add(statusLabel, 0, 1);

            // ========== CHAT DISPLAY AREA ==========
            this.chatDisplayRichTextBox = new RichTextBox()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = DARK_PANEL,
                ForeColor = TEXT_COLOR,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0),
                WordWrap = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                TabStop = false
            };
            mainPanel.Controls.Add(chatDisplayRichTextBox, 0, 2);

            // ========== USER INPUT TEXTBOX ==========
            this.userInputTextBox = new TextBox()
            {
                Dock = DockStyle.Fill,
                BackColor = INPUT_BG,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Multiline = true,
                AcceptsReturn = false,
                ScrollBars = ScrollBars.Vertical,
                Margin = new Padding(0)
            };
            // Handle Enter key to send message
            userInputTextBox.KeyDown += UserInputTextBox_KeyDown;
            mainPanel.Controls.Add(userInputTextBox, 0, 3);

            // ========== SEND BUTTON ==========
            this.sendButton = new Button()
            {
                Dock = DockStyle.Fill,
                Text = "Send Message",
                BackColor = Color.FromArgb(0, 180, 100),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0)
            };
            sendButton.Click += new EventHandler(SendButton_Click);
            mainPanel.Controls.Add(sendButton, 0, 4);

            // ========== CLEAR BUTTON ==========
            this.clearButton = new Button()
            {
                Dock = DockStyle.Fill,
                Text = "Clear Chat",
                BackColor = Color.FromArgb(180, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0)
            };
            clearButton.Click += new EventHandler(ClearButton_Click);
            mainPanel.Controls.Add(clearButton, 0, 5);

            // Add main panel to form
            this.Controls.Add(mainPanel);
        }

        /// <summary>
        /// UserInputTextBox_KeyDown - Handles keyboard input in the text box.
        /// Pressing Enter sends the message without creating a new line.
        /// Shift+Enter creates a new line.
        /// </summary>
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                SendMessage();
                e.Handled = true;  // Prevent ding sound
                e.SuppressKeyPress = true;  // Prevent default Enter behavior
            }
        }
    }
}