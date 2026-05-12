using System;
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
            titleLabel.Text = $"Welcome to Cybersecurity Bot, {_userName}!";
            statusLabel.Text = "Ready to learn about cybersecurity...";

            // Display welcome message
            DisplayBotMessage($"👋 Hello {_userName}! Welcome to the Cybersecurity Awareness Bot.");
            DisplayBotMessage("I'm here to teach you about cybersecurity best practices, threats, and how to stay safe online.");
            DisplayBotMessage("Type 'help' to see available topics, or just ask me questions naturally!");

            // Play voice greeting
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
                Text = "Welcome to Cybersecurity Bot",
                Width = 400,
                Height = 180,
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
                Text = "Start Learning",
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
        /// </summary>
        private void SendMessage()
        {
            if (userInputTextBox == null || _chatbot == null) return;

            string userInput = userInputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            // Display user message
            DisplayUserMessage(userInput);

            // Clear input box
            userInputTextBox.Clear();

            // Process message through chatbot and get response
            try
            {
                string botResponse = _chatbot.ProcessMessage(userInput);
                DisplayBotMessage(botResponse);

                // Update status
                statusLabel.Text = $"Last message: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                DisplayBotMessage($"⚠️ An error occurred: {ex.Message}");
                statusLabel.Text = "Error processing message";
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
        /// User messages appear in blue to distinguish from bot messages.
        /// </summary>
        private void DisplayUserMessage(string message)
        {
            AppendMessage($"You: {message}", BLUE_ACCENT);
        }

        /// <summary>
        /// DisplayBotMessage - Formats and displays bot response in the chat area.
        /// Bot messages appear in cyan (green-blue) to match theme.
        /// </summary>
        private void DisplayBotMessage(string message)
        {
            AppendMessage($"Bot: {message}", CYAN_ACCENT);
        }

        /// <summary>
        /// AppendMessage - Core method to append colored text to chat display.
        /// Handles text formatting, coloring, and automatic scrolling to newest message.
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
                Text = "Cybersecurity Bot",
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
