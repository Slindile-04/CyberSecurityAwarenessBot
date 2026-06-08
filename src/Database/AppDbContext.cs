using System;
using System.IO;
using CyberSecurityAwarenessBot.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberSecurityAwarenessBot.Database
{
    /// <summary>
    /// AppDbContext.cs - SQLite database context for the task assistant.
    /// </summary>
    public class AppDbContext : DbContext
    {
        private readonly string _databasePath;

        /// <summary>
        /// Task table mapped to the SQLite database.
        /// </summary>
        public DbSet<TaskItem> Tasks { get; set; } = null!;

        /// <summary>
        /// Default constructor uses a file named taskassistant.db in the application folder.
        /// </summary>
        public AppDbContext()
            : this(Path.Combine(AppContext.BaseDirectory, "taskassistant.db"))
        {
        }

        /// <summary>
        /// Constructor accepting a custom database path.
        /// </summary>
        /// <param name="databasePath">The SQLite database file path.</param>
        public AppDbContext(string databasePath)
        {
            _databasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired();
                entity.Property(t => t.Description).IsRequired();
                entity.Property(t => t.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
