# CyberSecurityAwarenessBot

**An intelligent, conversational cybersecurity awareness chatbot designed to educate users about online threats and safe digital practices.**

---

## 📋 Project Overview

The **CyberSecurityAwarenessBot** is an interactive chatbot application that provides personalized cybersecurity education through natural conversation. Currently running as a terminal application with a GUI (Windows Forms) under development.

### Purpose
- Educate users about common cybersecurity threats
- Promote safe online practices  
- Provide accessible, engaging security awareness training
- Create a foundation for extended cybersecurity education platforms

### Target Audience
Users interested in learning about:
- Phishing attacks and social engineering
- Strong password creation and management
- Two-factor authentication (2FA)
- Data privacy and protection
- Secure browsing practices
- Ransomware threats
- Patch management
- Wi-Fi security
- And more...

---

## 🚀 Features

### Core Chatbot Features ✅
- **Keyword Recognition** – Understands natural language queries about cybersecurity topics
- **Intelligent Responses** – Delivers varied, non-repetitive tips and advice
- **Conversation Flow** – Maintains context across multi-turn interactions
- **Memory & Recall System** – Stores and recalls user interests and preferences
- **Sentiment Detection** – Detects emotions (Worried, Curious, Frustrated, Positive) and adapts tone accordingly
- **Tip Progression** – Delivers 7 tips per topic without repetition
- **Error Handling** – Gracefully handles unexpected input

### UI/UX Features ✅
- **Console Application** – Terminal-based interface with color coding and emoji support
- **ASCII Title Screen** – Cybersecurity-themed animated banner at startup
- **Audio Integration** – WAV audio greetings on program startup
- **Typing Effects** – Character-by-character animation for natural conversation feel
- **User Personalization** – All responses include user's name for engagement
- **Input Validation** – Helpful suggestions for unexpected input

### Future Features 🚧
- **Windows Forms GUI** – Full graphical interface (in development)
- **GUI Animations** – Smooth transitions and visual effects
- **Enhanced AI Flow** – Smarter conversation patterns
- **Persistent Memory** – Save/load user preferences across sessions
- **Conversation History** – Display previous discussions
- **Smart Recommendations** – Suggest related topics based on interests

---

## 💻 Technologies Used

| Component | Technology |
|-----------|-----------|
| **Language** | C# 12 |
| **.NET Framework** | .NET 8.0 |
| **Target Framework** | net8.0-windows |
| **UI Framework** | Windows Forms (WinForms) |
| **Audio** | System.Media.SoundPlayer |
| **Console** | System.Console |
| **Version Control** | Git / GitHub |
| **IDE** | Visual Studio Code |

---

## 🏗️ Architecture Overview

### Application Structure

```
src/
├── UI/
│   └── MainForm.cs                   ← Windows Forms GUI (future)
├── Chatbot.cs                        ← Core conversation engine
├── Program.cs                        ← Application entry point
├── Services/
│   ├── ConversationManager.cs        ← Context & state tracking
│   ├── TipTracker.cs                 ← Tip selection & progression
│   ├── MemoryService.cs              ← User memory management
│   ├── SentimentAnalyzer.cs          ← Emotion detection
│   ├── VoiceService.cs               ← Audio playback
│   └── ThemeService.cs               ← Theme definitions
├── Models/
│   ├── ChatMessage.cs                ← Message data structure
│   ├── SecurityTip.cs                ← Tip data structure
│   └── User.cs                       ← User data structure
├── Helpers/
│   ├── InputHelper.cs                ← Input parsing utilities
│   └── UIHelper.cs                   ← Console UI utilities
└── TipRepository.cs                  ← Central tip storage (70 tips)
```

### Layer Separation

| Layer | Purpose | Files |
|-------|---------|-------|
| **UI** | Display & input handling | MainForm.cs |
| **Logic** | Conversation & decision making | Chatbot.cs, Services/ |
| **Data** | Tip storage & user info | TipRepository.cs, Models/ |
| **Utilities** | Helper functions | Helpers/ |

### Key Services

**ConversationManager** – Tracks conversation context:
- Current/previous topics
- Sentiment history  
- User intent (AskingTip, RequestingEducation, etc.)
- Context for follow-up detection

**TipTracker** – Manages tip delivery:
- Prevents duplicate tips
- Tracks 7-tip progression per topic
- Provides progress feedback

**SentimentAnalyzer** – Detects emotions via keyword analysis:
- Worried: Provides reassuring responses
- Frustrated: Simplifies explanations  
- Curious: Engages with detailed content
- Positive: Celebrates enthusiasm

---

## 📊 Current Status

### ✅ Completed
- Terminal chatbot fully functional
- All 4 improvement issues resolved
- Memory recall system working
- Sentiment detection integrated
- Tip progression system implemented
- Flexible conversation patterns supported
- Project compiles successfully (0 errors, 37 non-blocking warnings)

### 🚧 In Development
- Windows Forms GUI integration
- Persistent data storage
- Enhanced animations

---

## 🛠️ Installation & Running

### Prerequisites
- **.NET SDK 8.0** or later ([download](https://dotnet.microsoft.com/download))
- **Git** for version control
- **Visual Studio Code** (optional, or any C# IDE)

### Clone the Repository
```bash
git clone https://github.com/yourusername/CyberSecurityAwarenessBot.git
cd CyberSecurityAwarenessBot
```

### Build the Project
```bash
dotnet build
```

### Run the Application

**Terminal Version (Current):**
```bash
dotnet run --project CyberSecurityAwarenessBot.csproj
```

**Release Build:**
```bash
dotnet build --configuration Release
dotnet run --configuration Release
```

### Run Tests
```bash
dotnet test
```

---

## 📖 Documentation

- **[DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md)** – Architecture guide for developers
  - Folder structure explanation
  - File responsibilities  
  - Development guidelines
  - How to extend the application

---

## 🎯 Usage Examples

### Example 1: Get a Phishing Tip
```
User: Tell me about phishing
Bot: I'd be happy to help! Here's a tip about phishing:
     🎣 Tip: Always hover over links in emails before clicking to verify the actual URL.
     Want another tip about phishing?
```

### Example 2: Sentiment-Aware Response
```
User: I'm worried about password security
Bot: I understand password security might feel concerning. Let me explain clearly 💡
     [Provides reassuring, simple explanation]
```

### Example 3: Follow-up Flexibility
```
User: Give me a privacy tip
Bot: [Provides tip #1]

User: Another one  ← Natural follow-up
Bot: [Provides tip #2 - same topic]

User: Go deeper    ← Deep dive request
Bot: Let me dive deeper into privacy...
     [Provides comprehensive education]
```

---

## 🔄 Versioning

This project uses semantic versioning:
- **v1.0** – Initial terminal chatbot release
- **v1.1** – Memory recall improvements  
- **v2.0** – Windows Forms GUI (planned)

See [GitHub Releases](https://github.com/yourusername/CyberSecurityAwarenessBot/releases) for version history.

---

## 🚀 Future Improvements

### Phase 3: GUI Polish
- [ ] Modern dark/light theme support
- [ ] Smooth animations and transitions
- [ ] Responsive dialog layouts
- [ ] Icon theming

### Phase 4: Intelligence
- [ ] Smarter topic recommendations
- [ ] Multi-turn conversation patterns
- [ ] Conversation history persistence
- [ ] User progress tracking

### Phase 5: Scalability
- [ ] Additional cybersecurity topics
- [ ] Admin panel for topic management
- [ ] User analytics
- [ ] Multi-language support

---

## 📝 License

This project is provided for educational purposes. See LICENSE file for details.

---

## 👥 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a pull request

See DEVELOPER_GUIDE.md for development guidelines.

---

## 📧 Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Check [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) for common questions

---

## 🎓 Academic Context

**Module:** PROG6221 – Programming 2A  
**Institution:** [Your Institution]  
**Purpose:** Educational platform for cybersecurity awareness

---

*Last Updated: May 2026*  
*Built with ❤️ for cybersecurity awareness*

