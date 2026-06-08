using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// ActivityLogService.cs - Records and retrieves user and chatbot activity entries.
    /// </summary>
    public class ActivityLogService
    {
        private readonly List<ActivityLogEntry> _entries;

        public int PageSize { get; }

        public ActivityLogService(int pageSize = 5)
        {
            PageSize = pageSize > 0 ? pageSize : 5;
            _entries = new List<ActivityLogEntry>();
        }

        /// <summary>
        /// Adds a new activity entry to the log.
        /// Newest entries appear first.
        /// </summary>
        public void AddEntry(string actionType, string description)
        {
            if (string.IsNullOrWhiteSpace(actionType) || string.IsNullOrWhiteSpace(description))
                return;

            var entry = new ActivityLogEntry(DateTime.Now, actionType.Trim(), description.Trim());
            _entries.Insert(0, entry);
        }

        /// <summary>
        /// Retrieves a page of activity entries.
        /// </summary>
        public IReadOnlyList<ActivityLogEntry> GetEntries(int page = 1)
        {
            if (page < 1)
                page = 1;

            return _entries
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Returns true if the log contains any entries.
        /// </summary>
        public bool HasEntries => _entries.Count > 0;

        /// <summary>
        /// Returns the total number of log entries.
        /// </summary>
        public int GetTotalEntries() => _entries.Count;

        /// <summary>
        /// Returns the total number of pages available.
        /// </summary>
        public int GetTotalPages()
        {
            if (_entries.Count == 0)
                return 0;

            return (_entries.Count + PageSize - 1) / PageSize;
        }

        /// <summary>
        /// Returns true if there are older entries beyond the given page.
        /// </summary>
        public bool HasMorePages(int page)
        {
            return page < GetTotalPages();
        }
    }
}
