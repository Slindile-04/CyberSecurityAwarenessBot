# CyberSecurityAwarenessBot

**An intelligent cybersecurity awareness chatbot built with C# and .NET 8, designed to educate users about online threats, security best practices, and personal cyber hygiene through an interactive graphical user interface.**

---

# 📋 Project Overview

CyberSecurityAwarenessBot is an educational chatbot application developed to help users learn about cybersecurity in an engaging and interactive way.

The application combines cybersecurity awareness training, personalized conversations, memory recall, sentiment detection, task management, quizzes, and activity logging into a single user-friendly platform.

The chatbot provides guidance on common cybersecurity topics while helping users organize cybersecurity-related tasks and track their learning progress.

---

# 🎯 Objectives

The chatbot aims to:

* Educate users about cybersecurity threats and defenses
* Encourage safe online behavior
* Provide personalized learning experiences
* Track user interactions and learning progress
* Allow users to manage cybersecurity-related tasks
* Reinforce knowledge through quizzes and interactive learning

---

# 🚀 Features

## 🔒 Cybersecurity Education

The chatbot teaches users about:

1. Phishing
2. Password Security
3. Two-Factor Authentication (2FA)
4. Privacy Protection
5. Secure Browsing
6. Ransomware
7. Social Engineering
8. Patch Management
9. Public Wi-Fi Safety
10. Password Managers

Each topic contains educational content and multiple security tips.

---

## 🧠 Memory Recall System

The chatbot can:

* Remember user interests
* Store favorite cybersecurity topics
* Recall previously stored preferences
* Personalize future conversations

Example:

User:

> I am interested in phishing

Bot:

> Got it! I'll remember that you're interested in phishing.

Later:

User:

> What topic am I interested in?

Bot:

> You told me you're interested in phishing.

---

## 😊 Sentiment Detection

The chatbot analyzes user input and adapts responses based on emotional tone.

Supported sentiments:

* Worried
* Curious
* Frustrated
* Positive
* Neutral

Example:

User:

> I'm worried about privacy

Bot:

> I can see you're worried about privacy. Let's address that together!

---

## 💬 Conversation Flow Management

The chatbot maintains context across multiple messages.

It can:

* Track the current topic
* Understand follow-up requests
* Continue discussions naturally
* Handle conversational interactions more effectively

---

## 🎮 Cybersecurity Quiz Game

The application includes an interactive quiz system.

Features:

* 20 cybersecurity questions
* Questions cover all 10 cybersecurity topics
* Multiple-choice and True/False questions
* Randomized question selection
* Score tracking
* Resume quiz functionality
* Performance feedback based on results

Example feedback:

* 10/10 → "Excellent! You're a cybersecurity pro!"
* 8-9/10 → "Great job! You have strong cybersecurity knowledge."
* Below 5/10 → "Keep learning and you'll improve your cybersecurity skills."

---

## ✅ Task Assistant & Reminder System

Users can manage cybersecurity-related tasks.

Examples:

* Enable Two-Factor Authentication
* Review Privacy Settings
* Update Passwords
* Install Security Updates

Features:

* Add tasks
* View tasks
* Delete tasks
* Mark tasks as completed
* Set reminders
* Store tasks in a SQLite database

Example:

User:

> Add task - Review Privacy Settings

Bot:

> Task added successfully. Would you like a reminder?

User:

> Yes, remind me in 3 days

Bot:

> Got it! I'll remind you in 3 days.

---

## 📜 Activity Log System

The chatbot records significant user actions.

Tracked actions include:

* Topics learned
* Tips requested
* Tasks added
* Tasks completed
* Tasks deleted
* Reminders created
* Quiz attempts
* Quiz scores

Users can view recent activity using:

> Show activity log

The chatbot displays 5 entries at a time and allows users to view more if additional records exist.

---

## 🖥️ Graphical User Interface (GUI)

The application includes a cybersecurity-themed Windows Forms GUI.

Features:

* Dark cybersecurity-themed design
* Green and white terminal-inspired color scheme
* Chat-style conversation display
* User-friendly controls
* ASCII-art welcome screen
* Personalized onboarding experience
* Audio greeting support

---

# 💻 Technologies Used

| Component       | Technology               |
| --------------- | ------------------------ |
| Language        | C# 12                    |
| Framework       | .NET 8                   |
| GUI             | Windows Forms            |
| Database        | SQLite                   |
| ORM             | Entity Framework Core    |
| Audio           | System.Media.SoundPlayer |
| IDE             | Visual Studio Code       |
| Version Control | Git & GitHub             |

---

# 🏗️ Project Structure

```text
src/
│
├── UI/
│   └── MainForm.cs
│
├── Database/
│   └── AppDbContext.cs
│
├── QuizGame/
│   ├── QuizManager.cs
│   └── QuizQuestion.cs
│
├── Services/
│   ├── ConversationManager.cs
│   ├── MemoryService.cs
│   ├── SentimentAnalyzer.cs
│   ├── VoiceService.cs
│   ├── ThemeService.cs
│   ├── TaskService.cs
│   └── ActivityLogService.cs
│
├── Models/
│   ├── UserMemory.cs
│   ├── TaskItem.cs
│   ├── ActivityLogEntry.cs
│   └── SecurityTip.cs
│
├── Chatbot.cs
├── TipRepository.cs
└── Program.cs
```

---

# 🔧 Key Components

## MainForm.cs

Responsible for:

* Displaying the GUI
* Handling user input
* Displaying chatbot responses
* Managing chat interactions

---

## ConversationManager.cs

Responsible for:

* Conversation state tracking
* Context management
* Topic continuation
* Follow-up request handling

---

## MemoryService.cs

Responsible for:

* Storing user preferences
* Memory recall
* User personalization

---

## UserMemory.cs

Responsible for:

* Managing stored interests
* Tracking user preferences
* Session-based memory storage

---

## SentimentAnalyzer.cs

Responsible for:

* Detecting user sentiment
* Categorizing emotions
* Supporting adaptive responses

---

## TaskService.cs

Responsible for:

* Creating tasks
* Updating tasks
* Deleting tasks
* Retrieving task information

---

## ActivityLogService.cs

Responsible for:

* Recording important actions
* Retrieving activity history
* Supporting activity log pagination

---

## QuizManager.cs

Responsible for:

* Quiz generation
* Question management
* Score tracking
* Quiz progression

---

## AppDbContext.cs

Responsible for:

* SQLite database access
* Entity management
* Data persistence

---

# 📊 Current Status

## ✅ Completed

* GUI implementation
* Cybersecurity learning system
* Memory recall
* Sentiment detection
* Conversation management
* Quiz system
* Task assistant
* Reminder system
* SQLite database integration
* Activity logging
* Personalized responses

---

# 🛠️ Installation

## Requirements

* .NET 8 SDK
* Visual Studio Code or Visual Studio
* Git

## Clone Repository

```bash
git clone https://github.com/Slindile-04/CyberSecurityAwarenessBot.git
cd CyberSecurityAwarenessBot
```

## Restore Packages

```bash
dotnet restore
```

## Build Project

```bash
dotnet build
```

## Run Application

```bash
dotnet run
```

---

# 📖 Documentation

* README.md — Project Overview
* DEVELOPER_GUIDE.md — Technical Documentation

---

# 🎓 Academic Context

**Module:** PROG6221 – Programming 2A

This project demonstrates:

* Object-Oriented Programming
* GUI Development
* Database Integration
* Entity Framework Core
* Data Persistence
* Collections (Lists & Dictionaries)
* Sentiment Analysis
* Memory Management
* Activity Tracking
* Software Architecture Principles

---

# 🔗 GitHub Repository

Repository:

https://github.com/Slindile-04/CyberSecurityAwarenessBot

---

Built for cybersecurity awareness, education, and practical software development experience.
