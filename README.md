# CyberSecurityAwarenessBot

A **Cybersecurity Awareness Chatbot** designed to educate South African citizens about common online threats and safe online practices.

*Module: PROG6221 – Programming 2A*  
*Language: C#*  
*Application Type: .NET Console Application*

---

## Project Overview

The **CyberSecurityAwarenessBot** is an interactive console application that serves as an educational tool for teaching users about cybersecurity best practices and online safety. The chatbot provides personalized, conversational guidance on critical security topics that affect South African citizens.

**Purpose:**
- Educate users about common cybersecurity threats
- Promote safe online practices
- Provide accessible, engaging security awareness training
- Create a foundation for extended cybersecurity education platforms

**Target Topics:**
- Phishing attacks and social engineering
- Strong password creation and management
- Two-factor authentication (2FA)
- Data privacy and protection
- Secure browsing practices

---

## Features

### Part 1 Implementation ✅

The following features have been successfully implemented in Part 1:

#### 🎨 **Visual Design**
- **ASCII Title Screen** – Cybersecurity-themed ASCII art banner displayed at startup with line-by-line animation
- **Console Color Styling** – Color-coded messages for different response types (green for success, cyan for menus, yellow for bot responses)
- **Emoji Support** – Unicode emoji integration (🤖, 🔐, 📚, etc.) for enhanced visual communication
- **Professional Layout** – Formatted boxes and borders for structured information display

#### 🎵 **Audio Integration**
- **Audio Greeting** – Welcome message played as a WAV file at program startup
- **Error Gracefully Handling** – Application continues normally if audio file is unavailable

#### 💬 **Interactive Conversation**
- **Dual Input Modes** – Supports both numeric menu selection (1-5) and natural language keywords
- **Conversational Responses** – Understands natural language queries like "How are you?" and "What can you help with?"
- **Topic Selection Menu** – Display of available cybersecurity topics in a user-friendly format
- **Personalized Responses** – All bot responses include the user's name for engaging, personalized interaction

#### ⌨️ **User Experience**
- **Typing Effect** – Character-by-character animation for bot responses, creating a natural conversational feel
- **Typing Indicator** – Shows "🤖 Bot is typing..." feedback while processing user input
- **Input Validation and Handling** – Graceful handling of unexpected input with helpful suggestions
- **Auto-Scroll Functionality** – Console automatically scrolls to show latest messages
- **Loading Sequence** – Animated startup sequence that simulates system initialization

#### 🔒 **Educational Content**
- **Structured Educational Modules** – Five comprehensive cybersecurity topics with formatted content boxes
- **Red Flags and Prevention Tips** – Each topic includes warning signs and mitigation strategies
- **Professional Formatting** – Content presented in easy-to-read boxes with user context

---

## Technologies Used

- **Language:** C# 12
- **.NET Framework:** .NET 8.0
- **Console Application:** System.Console
- **Audio Playback:** System.Media.SoundPlayer
- **File I/O:** System.IO for path and file management
- **Text Encoding:** UTF-8 for emoji and Unicode support
- **Version Control:** Git / GitHub
- **IDE:** Visual Studio Code

---

## Project Structure

```
CyberSecurityAwarenessBot/
├── src/
│   ├── Program.cs                 # Entry point and startup orchestration
│   ├── Chatbot.cs                 # Core chatbot logic and conversation engine
│   └── Helpers/
│       ├── UIHelper.cs            # UI utilities (colors, animations, typing effects)
│       └── InputHelper.cs         # Input validation and processing
├── Audio/
│   └── [Audio greeting files]     # WAV files for audio greetings
├── bin/                           # Compiled binaries (Debug/Release)
├── obj/                           # Build intermediate files
├── CyberSecurityAwarenessBot.csproj
├── CyberSecurityAwarenessBot.sln
└── README.md                      # This file
```

### Key Files Explained

| File | Purpose |
|------|---------|
| **Program.cs** | Entry point that orchestrates startup sequence: loading animation → ASCII title → audio greeting → user name collection → chatbot initialization |
| **Chatbot.cs** | Core conversation engine with intelligent route matching for both numeric menu and natural language inputs |
| **UIHelper.cs** | Centralized UI library providing colored output, typing animations, ASCII art display, and loading sequences |
| **InputHelper.cs** | Input validation and processing (foundation for future enhancement) |
| **Audio/** | Directory containing WAV files for audio greetings |

---

## How to Run the Project

### Prerequisites
- .NET SDK 8.0 or later installed
- Git (for cloning the repository)

### Installation & Execution

#### Option 1: Using the Terminal
```bash
# Clone the repository
git clone https://github.com/Slindile-04/CyberSecurityAwarenessBot.git
cd CyberSecurityAwarenessBot

# Run the application
dotnet run --project CyberSecurityAwarenessBot.csproj
```

#### Option 2: Using Visual Studio Code
1. Clone the repository: `git clone https://github.com/Slindile-04/CyberSecurityAwarenessBot.git`
2. Open the folder in VS Code: `code CyberSecurityAwarenessBot`
3. Open the terminal in VS Code (Ctrl + `)
4. Run: `dotnet run`

#### Option 3: Build and Run
```bash
# Build the project
dotnet build

# Run the compiled application
dotnet run
```

### First Use
1. The application will display a loading sequence and ASCII art banner
2. A welcome audio greeting will play (if available)
3. Enter your name when prompted
4. Select a topic by number (1-5) or type a keyword (e.g., "phishing", "passwords")
5. Type "help" for a list of all available commands
6. Type "exit" or "quit" to end the session

---

## Usage Examples

### Example Interaction

```
╔════════════════════════════════════════════════════════════╗
║             Cybersecurity Topics (Numeric Menu)            ║
╠════════════════════════════════════════════════════════════╣
║  1. Phishing Attacks   | 2. Strong Passwords               ║
║  3. Two-Factor Auth    | 4. Data Privacy                   ║
║  5. Secure Browsing    | 0. Exit                           ║
╠════════════════════════════════════════════════════════════╣
║ Or just ask naturally:                                     ║
║ • "How are you?"  • "What can you help with?"         ║
║ • "Tell me about phishing"  • "help"                  ║
╚════════════════════════════════════════════════════════════╝

Your Name, what would you like to know? phishing

🤖 Bot is typing...

┌────────────── PHISHING ATTACKS EDUCATION ──────────────┐
│ User: [Your Name]                                      │
├────────────────────────────────────────────────────────┤
│ Phishing is a social engineering attack where attackers│
│ impersonate trusted entities to steal sensitive info.  │
│                                                        │
│ RED FLAGS:                                             │
│ • Urgent requests for passwords or personal info       │
│ • Suspicious sender email addresses                    │
│ • Links that don't match the displayed text            │
│ • Grammar and spelling errors                          │
└────────────────────────────────────────────────────────┘
```

### Natural Language Interactions
- **Greeting:** "How are you?" → Bot responds conversationally
- **Purpose:** "What can you help me with?" → Bot explains its purpose
- **Menu Selection:** "Tell me about passwords" → Shows password education
- **Exit:** "Goodbye" → Safely exits the application

---

## Architecture & Design Patterns

### Key Design Decisions

1. **Separation of Concerns**
   - Core chatbot logic isolated in `Chatbot.cs`
   - UI/styling handled by `UIHelper.cs`
   - Input validation in `InputHelper.cs`
   - This structure allows easy modification and testing

2. **Relative Path Resolution**
   - Audio files located using relative paths instead of hardcoded paths
   - Ensures portability across different machines and environments
   - Navigates from executable location back to project root

3. **Graceful Error Handling**
   - Missing audio files don't crash the application
   - Invalid inputs receive helpful suggestions
   - All exceptions are handled with user-friendly messages

4. **User Engagement**
   - Multi-sensory startup experience (visual, audio, text)
   - Personalization through user name collection
   - Typing animations create natural conversation feel
   - Color coding helps users parse information quickly

5. **Extensibility**
   - Adding new educational topics requires only adding new methods
   - UI styling can be updated in a single location
   - Helper classes provide foundation for future features

---

## Future Improvements (Part 2+)

Planned enhancements for future iterations:

### 🖼️ **GUI Implementation**
- Migrate from console to graphical interface (Windows Forms or WPF)
- Multi-window support for side-by-side chatbot interaction
- Rich text formatting for better content presentation

### 🤖 **Enhanced Chatbot Intelligence**
- Intent-based response system using natural language processing
- Machine learning model for improved keyword matching
- Context-aware responses that remember previous user input

### 📚 **Expanded Cybersecurity Content**
- Additional security topics (social engineering, malware, ransomware, etc.)
- Real-world case studies and examples
- Interactive quizzes to test knowledge
- Progress tracking and achievement badges

### 💾 **Data Persistence**
- User profiles and conversation history
- Learning progress tracking
- Personalized recommendations based on user interactions
- Export/download security tips for offline reference

### 🌐 **Multi-Language Support**
- Localization for South African languages (Zulu, Xhosa, Sotho, etc.)
- Culturally relevant examples and scenarios

### 📊 **Analytics & Reporting**
- Track user engagement and learning outcomes
- Generate reports on most viewed topics
- Usage statistics for program improvement

---

## Contributing

This is an academic project created as part of PROG6221 coursework. Contributions are welcome!

To contribute:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add YourFeature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

---

## License

This project is provided as-is for educational purposes as part of the PROG6221 – Programming 2A module.

---

## Author

**Slindile**  
*Student, PROG6221 – Programming 2A*

This CyberSecurityAwarenessBot was created as part of academic coursework to demonstrate:
- Object-oriented programming principles
- Console application development
- User experience and interface design
- Separation of concerns and code organization

---

## Contact & Support

For questions, issues, or suggestions, please:
- Open an issue on GitHub
- Contact via: [Your Contact Information]

---

## Acknowledgments

- Thanks to the PROG6221 module instructors for the project assignment
- Special thanks to System.Media.SoundPlayer for easy audio integration
- Inspiration for cybersecurity awareness from South African cybersecurity best practices

---

**Last Updated:** March 12, 2026  
**Status:** Part 1 Complete ✅ | Part 2 (In Planning)
