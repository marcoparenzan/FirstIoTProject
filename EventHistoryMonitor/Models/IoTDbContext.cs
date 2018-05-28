using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventHistoryMonitor.Models
{
    public class IoTDbContext: DbContext
    {
        private string _connectionString;

        public IoTDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Average> Averages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
