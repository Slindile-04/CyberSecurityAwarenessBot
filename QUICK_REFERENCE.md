# Quick Reference Guide - Tip System & Conversation Flow

## 📋 Core Components Overview

### TipRepository.cs - Data Layer
**Purpose**: Centralized storage for all cybersecurity tips

```csharp
public class TipRepository
{
    private readonly Dictionary<string, List<string>> _topicTips;
    
    // Initialize with 10 topics × 5-7 tips each
    private Dictionary<string, List<string>> InitializeTips()
    
    // Get a random tip from a topic
    public string GetRandomTip(string topic)
    
    // Helper methods
    public bool HasTopic(string topic)
    public List<string> GetAllTopics()
    public int GetTipCount(string topic)
}
```

---

### Chatbot.cs - Business Logic Layer

#### State Variables (NEW)
```csharp
private readonly TipRepository _tipRepository;    // Initialized in constructor
private string _currentTopic;                     // Tracks active topic (e.g., "phishing")
private int _tipCount;                            // Counts tips given (0-3)
```

#### Key Methods (NEW)

**1. Detect & Handle Tip Requests**
```csharp
private bool IsTipRequest(string input)
// Detects: "give me a phishing tip", "password tip", "any 2fa advice"

private bool IsContinuationRequest(string input)
// Detects: "another tip", "more", "one more"

private void HandleTipRequest(string input)
// Core logic for retrieving and managing tips
```

**2. Map Keywords to Topics**
```csharp
private string MapKeywordToTopic(string input)
// Maps: "phishing" → "phishing"
//       "password", "passwd" → "passwords"
//       "2fa", "two-factor" → "2fa"
//       "wifi", "vpn" → "wifi"
// etc.
```

**3. Manage Conversation Flow**
```csharp
private void OfferFullEducation(string topic)
// After 3 tips, switches to full education method
// Example: topic = "phishing" → calls ProvidePhishingEducation()

private void ResetConversationState()
// Sets _currentTopic = null, _tipCount = 0
// Called when switching topics or after full education
```

---

## 🎯 Conversation Flow Diagram

```
User Input
    ↓
┌─────────────────────────┐
│ Is it a tip request?    │
│ (contains "tip"/"advice")
└─────────────────────────┘
    ↓ YES            ↓ NO
    │               Continue with existing
    │               menu/conversational logic
    ↓
┌──────────────────────────┐
│ Extract topic keyword    │
│ (phishing, password, etc)│
└──────────────────────────┘
    ↓
┌──────────────────────────┐
│ Is it a continuation?    │
│ ("another", "more")      │
└──────────────────────────┘
    ↓ YES            ↓ NO
    │               Get new topic
    │ Use _currentTopic
    │               from input
    ↓               ↓
┌──────────────────────────┐
│ HasTopic?                │
│ (_tipRepository)         │
└──────────────────────────┘
    ↓ YES            ↓ NO
    │               Show available topics
    │               & ask to try again
    ↓
┌──────────────────────────┐
│ Get random tip           │
│ _tipRepository           │
│ .GetRandomTip(topic)     │
└──────────────────────────┘
    ↓
┌──────────────────────────┐
│ Display tip              │
│ Update state:            │
│ _currentTopic = topic    │
│ _tipCount++              │
└──────────────────────────┘
    ↓
┌──────────────────────────┐
│ _tipCount >= 3?          │
└──────────────────────────┘
    ↓ YES            ↓ NO
    │               Prompt for another tip
    │               Show remaining count
    ↓
┌──────────────────────────┐
│ ProvidesFullEducation()  │
│ - Call education method  │
│ - Reset state            │
└──────────────────────────┘
    ↓
Return to menu
```

---

## 💬 User Interaction Examples

### Example 1: Tips Leading to Full Education
```
User: "give me a phishing tip"
Bot:  🎣 Tip: Always hover over links in emails before clicking to verify URL

Bot:  Want another tip about phishing? (2 more tips before full breakdown)

User: "another tip"
Bot:  🎣 Tip: Be suspicious of urgent requests for passwords or personal info

Bot:  Want another tip about phishing? (1 more tips before full breakdown)

User: "more"
Bot:  🎣 Tip: Check the sender's email address carefully—attackers often use...

Bot:  You seem really interested in phishing! 💡
      Let me give you a detailed breakdown...
      
      [Full phishing education provided]
      [State reset: _currentTopic = null, _tipCount = 0]
```

### Example 2: Topic Switching
```
User: "password tip"
Bot:  🔐 Tip: Use at least 12 characters combining...

User: "tell me about ransomware"
Bot:  [State resets]
      [Full ransomware education provided]
```

### Example 3: Edge Cases Handled
```
User: "another tip"  [but no active topic]
Bot:  I'd like to give you another tip, which topic first?
      (phishing, passwords, 2fa, privacy, browsing, ransomware, social engineering, 
       patch management, wifi, password manager)

User: "unknown topic"
Bot:  I'm not sure which topic you're interested in. Try asking about: 
      phishing, passwords, 2fa, privacy, [...rest of topics...]
```

---

## 🔧 Available Topics & Keywords

| Topic | Primary Keyword | Aliases |
|-------|-----------------|---------|
| Phishing | phishing | phish, email |
| Passwords | password | passwd, pwd |
| 2FA | 2fa | two-factor, authentication, auth |
| Privacy | privacy | private, personal |
| Browsing | browsing | browser, web |
| Ransomware | ransomware | ransom |
| Social Engineering | social engineering | engineer |
| Patch Management | patch | update, software |
| Wi-Fi | wifi | wi-fi, vpn, network, public |
| Password Manager | password manager | manager, vault |

---

## 📊 Tip Count Tracking

```
TipCount = 0  →  Initial state
TipCount = 1  →  First tip given → "Want another tip? (2 more)"
TipCount = 2  →  Second tip      → "Want another tip? (1 more)"
TipCount = 3  →  Third tip       → "You seem really interested..."
                                   Full education provided
                                   State reset to TipCount = 0
```

---

## 🚀 Adding a New Topic (Step-by-Step)

### Step 1: Add to TipRepository.cs (InitializeTips method)
```csharp
{
    "network security", new List<string>
    {
        "🛡️ Tip: Implement network segmentation to isolate critical systems...",
        "🛡️ Tip: Use firewalls to control traffic between network zones...",
        "🛡️ Tip: Monitor network traffic for unusual patterns...",
        "🛡️ Tip: Regular network assessments help identify vulnerabilities...",
        "🛡️ Tip: Disable unnecessary network services to reduce attack surface...",
        "🛡️ Tip: Use VLANs to separate different network segments...",
    }
}
```

### Step 2: Add to MapKeywordToTopic() method
```csharp
{ "network security", "network security" },
{ "network", "network security" },
{ "segmentation", "network security" }
```

### Step 3: Add to HandleUserInput() switch statement
```csharp
case "11":
case "network security":
case "network":
case "tell me about network security":
    ProvideNetworkSecurityEducation();
    return;
```

### Step 4: Add education method
```csharp
private void ProvideNetworkSecurityEducation()
{
    Console.WriteLine();
    UIHelper.DisplayTypingIndicator();
    
    string[] networkContent = new string[]
    {
        "┌───── NETWORK SECURITY EDUCATION ──────────┐",
        $"│ User: {_userName.PadRight(35)} │",
        // ... education content ...
    };
    
    foreach (var line in networkContent)
    {
        UIHelper.DisplayBotMessage(line, ConsoleColor.Yellow);
    }
    UIHelper.AutoScroll();
}
```

### Step 5: Update DisplayMenuOptions() (optional)
```csharp
UIHelper.PrintColoredLine("║  11. Network Security                       ║", ConsoleColor.Cyan);
```

### Step 6: Update DisplayHelpMessage() (optional)
```csharp
UIHelper.DisplayBotMessage("   • Type '11' or 'network security' - Learn about network security", ConsoleColor.White);
```

---

## ✅ Verification Checklist

After implementation, verify:
- [ ] Build compiles without errors
- [ ] Can request tips: "give me a phishing tip"
- [ ] Can request continuation: "another tip", "more", "one more"
- [ ] Tips are different on repeated requests (randomization works)
- [ ] After 3 tips, full education is provided
- [ ] State resets after full education
- [ ] Can switch topics mid-conversation
- [ ] Edge cases handled (no active topic, unknown topic)
- [ ] Keyword mapping works for multiple variations
- [ ] Case-insensitive input works

---

## 📝 Code Metrics

| Metric | Value |
|--------|-------|
| Lines added (TipRepository.cs) | ~250 |
| Lines added (Chatbot.cs modifications) | ~350 |
| Topics supported | 10 |
| Tips per topic | 5-7 |
| Total tips | 66 |
| New public methods | 4 |
| New private methods | 6 |
| Build status | ✅ Success |
| Compilation errors | 0 |

