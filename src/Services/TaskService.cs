using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CyberSecurityAwarenessBot.Database;
using CyberSecurityAwarenessBot.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberSecurityAwarenessBot.Services
{
    /// <summary>
    /// TaskService.cs - Manages cybersecurity task items and reminders.
    /// </summary>
    public class TaskService
    {
        private readonly string _databasePath;
        private readonly List<TaskActionLogEntry> _actionLogEntries;

        /// <summary>
        /// Constructor - Initializes the task service and ensures the SQLite database exists.
        /// </summary>
        /// <param name="databasePath">Optional custom SQLite database path.</param>
        public TaskService(string? databasePath = null)
        {
            _databasePath = databasePath ?? Path.Combine(AppContext.BaseDirectory, "taskassistant.db");
            _actionLogEntries = new List<TaskActionLogEntry>();

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        private AppDbContext CreateContext()
        {
            return new AppDbContext(_databasePath);
        }

        /// <summary>
        /// Adds a new task to the database.
        /// </summary>
        public TaskItem AddTask(string title, string description, DateTime? reminderDate = null)
        {
            using var context = CreateContext();
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                IsCompleted = false,
                DateCreated = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            context.SaveChanges();
            RecordAction(task.Id, TaskActionType.Added, $"Added task '{title}'");
            return task;
        }

        /// <summary>
        /// Deletes a task by ID.
        /// </summary>
        public bool DeleteTask(int taskId)
        {
            using var context = CreateContext();
            var task = context.Tasks.Find(taskId);
            if (task == null)
                return false;

            context.Tasks.Remove(task);
            context.SaveChanges();
            RecordAction(taskId, TaskActionType.Deleted, $"Deleted task '{task.Title}'");
            return true;
        }

        /// <summary>
        /// Marks a task as completed.
        /// </summary>
        public bool MarkTaskCompleted(int taskId)
        {
            using var context = CreateContext();
            var task = context.Tasks.Find(taskId);
            if (task == null)
                return false;

            task.IsCompleted = true;
            context.SaveChanges();
            RecordAction(taskId, TaskActionType.Completed, $"Completed task '{task.Title}'");
            return true;
        }

        /// <summary>
        /// Sets a reminder date for a task.
        /// </summary>
        public bool SetReminder(int taskId, DateTime reminderDate)
        {
            using var context = CreateContext();
            var task = context.Tasks.Find(taskId);
            if (task == null)
                return false;

            task.ReminderDate = reminderDate;
            context.SaveChanges();
            RecordAction(taskId, TaskActionType.ReminderSet, $"Set reminder for '{task.Title}' to {reminderDate:yyyy-MM-dd HH:mm}");
            return true;
        }

        /// <summary>
        /// Gets all tasks from the database.
        /// </summary>
        public List<TaskItem> GetAllTasks()
        {
            using var context = CreateContext();
            return context.Tasks
                .OrderBy(t => t.DateCreated)
                .ToList();
        }

        /// <summary>
        /// Gets all pending, incomplete tasks.
        /// </summary>
        public List<TaskItem> GetPendingTasks()
        {
            using var context = CreateContext();
            return context.Tasks
                .Where(t => !t.IsCompleted)
                .OrderBy(t => t.DateCreated)
                .ToList();
        }

        /// <summary>
        /// Returns tasks with reminders that are due on or before the provided date.
        /// </summary>
        public List<TaskItem> GetDueReminders(DateTime now)
        {
            using var context = CreateContext();
            return context.Tasks
                .Where(t => t.ReminderDate.HasValue && t.ReminderDate <= now && !t.IsCompleted)
                .OrderBy(t => t.ReminderDate)
                .ToList();
        }

        /// <summary>
        /// Retrieves a task by ID.
        /// </summary>
        public TaskItem? GetTaskById(int taskId)
        {
            using var context = CreateContext();
            return context.Tasks.Find(taskId);
        }

        /// <summary>
        /// Returns an immutable snapshot of the task action log.
        /// </summary>
        public IReadOnlyList<TaskActionLogEntry> GetActionLogEntries()
        {
            return _actionLogEntries.AsReadOnly();
        }

        private void RecordAction(int taskId, TaskActionType actionType, string details)
        {
            _actionLogEntries.Add(new TaskActionLogEntry(taskId, actionType, DateTime.UtcNow, details));
        }

        /// <summary>
        /// Converts raw task titles into a friendly title case format.
        /// </summary>
        public static string ConvertTitleToFriendlyText(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            var cleaned = title.Trim();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
        }
    }

    /// <summary>
    /// Compliance type for recorded task actions.
    /// </summary>
    public enum TaskActionType
    {
        Added,
        ReminderSet,
        Completed,
        Deleted
    }

    /// <summary>
    /// Represents a log entry for task actions.
    /// </summary>
    public record TaskActionLogEntry(int TaskId, TaskActionType ActionType, DateTime Timestamp, string Details);
}
