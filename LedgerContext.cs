using EmmettPierson.com.Models;
using Microsoft.EntityFrameworkCore;

namespace EmmettPierson.com
{
    public class LedgerContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public LedgerContext(DbContextOptions<LedgerContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = EmmettPiersonV1");
        }
    }
}
