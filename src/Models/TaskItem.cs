using System;

namespace CyberSecurityAwarenessBot.Models
{
    /// <summary>
    /// Represents a cybersecurity task item stored in the SQLite database.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Primary key for the task.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title of the task.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed description of the task.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Optional reminder date for the task.
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Whether the task has been completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// When the task was created.
        /// </summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
