using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace Telegram_bot
{
    class DataBaseBot : DbContext
    {
        public DataBaseBot() : base("DbConnectionString")
        {
        }
        public DbSet<Crocodile.CrocoChatIDTable> CrocoChatIDTables { get; set; }
        public DbSet<MessagesAndRating.RatingTable> RatingTables { get; set; }
        public DbSet<MessagesAndRating.CountMessageTable> CountMessageTables { get; set; }
        public DbSet<MessagesAndRating.SecretWordTable> SecretWordTables { get; set; }
        public DbSet<Reports.AdminTable> AdminTables { get; set; }
        public DbSet<RestrictMedia.RestrictMediaTable> restrictMediaTables { get; set; }
        public DbSet<RollGame.RollEventTable> rollEventTables { get; set; }
    }
}
