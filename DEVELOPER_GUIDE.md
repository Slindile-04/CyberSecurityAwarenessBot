# CyberSecurityAwarenessBot - Developer Guide

**A comprehensive guide for developers and contributors to understand, modify, and extend the CyberSecurityAwarenessBot.**

---

## 📚 Table of Contents

1. [Folder Structure](#folder-structure)
2. [File Responsibilities](#file-responsibilities)
3. [Application Flow](#application-flow)
4. [Development Guidelines](#development-guidelines)
5. [Adding New Features](#adding-new-features)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## 📁 Folder Structure

### Root Directory

```
CyberSecurityAwarenessBot/
├── src/                    ← All source code
├── tests/                  ← Unit tests
├── Resources/              ← Images, themes, assets
├── config/                 ← Configuration files
├── Audio/                  ← Voice files (greeting.wav)
├── bin/                    ← Compiled binaries
├── obj/                    ← Build artifacts
└── .vscode/                ← VS Code settings
```

### `src/` Directory Structure

```
src/
├── Program.cs              ← Application entry point
├── Chatbot.cs              ← Core conversation engine
├── TipRepository.cs        ← Central tip storage
├── SentimentAnalyzer.cs    ← Emotion detection
├── UserMemory.cs           ← User preferences storage
│
├── UI/                     ← GUI Layer (Windows Forms)
│   ├── MainForm.cs         ← Main application window
│   ├── Controls/           ← Reusable UI components (future)
│   └── Dialogs/            ← Dialog windows (future)
│
├── Services/               ← Business Logic Services
│   ├── ConversationManager.cs   ← Context & state tracking
│   ├── TipTracker.cs            ← Tip selection & progression
│   ├── MemoryService.cs         ← Memory management
│   ├── VoiceService.cs          ← Audio playback
│   └── ThemeService.cs          ← Theme management
│
├── Models/                 ← Data Structures
│   ├── ChatMessage.cs      ← Message model
│   ├── SecurityTip.cs      ← Tip model
│   └── User.cs             ← User model
│
└── Helpers/                ← Utility Functions
    ├── InputHelper.cs      ← Input parsing
    └── UIHelper.cs         ← Console UI utilities
```

### `Resources/` Directory

```
Resources/
├── Images/                 ← Icons and graphics
│   └── cyber-theme/
└── Themes/                 ← Theme definitions (JSON)
```

### `config/` Directory

```
config/
├── appsettings.json        ← Application configuration
└── tips.json               ← Security tips database (future)
```

---

## 📄 File Responsibilities

### Core Files (Chatbot Logic)

#### `src/Program.cs`
**Purpose:** Application entry point  
**Responsibility:** Initialize the application and start the console chatbot  
**Edit When:** Setting up startup sequence, changing initial flow  
**Key Code:** `Main()` method entry point  
**Dependencies:** None (entry point)

#### `src/Chatbot.cs`
**Purpose:** Core conversation orchestrator  
**Responsibility:** Process user input, generate responses, coordinate services  
**Edit When:** Adding new conversation logic, changing response patterns  
**Key Methods:**
- `HandleUserInput(string input)` – Main message processing
- `HandleTipRequest(string input, string sentiment)` – Delivers tips
- `GenerateEducationContent(string topic)` – Educational content
- `CheckAndStoreInterests(string input)` – Interest tracking

**Dependencies:** All services (ConversationManager, TipTracker, SentimentAnalyzer, UserMemory)  
**Safety:** Highly modular; safe to extend with new methods

#### `src/TipRepository.cs`
**Purpose:** Centralized tip storage  
**Responsibility:** Store and retrieve cybersecurity tips  
**Edit When:** Adding new topics, modifying existing tips  
**Key Methods:**
- `InitializeTips()` – Defines all 70 tips (10 topics × 7 tips)
- `GetAllTips(string topic)` – Returns all tips for a topic
- `GetRandomTip(string topic)` – Returns random tip

**Structure:**
```csharp
Dictionary<string, List<string>> {
    "phishing" → [7 tips],
    "passwords" → [7 tips],
    "2fa" → [7 tips],
    // ... etc
}
```

**Safety:** Only data; safe to modify content

---

### Service Files (Business Logic)

#### `src/Services/ConversationManager.cs`
**Purpose:** Tracks and manages conversation state  
**Responsibility:** Maintain context, track sentiment history, detect intent  
**Edit When:** Changing context detection, adding new intent types  
**Key Fields:**
- `_currentTopic` – What topic is being discussed
- `_previousTopic` – What topic was discussed before
- `_sentimentHistory` – Last 5 message sentiments
- `_userIntent` – Current user intent (AskingTip, RequestingEducation, etc.)

**Enums:**
```csharp
UserIntent {
    Unknown,
    AskingTip,
    RequestingEducation,
    AskingQuestion,
    SettingPreference,
    ContinuingConversation,
    ExploringSameTopic
}
```

**Safety:** Foundation for context awareness; test thoroughly before modifying

#### `src/Services/TipTracker.cs`
**Purpose:** Manages tip selection and prevents duplicates  
**Responsibility:** Track which tips have been shown, provide next unused tip  
**Edit When:** Changing progression logic (currently 7 tips per topic)  
**Key Methods:**
- `GetNextUnusedTip(string topic)` – Returns random unused tip
- `GetTipCount(string topic)` – How many tips shown
- `HasAllTipsBeenShown(string topic)` – Check if exhausted
- `ResetTopic(string topic)` – Reset tracking for topic

**Data Structure:** `Dictionary<string, HashSet<int>>` tracking used tip indices  
**Safety:** Critical for tip system; test all scenarios before modifying

#### `src/Services/MemoryService.cs`
**Purpose:** Manages user memory operations  
**Responsibility:** Coordinate with UserMemory for storage operations  
**Edit When:** Adding new memory operations  
**Safety:** Works with UserMemory; ensure consistency

#### `src/Services/VoiceService.cs`
**Purpose:** Audio playback  
**Responsibility:** Play WAV files (greeting on startup)  
**Edit When:** Adding new audio features  
**Safety:** Wrapped in try-catch; graceful failure if audio unavailable

#### `src/Services/ThemeService.cs`
**Purpose:** Console color and theme management  
**Responsibility:** Define colors, emojis, formatting for different message types  
**Edit When:** Changing visual appearance  
**Safety:** Pure cosmetic; safe to modify colors and styling

---

### Model Files (Data Structures)

#### `src/Models/ChatMessage.cs`
**Purpose:** Represents a single message  
**Properties:** Sender, Content, Timestamp, Sentiment  
**Edit When:** Adding new message metadata  
**Safety:** Data structure; safe to add properties

#### `src/Models/SecurityTip.cs`
**Purpose:** Represents a security tip  
**Properties:** Topic, Content, Difficulty, EmergencyLevel  
**Edit When:** Extending tip metadata  
**Safety:** Data structure; safe to extend

#### `src/Models/User.cs`
**Purpose:** Represents user data  
**Properties:** Name, Interests, PreferredTopics  
**Edit When:** Adding user tracking  
**Safety:** Data structure; safe to extend

---

### Helper Files (Utilities)

#### `src/Helpers/InputHelper.cs`
**Purpose:** Parse and validate user input  
**Responsibility:** Keyword detection, input normalization  
**Edit When:** Changing input parsing logic  
**Key Methods:** Input validation, keyword extraction  
**Safety:** Safe to modify parsing rules

#### `src/Helpers/UIHelper.cs`
**Purpose:** Console UI utilities  
**Responsibility:** Typing effects, color output, formatting  
**Edit When:** Changing console display  
**Key Methods:** `PrintWithTypingEffect()`, `ColoredConsoleWrite()`  
**Safety:** Pure display logic; safe to modify

---

### GUI File (Future Development)

#### `src/UI/MainForm.cs`
**Purpose:** Windows Forms GUI  
**Responsibility:** Display messages, capture input, manage UI layout  
**Edit When:** Modifying GUI appearance or interaction  
**Safety:** GUI layer only; keep business logic out of this file  
**Key Rule:** **NEVER put chatbot logic in MainForm.cs**

**What Goes Here:**
- UI element initialization
- Button click handlers
- Text display
- Window management

**What Goes in Chatbot.cs:**
- Message processing
- Response generation
- Service coordination
- Business logic

---

## 🔄 Application Flow

### Terminal Application Flow

```
1. Program.cs Main()
   ↓
2. Initialize Chatbot
   - Create services
   - Load tips
   - Create UserMemory
   ↓
3. Display Title & Audio Greeting
   ↓
4. Get User Name
   ↓
5. Main Conversation Loop
   ├─ Accept user input
   ├─ Pass to Chatbot.HandleUserInput()
   │  ├─ ConversationManager.UpdateContext()
   │  ├─ SentimentAnalyzer.DetectSentiment()
   │  ├─ ConversationManager.UpdateIntent()
   │  ├─ Process intent & generate response
   │  └─ Return response
   ├─ Display response with typing effect
   └─ Loop back to step 5
   ↓
6. Exit on "quit" or "exit"
```

### Message Processing Flow (in Chatbot.HandleUserInput)

```
Input: "Tell me a phishing tip"
  ↓
1. Convert to lowercase
2. ConversationManager.UpdateContext(input)
3. SentimentAnalyzer.DetectSentiment(input)
4. Determine Intent (AskingTip, RequestingEducation, etc.)
5. CheckAndStoreInterests(input)
6. IsFlexibleFollowUpRequest? → HandleTipRequest()
7. IsDeepDiveRequest? → ProvideDeepDiveEducation()
8. Generate Response (topic-specific, sentiment-aware)
  ↓
Output: Personalized response with tip, sentiment-aware tone
```

### Tip Selection Flow

```
User: "Another phishing tip"
  ↓
1. TipTracker.GetNextUnusedTip("phishing")
2. Check HashSet for used indices
3. Generate random index from unused
4. Retrieve tip from TipRepository
5. Get tip count: "3 of 7 tips shown"
6. Return tip + progress
  ↓
Result: Unique tip never shown before + progress feedback
```

---

## 💡 Development Guidelines

### 1. Separation of Concerns

**Keep This Separated:**

| Should Be In | Should NOT Be In |
|-------------|------------------|
| **Chatbot.cs** | UI code, console colors |
| **Services/** | Chatbot orchestration, UI logic |
| **MainForm.cs** | Business logic, chatbot decisions |
| **Helpers/** | Services, business logic |

**Bad Example:**
```csharp
// ❌ DON'T put UI in Chatbot.cs
public void HandleUserInput(string input)
{
    Console.ForegroundColor = ConsoleColor.Green;  // ❌ UI code!
    Console.WriteLine("Processing...");
}
```

**Good Example:**
```csharp
// ✅ Keep UI out of Chatbot.cs
public string HandleUserInput(string input)
{
    string response = ProcessInput(input);
    return response;  // UI layer handles Console.WriteLine()
}
```

### 2. Modular Design

**Keep Methods Small:**
- Each method should do ONE thing
- Method name should describe what it does
- Break complex logic into smaller methods

**Bad Example:**
```csharp
// ❌ Too many responsibilities
public void ProcessUserMessage()
{
    // 1. Parse input
    // 2. Detect sentiment
    // 3. Update memory
    // 4. Check interests
    // 5. Generate response
    // 6. Format output
    // 7. Save to file
}
```

**Good Example:**
```csharp
// ✅ Each method has one responsibility
private string ParseInput(string input) { ... }
private void UpdateMemoryIfNeeded(string input) { ... }
private string GenerateResponse(string input) { ... }
```

### 3. Service Architecture

**Add New Services for New Features:**

If you need to add a major feature:
1. Create a new service class in `src/Services/`
2. Keep it independent of UI
3. Inject into Chatbot via constructor
4. Use through dependency pattern

**Example: Adding a Quiz Service**
```csharp
// src/Services/QuizService.cs
public class QuizService
{
    public Question GetNextQuestion() { ... }
    public bool CheckAnswer(string answer) { ... }
}

// In Chatbot.cs
private readonly QuizService _quizService;

public Chatbot(...)
{
    _quizService = new QuizService();  // Inject
}
```

### 4. Testing Strategy

**Test Each Service Independently:**
- Test `SentimentAnalyzer` with various keywords
- Test `TipTracker` for no duplicate tips
- Test `ConversationManager` for context tracking
- Test `Chatbot` orchestration

**Avoid Testing UI:**
- UI testing is harder; focus on business logic
- Separate logic from UI makes this easier

### 5. Error Handling

**Graceful Failures:**
```csharp
// ✅ Graceful error handling
try
{
    var tip = _tipRepository.GetRandomTip(topic);
    return tip;
}
catch (Exception ex)
{
    Console.WriteLine("Unable to retrieve tip, using default.");
    return "Please ask about a specific topic.";
}
```

**Never Silent Failures:**
```csharp
// ❌ Silent failure (bad)
try { /* something */ }
catch { }  // Hides problems!
```

---

## ➕ Adding New Features

### Scenario 1: Add a New Cybersecurity Topic

1. **Add tips to TipRepository:**
   ```csharp
   // In TipRepository.InitializeTips()
   _topicTips.Add("malware", new List<string>
   {
       "🦠 Tip 1: ...",
       "🦠 Tip 2: ...",
       // ... 7 tips total
   });
   ```

2. **Test with existing code:**
   - User can say "Tell me about malware"
   - TipTracker automatically manages 7-tip progression
   - Done! No code changes needed elsewhere

### Scenario 2: Add Sentiment-Aware Feature

1. **Detect new sentiment in SentimentAnalyzer:**
   ```csharp
   private List<string> _confusedKeywords = new List<string>
   {
       "confused", "unclear", "doesn't make sense", ...
   };
   ```

2. **Update sentiment responses in Chatbot:**
   ```csharp
   if (sentiment == "confused")
   {
       return "Let me explain this clearly and simply. 📖";
   }
   ```

### Scenario 3: Track New User Preference

1. **Update UserMemory:**
   ```csharp
   public void AddPreference(string preference) { ... }
   public List<string> GetPreferences() { ... }
   ```

2. **Update CheckAndStoreInterests in Chatbot:**
   ```csharp
   // Detect and store new preference
   ```

3. **Use in responses:**
   ```csharp
   var preferences = _userMemory.GetPreferences();
   // Generate responses based on preferences
   ```

---

## 🧪 Testing

### Build Project
```bash
dotnet build
```

### Run Project
```bash
dotnet run --project CyberSecurityAwarenessBot.csproj
```

### Test Scenarios

#### Test 1: Tip System
```
Input: "Give me a phishing tip"
Expected: Unique tip shown
Repeat 7 times, verify all different tips
```

#### Test 2: Memory Recall
```
Input: "I'm interested in privacy"
Later: "What am I interested in?"
Expected: Bot recalls privacy
```

#### Test 3: Sentiment Responsiveness
```
Input: "I'm worried about passwords"
Expected: Reassuring, simple tone
```

#### Test 4: Flexible Follow-Ups
```
Input: "Tell me a phishing tip"
Input: "Another one"  ← Natural variation
Expected: Another phishing tip (not random topic)
```

---

## 🔧 Troubleshooting

### Issue: Build Errors

**Problem:** Project won't build
**Solution:**
```bash
dotnet clean
dotnet restore
dotnet build
```

### Issue: Audio File Not Found

**Problem:** Greeting audio doesn't play
**Solution:**
- Check `Audio/greeting.wav` exists
- Verify path in VoiceService.cs
- Application continues even if audio fails (graceful)

### Issue: Duplicate Tips Appearing

**Problem:** User sees same tip twice
**Solution:**
- Check TipTracker logic
- Verify `_usedIndices` HashSet is working
- Ensure TipTracker not reset between requests

### Issue: Sentiment Not Detected

**Problem:** Bot doesn't respond empathetically
**Solution:**
- Add keywords to SentimentAnalyzer
- Check keyword list for typos
- Ensure sentiment is passed to response generation

### Issue: GUI Not Compiling

**Problem:** MainForm.cs has errors
**Solution:**
- Ensure Windows Desktop Runtime installed
- Check target framework: net8.0-windows
- Verify System.Drawing.Common package installed

---

## 📚 Learning Path for New Developers

### Week 1: Understand Architecture
1. Read this guide (you're here!)
2. Review folder structure
3. Run the application
4. Trace one message through HandleUserInput()

### Week 2: Understand Services
1. Study ConversationManager
2. Study TipTracker
3. Study SentimentAnalyzer
4. Trace how they coordinate

### Week 3: Make Simple Changes
1. Add a new cybersecurity topic
2. Add new sentiment keywords
3. Modify a response message
4. Test your changes

### Week 4: Build Features
1. Extend a service
2. Add a new feature
3. Write tests
4. Submit pull request

---

## 🎯 Best Practices Checklist

Before committing code:

- [ ] Code builds without errors
- [ ] No console UI code in business logic classes
- [ ] Each method has single responsibility
- [ ] Services are independent and testable
- [ ] Error handling is graceful
- [ ] Meaningful commit messages
- [ ] Code is readable (clear variable names)
- [ ] Complex logic is commented
- [ ] No hardcoded values (use constants)

---

## 📞 Getting Help

### Common Questions

**Q: Where should I add new conversation logic?**  
A: In `Chatbot.cs` method that handles that interaction type.

**Q: How do I add a new service?**  
A: Create file in `src/Services/`, inject into Chatbot constructor.

**Q: Can I modify the UI without breaking chatbot logic?**  
A: Yes! UI is in `MainForm.cs`; logic is in services. Keep them separate.

**Q: How do I test my changes?**  
A: Build with `dotnet build`, run with `dotnet run`, test manually.

---

## 📖 Additional Resources

- **README.md** – User-facing documentation
- **CyberSecurityAwarenessBot.csproj** – Project configuration
- **.vscode/tasks.json** – Build tasks
- **src/** – Browse the actual code with comments

---

*Last Updated: May 2026*  
*For questions or contributions, see README.md*
