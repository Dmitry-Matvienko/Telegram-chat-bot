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

        public DbSet<Rate> Rates{ get; set; }
        public DbSet<CountMessage> countMessages{ get; set; }
        public DbSet<SecretWords> secretWords { get; set; }
    }
}
