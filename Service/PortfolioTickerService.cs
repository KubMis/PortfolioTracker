using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Model;
using PortfolioTracker.PortfolioDbContext;

namespace PortfolioTracker.Service
{
    public class PortfolioTickerService
    {
        private readonly PortfolioTrackerContext _context;
        
        public PortfolioTickerService(PortfolioTrackerContext context) 
        {
            _context = context;
        }
        
        public async Task<List<PortfolioTicker>> CreatePortfolioTickerFromDto(List<PortfolioTickerDto> portfolioTickerDtos, int portfolioId)
        {
            var portfolioTickers = new List<PortfolioTicker>();

            foreach (var dto in portfolioTickerDtos)
            {
                var ticker = _context.tickers.FirstOrDefault(t => t.TickerSymbol == dto.TickerSymbol);
                var portfolioTicker = new PortfolioTicker
                {
                    PortfolioId = portfolioId,
                    TickerId = ticker.TickerId,
                    TickerSymbol = dto.TickerSymbol,
                    NumberOfShares = dto.NumberOfShares,
                    AverageSharePirce = dto.AverageSharePrice,
                    LastUpdateDate = DateTime.UtcNow
                };
                portfolioTickers.Add(portfolioTicker);
            }

            return portfolioTickers;
        }
    }
}
