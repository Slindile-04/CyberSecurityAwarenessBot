# CyberSecurityAwarenessBot - Developer Guide

**Technical documentation for developers, contributors, and assessors.**

---

# 📚 Table of Contents

1. Project Overview
2. Folder Structure
3. System Architecture
4. Core Components
5. Application Flow
6. Database Layer
7. Quiz System
8. Activity Log System
9. Task Assistant System
10. Memory & Sentiment Systems
11. Development Guidelines
12. Testing
13. Future Enhancements

---

# 📖 Project Overview

The CyberSecurityAwarenessBot is a Windows Forms (WinForms) application developed in C# using .NET 8.

The chatbot teaches users about cybersecurity through:

* Educational cybersecurity topics
* Memory recall
* Sentiment detection
* Interactive quizzes
* Cybersecurity task management
* Activity logging
* Natural language conversation simulation

The application follows a modular architecture where responsibilities are separated into services, models, database components, and UI components.

---

# 📁 Folder Structure

```text
src/
├── Program.cs
├── Chatbot.cs
├── TipRepository.cs
│
├── UI/
│   └── MainForm.cs
│
├── Services/
│   ├── ConversationManager.cs
│   ├── MemoryService.cs
│   ├── ActivityLogService.cs
│   ├── TaskService.cs
│   ├── ThemeService.cs
│   ├── VoiceService.cs
│   ├── TipTracker.cs
│   └── SentimentAnalyzer.cs
│
├── Database/
│   └── AppDbContext.cs
│
├── QuizGame/
│   ├── QuizManager.cs
│   └── QuizQuestion.cs
│
├── Models/
│   ├── TaskItem.cs
│   ├── ActivityLogEntry.cs
│   ├── User.cs
│   ├── ChatMessage.cs
│   └── SecurityTip.cs
│
└── Helpers/
    ├── InputHelper.cs
    └── UIHelper.cs
```

---

# 🏗️ System Architecture

The application follows a layered architecture.

| Layer            | Responsibility                           |
| ---------------- | ---------------------------------------- |
| UI Layer         | User interaction and display             |
| Chatbot Layer    | Input processing and response generation |
| Service Layer    | Business logic                           |
| Database Layer   | Data persistence                         |
| Model Layer      | Data structures                          |
| Repository Layer | Cybersecurity content storage            |

---

# 💻 Core Components

## Program.cs

Application entry point.

Responsibilities:

* Launch application
* Start MainForm
* Initialize dependencies

---

## MainForm.cs

Main graphical interface.

Responsibilities:

* Display chatbot responses
* Accept user input
* Handle button clicks
* Manage chat display
* Apply themes

Important Rule:

Business logic should not be implemented directly inside MainForm.

---

## Chatbot.cs

Central chatbot engine.

Responsibilities:

* Process user messages
* Route requests
* Coordinate services
* Generate responses
* Handle educational content

This file acts as the brain of the application.

---

## TipRepository.cs

Stores all cybersecurity learning content.

Responsibilities:

* Store cybersecurity topics
* Store educational breakdowns
* Store cybersecurity tips

Current Topics:

1. Phishing
2. Passwords
3. Two-Factor Authentication
4. Privacy
5. Secure Browsing
6. Ransomware
7. Social Engineering
8. Patch Management
9. Public WiFi
10. Password Managers

---

# ⚙️ Service Layer

## ConversationManager.cs

Tracks conversation context.

Responsibilities:

* Track current topic
* Track previous topic
* Track user intent
* Support follow-up questions
* Enable natural conversation flow

Example:

User: Tell me about phishing

User: Tell me more

The chatbot understands that "more" refers to phishing.

---

## MemoryService.cs

Handles memory operations.

Responsibilities:

* Store user preferences
* Retrieve stored information
* Manage memory interactions

---

## UserMemory.cs

Stores user interests and preferences.

Responsibilities:

* Remember favorite topics
* Remember user interests
* Support personalized responses

Example:

User: I'm interested in phishing

Later:

User: What topic do I like?

Bot recalls phishing.

---

## SentimentAnalyzer.cs

Detects emotional tone using keyword matching.

Detected Sentiments:

* Worried
* Curious
* Frustrated
* Positive
* Neutral

Example:

User: I'm worried about privacy

Bot responds empathetically before providing information.

---

## ThemeService.cs

Manages application styling.

Responsibilities:

* Cybersecurity-themed colours
* Dark mode appearance
* Consistent UI styling

Theme Choices:

* Black background
* Green chatbot responses
* White user messages

This creates a terminal-inspired cybersecurity aesthetic.

---

## VoiceService.cs

Handles startup audio.

Responsibilities:

* Play greeting audio
* Handle audio exceptions gracefully

---

## TipTracker.cs

Tracks cybersecurity tips shown to users.

Responsibilities:

* Prevent duplicate tips
* Manage tip progression
* Track topic learning progress

---

# 🗄️ Database Layer

## AppDbContext.cs

SQLite database manager.

Responsibilities:

* Create database
* Manage tables
* Store tasks
* Store reminders

Benefits:

* Lightweight
* No server required
* Easy deployment
* Ideal for desktop applications

---

# 📝 Task Assistant System

## TaskService.cs

Manages cybersecurity-related tasks.

Features:

* Add tasks
* View tasks
* Complete tasks
* Delete tasks
* Set reminders

Example Tasks:

* Enable Two-Factor Authentication
* Review Privacy Settings
* Update Password Manager
* Check Software Updates

Data is persisted using SQLite.

---

## TaskItem.cs

Represents a cybersecurity task.

Properties:

* Id
* Title
* Description
* ReminderDate
* IsCompleted

---

# 🎮 Quiz System

## QuizManager.cs

Controls quiz functionality.

Responsibilities:

* Start quiz sessions
* Randomize questions
* Track score
* Resume quizzes
* Generate feedback

Features:

* 20 questions
* Multiple Choice
* True/False
* Score tracking

---

## QuizQuestion.cs

Represents an individual quiz question.

Properties:

* Question
* Topic
* Options
* CorrectAnswer
* Explanation

Topics Covered:

* Phishing
* Passwords
* 2FA
* Privacy
* Browsing
* Ransomware
* Social Engineering
* Patch Management
* Public WiFi
* Password Managers

---

# 📊 Activity Log System

## ActivityLogService.cs

Tracks important user actions.

Responsibilities:

* Record events
* Display recent activity
* Support pagination

Examples Logged:

* Task added
* Task deleted
* Reminder created
* Quiz started
* Quiz completed
* Topic viewed
* Tips requested

---

## ActivityLogEntry.cs

Represents a log entry.

Properties:

* Id
* ActionDescription
* Timestamp

---

# 🔄 Application Flow

```text
Program.cs
      ↓
Launch MainForm
      ↓
Initialize Services
      ↓
User enters message
      ↓
MainForm sends message to Chatbot
      ↓
Chatbot determines intent
      ↓
Appropriate Service executes action
      ↓
Response generated
      ↓
Activity recorded
      ↓
Response displayed in GUI
```

---

# 🧠 Memory & Sentiment Systems

## Memory Recall

The chatbot can remember:

* Favourite cybersecurity topics
* User interests
* Previous conversation context

This enables more personalized conversations.

---

## Sentiment Detection

The chatbot adapts responses based on emotion.

Examples:

Worried → Reassuring response

Curious → Detailed explanation

Frustrated → Simplified explanation

Positive → Encouraging response

---

# 🧪 Testing

Recommended Tests:

### Educational Topics

Verify all 10 topics respond correctly.

### Memory Recall

Store an interest and verify recall.

### Sentiment Detection

Test worried, curious, frustrated, and positive phrases.

### Quiz System

Verify score calculation and feedback.

### Task Assistant

Verify:

* Add task
* Complete task
* Delete task
* Reminder creation

### Activity Log

Verify actions are correctly recorded.

---

# 💡 Development Guidelines

## Separation of Concerns

Keep responsibilities separated.

### UI Layer

Handles:

* Display
* Input
* User interaction

### Service Layer

Handles:

* Business logic
* Data processing

### Database Layer

Handles:

* Persistence
* Queries

---

## Code Quality Principles

* Use meaningful names
* Keep methods small
* Avoid duplicated logic
* Comment complex functionality
* Handle exceptions gracefully

---

# 🚀 Future Enhancements

Potential future improvements:

* Desktop reminder notifications
* Persistent user memory across sessions
* Additional cybersecurity topics
* Analytics dashboard
* Advanced NLP simulation
* Export activity logs
* User progress tracking
* Multi-language support

---

# 🎯 Summary

The CyberSecurityAwarenessBot combines cybersecurity education, memory recall, sentiment awareness, task management, quiz functionality, and activity tracking into a single desktop application.

The project demonstrates:

* Object-Oriented Programming
* GUI Development
* Database Integration
* Modular Software Design
* Data Persistence
* User Interaction Design
* Cybersecurity Awareness Education

---

Last Updated: June 2026
