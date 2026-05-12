namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// MemoryService.cs - Manages user memory and conversation history
    /// 
    /// Features:
    /// - Store and retrieve user information
    /// - Maintain conversation history
    /// - Provide user context for personalized responses
    /// - Persist memory to storage
    /// </summary>
    public interface IMemoryService
    {
        void StoreMemory(string key, string value);
        string RetrieveMemory(string key);
        void ClearMemory();
        Dictionary<string, string> GetAllMemories();
    }

    public class MemoryService : IMemoryService
    {
        private Dictionary<string, string> _memory;
        private readonly string _dataPath;

        public MemoryService(string dataPath = "data")
        {
            _dataPath = dataPath;
            _memory = new Dictionary<string, string>();
            Directory.CreateDirectory(_dataPath);
        }

        public void StoreMemory(string key, string value)
        {
            _memory[key] = value;
        }

        public string RetrieveMemory(string key)
        {
            if (_memory.TryGetValue(key, out var value))
                return value;

            return string.Empty;
        }

        public void ClearMemory()
        {
            _memory.Clear();
        }

        public Dictionary<string, string> GetAllMemories()
        {
            return new Dictionary<string, string>(_memory);
        }
    }
}
