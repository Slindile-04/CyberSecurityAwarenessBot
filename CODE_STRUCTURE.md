# Code Structure Reference - Implementation Details

## File Structure
```
CyberSecurityAwarenessBot/
├── src/
│   ├── Chatbot.cs              (MODIFIED - Enhanced with tip system)
│   ├── TipRepository.cs        (NEW - Stores all tips)
│   ├── Program.cs              (Unchanged)
│   ├── Helpers/
│   │   ├── InputHelper.cs
│   │   └── UIHelper.cs
├── IMPLEMENTATION_SUMMARY.md   (NEW - Full overview)
├── QUICK_REFERENCE.md          (NEW - Quick reference)
└── CODE_STRUCTURE.md           (NEW - This file)
```

---

## TipRepository.cs - Complete Structure

### Class Definition
```csharp
namespace CyberSecurityAwarenessBot.Core
{
    public class TipRepository
    {
        // Private Fields
        private readonly Dictionary<string, List<string>> _topicTips;
        private readonly Random _random;
        
        // Constructor
        public TipRepository()
        
        // Private Methods
        private Dictionary<string, List<string>> InitializeTips()
        
        // Public Methods
        public string GetRandomTip(string topic)
        public List<string> GetAllTopics()
        public bool HasTopic(string topic)
        public int GetTipCount(string topic)
    }
}
```

### Topics & Tips Structure
```csharp
Dictionary<string, List<string>> {
    "phishing" → List<string> { 7 tips },
    "passwords" → List<string> { 7 tips },
    "2fa" → List<string> { 7 tips },
    "privacy" → List<string> { 7 tips },
    "browsing" → List<string> { 7 tips },
    "ransomware" → List<string> { 7 tips },
    "social engineering" → List<string> { 7 tips },
    "patch management" → List<string> { 7 tips },
    "wifi" → List<string> { 7 tips },
    "password manager" → List<string> { 7 tips }
}
```

### Key Method Signatures

```csharp
// Get random tip for a topic
public string GetRandomTip(string topic)
{
    // Returns: String (tip) or null
    // Throws: Nothing
    // Safety: Returns null if topic doesn't exist or list is empty
}

// Retrieve all available topics
public List<string> GetAllTopics()
{
    // Returns: List<string> of all topic keys
    // Usage: Display in error messages or menus
}

// Validate topic existence
public bool HasTopic(string topic)
{
    // Returns: true if topic exists, false otherwise
    // Usage: Pre-check before calling GetRandomTip()
}

// Count tips available
public int GetTipCount(string topic)
{
    // Returns: Number of tips (0 if topic not found)
    // Usage: Future extensibility (could limit suggestions)
}
```

---

## Chatbot.cs - Modified Structure

### Class Declaration (UPDATED)
```csharp
public class CyberSecurityAwarenessBot
{
    // Existing Fields
    private readonly string _audioPath;
    private readonly InputHelper _inputHelper;
    private readonly string _userName;
    private bool _isRunning;
    
    // NEW FIELDS
    private readonly TipRepository _tipRepository;      // Repository instance
    private string _currentTopic;                       // Active topic (e.g., "phishing")
    private int _tipCount;                              // Tips given counter (0-3)
    
    // Constructor: Updated to initialize new fields
    public CyberSecurityAwarenessBot(string audioPath, string userName)
    
    // Existing Methods: Unchanged
    public void Start()
    public void InteractionLoop()
    private void DisplayMenuOptions()
    
    // MODIFIED METHOD: HandleUserInput()
    // Now checks for tip requests first before menu selection
    
    // NEW METHODS (Step 3 & 4)
    private bool IsTipRequest(string input)
    private bool IsContinuationRequest(string input)
    private string MapKeywordToTopic(string input)
    private void HandleTipRequest(string input)
    private void OfferFullEducation(string topic)
    private void ResetConversationState()
    
    // Existing Methods: Unchanged
    private void RespondToGreeting()
    private void RespondToPurpose()
    // ... all education methods ...
}
```

---

## Method Call Flow Diagram

### For Tip Request: "give me a phishing tip"

```
HandleUserInput("give me a phishing tip")
    ↓
IsTipRequest("give me a phishing tip")  → true
    ↓
HandleTipRequest("give me a phishing tip")
    ├─ MapKeywordToTopic() → "phishing"
    ├─ _tipRepository.HasTopic("phishing") → true
    ├─ _tipRepository.GetRandomTip("phishing") → "🎣 Tip: Always hover over links..."
    ├─ Display tip with typing animation
    ├─ _currentTopic = "phishing"
    ├─ _tipCount = 1
    ├─ _tipCount >= 3? → false
    └─ Display prompt: "Want another tip? (2 more)"
```

### For Continuation: "another tip"

```
HandleUserInput("another tip")
    ↓
IsTipRequest("another tip")  → false
IsContinuationRequest("another tip") && _currentTopic != null → true
    ↓
HandleTipRequest("another tip")
    ├─ MapKeywordToTopic("another tip") → null (ignored)
    ├─ IsContinuationRequest() → true, use _currentTopic ("phishing")
    ├─ _tipRepository.HasTopic("phishing") → true
    ├─ _tipRepository.GetRandomTip("phishing") → Different random tip
    ├─ Display new tip
    ├─ _currentTopic = "phishing" (unchanged)
    ├─ _tipCount = 2
    ├─ _tipCount >= 3? → false
    └─ Display prompt: "Want another tip? (1 more)"
```

### After 3rd Tip: "another tip" (3rd time)

```
HandleUserInput("another tip")
    ↓
HandleTipRequest("another tip")
    ├─ Get random tip
    ├─ Display tip
    ├─ _tipCount = 3
    ├─ _tipCount >= 3? → TRUE
    └─ OfferFullEducation("phishing")
         ├─ Display: "You seem really interested in phishing! 💡"
         ├─ Switch on "phishing" case
         ├─ ProvidePhishingEducation()  [Full education]
         └─ ResetConversationState()
              ├─ _currentTopic = null
              ├─ _tipCount = 0
              └─ Ready for new topic
```

---

## State Transitions

```
INITIAL STATE:
_currentTopic = null
_tipCount = 0

AFTER TIP REQUEST #1:
_currentTopic = "phishing"      [Set to requested topic]
_tipCount = 1                   [Incremented]

AFTER TIP REQUEST #2 (continuation):
_currentTopic = "phishing"      [Same topic]
_tipCount = 2                   [Incremented]

AFTER TIP REQUEST #3 (continuation):
_currentTopic = "phishing"      [Same topic]
_tipCount = 3                   [Incremented]
├─ OfferFullEducation()
└─ ResetConversationState()     [Triggered]

AFTER RESET:
_currentTopic = null            [Cleared]
_tipCount = 0                   [Reset to 0]
└─ Ready for new topic request

IF USER SWITCHES TOPIC:
"tell me about passwords" (while _currentTopic = "phishing")
├─ HandleUserInput detects full education request
├─ ResetConversationState() called first
│  ├─ _currentTopic = null
│  └─ _tipCount = 0
└─ ProvidePasswordEducation() executed
```

---

## Keyword Mapping Reference

### Complete Mapping Dictionary

```csharp
private string MapKeywordToTopic(string input)
{
    var keywordMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "phishing", "phishing" },
        
        { "password", "passwords" },
        { "passwd", "passwords" },
        
        { "2fa", "2fa" },
        { "two-factor", "2fa" },
        { "two factor", "2fa" },
        { "authentication", "2fa" },
        { "auth", "2fa" },
        
        { "privacy", "privacy" },
        { "private", "privacy" },
        
        { "browsing", "browsing" },
        { "browser", "browsing" },
        
        { "ransomware", "ransomware" },
        { "ransom", "ransomware" },
        
        { "social engineering", "social engineering" },
        { "engineer", "social engineering" },
        
        { "patch", "patch management" },
        { "update", "patch management" },
        { "software", "patch management" },
        
        { "wifi", "wifi" },
        { "wi-fi", "wifi" },
        { "network", "wifi" },
        { "public", "wifi" },
        { "vpn", "wifi" },
        
        { "password manager", "password manager" },
        { "manager", "password manager" },
        { "vault", "password manager" }
    };
    
    // Iterates through keywords, returns first match found
    foreach (var kvp in keywordMap)
    {
        if (input.Contains(kvp.Key))
        {
            return kvp.Value;
        }
    }
    
    return null;  // No keyword found
}
```

---

## Edge Case Handling

### Case 1: "another tip" with no active topic
```csharp
if (IsContinuationRequest(input) && string.IsNullOrEmpty(_currentTopic))
{
    // Ask user which topic they want tips about
    UIHelper.DisplayBotMessage(
        $"I'd like to give you another tip, {_userName}, but I need to know which topic first.",
        ConsoleColor.Yellow);
    return;
}
```

### Case 2: Invalid/unknown topic
```csharp
if (!_tipRepository.HasTopic(activeTopic))
{
    UIHelper.DisplayBotMessage(
        $"I don't have tips for '{activeTopic}' yet, {_userName}. Try another topic.",
        ConsoleColor.Yellow);
    return;
}
```

### Case 3: Topic switched mid-conversation
```csharp
// In HandleUserInput, before processing full education:
ResetConversationState();  // Clears _currentTopic and _tipCount
```

---

## Integration with Existing Code

### No Breaking Changes
- ✅ All existing education methods unchanged
- ✅ All existing conversation handlers unchanged
- ✅ Menu display unchanged (extensible if needed)
- ✅ Input validation unchanged
- ✅ UIHelper methods unchanged

### New Dependencies
- TipRepository.cs - Only new file, no changes to existing files required

### Backward Compatibility
- ✅ Full education requests work exactly as before
- ✅ Menu selections work exactly as before
- ✅ Conversational input works exactly as before
- ✅ Tip feature is purely additive

---

## Performance Considerations

### Memory Usage
- **Dictionary**: O(1) lookup for topic existence
- **Random tips**: Random instance created once (reused)
- **State variables**: Minimal (string + int + reference)

### Time Complexity
- **GetRandomTip()**: O(1) - direct array index
- **HasTopic()**: O(1) - dictionary lookup
- **MapKeywordToTopic()**: O(n) - linear search through keywords
  - Typically 5-10 iterations max (acceptable)

### Scalability
- Can easily support 50+ topics
- Can support 20+ tips per topic without performance impact
- State management is minimal and efficient

---

## Testing Helpers

### Unit Test Pseudocode
```csharp
[Test]
public void TestGetRandomTip()
{
    var repo = new TipRepository();
    var tip = repo.GetRandomTip("phishing");
    Assert.IsNotNull(tip);
    Assert.Contains("Tip:", tip);
}

[Test]
public void TestMapKeywordToTopic()
{
    var bot = new ChatBot(...);
    Assert.AreEqual("phishing", bot.MapKeywordToTopic("phishing tip"));
    Assert.AreEqual("passwords", bot.MapKeywordToTopic("password advice"));
    Assert.AreEqual("2fa", bot.MapKeywordToTopic("2fa tips"));
}

[Test]
public void TestTipCountIncrement()
{
    // Simulate 3 tip requests
    // Verify _tipCount increments
    // Verify OfferFullEducation is called on 3rd count
}
```

