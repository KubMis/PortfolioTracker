using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Model;

namespace PortfolioTracker.PortfolioDbContext
{
    public class PortfolioTrackerContext: DbContext
    {
        public DbSet<Portfolio> portfolios { get; set; }
        public DbSet<Ticker> tickers { get; set; }
        public DbSet<PortfolioTicker> portfolioTickers { get; set; }
        public string DbPath { get; }

        public PortfolioTrackerContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "blogging.db");
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
