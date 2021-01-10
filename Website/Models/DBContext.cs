using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Website.Models
{
    public class DBContext : IdentityDbContext<User>
    {
        public DbSet<Streamer> Streamers { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        private readonly ILoggerFactory _loggerFactory;

        public DBContext() { }

        public DBContext(DbContextOptions<DBContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Allow null if you are using an IDesignTimeDbContextFactory
            if (_loggerFactory != null)
            {
                if (Debugger.IsAttached)
                {
                    // Probably shouldn't log sql statements in production
                    optionsBuilder.UseLoggerFactory(_loggerFactory);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseIdentityColumns();
            builder.HasPostgresExtension("pg_trgm");

            builder.Entity<Streamer>().Property(x => x.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Alert>().Property(x => x.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
