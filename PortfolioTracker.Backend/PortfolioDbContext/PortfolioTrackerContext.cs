using Microsoft.EntityFrameworkCore;
using PortfolioTracker.PortfolioTracker.Backend.Model;

namespace PortfolioTracker.PortfolioTracker.Backend.PortfolioDbContext
{
    public class PortfolioTrackerContext : DbContext
    {
        public DbSet<Portfolio> portfolios { get; set; }
        public DbSet<Ticker> tickers { get; set; }
        public DbSet<PortfolioTicker> portfolioTickers { get; set; }
        public string DbPath { get; }

        public PortfolioTrackerContext(DbContextOptions<PortfolioTrackerContext> options)
        : base(options)
        {
        }
    }
}
