using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Telegram.Bot.Types;

namespace BSUIRQueueTelegramBot.Data

{
    public class ApplicationContext : DbContext
    {
        public DbSet<Record> records { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>().HasKey(r => new { r.Place,r.Subject});
        }
    }
}
