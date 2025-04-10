using Microsoft.AspNetCore.Mvc;
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

        public async Task<List<PortfolioTicker>> CreatePortfolioTickerFromDtosList(List<PortfolioTickerDto> portfolioTickerDtos, int portfolioId)
        {
            var portfolioTickers = new List<PortfolioTicker>();

            foreach (var dto in portfolioTickerDtos)
            {
                var portfolioTicker = await CreatePortfolioTickerFromDto(dto, portfolioId);
                portfolioTickers.Add(portfolioTicker);
            }

            return portfolioTickers;
        }

        public async Task<PortfolioTicker> CreatePortfolioTickerFromDto(PortfolioTickerDto portfolioTickerDto, int portfolioId)
        {
            var ticker = _context.tickers.FirstOrDefault(t => t.TickerSymbol == portfolioTickerDto.TickerSymbol);
            var portfolioTicker = new PortfolioTicker
            {
                PortfolioId = portfolioId,
                TickerId = ticker.TickerId,
                TickerSymbol = portfolioTickerDto.TickerSymbol,
                NumberOfShares = portfolioTickerDto.NumberOfShares,
                AverageSharePirce = portfolioTickerDto.AverageSharePrice,
                LastUpdateDate = DateTime.UtcNow
            };

            return portfolioTicker;
        }

        public bool IsPortfolioTickerDtoValid(PortfolioTickerDto portfolioTickerDto)
        {
            if (portfolioTickerDto == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(portfolioTickerDto.TickerSymbol))
            {
                return false;
            }

            if (portfolioTickerDto.NumberOfShares <= 0)
            {
                return false;
            }

            if (portfolioTickerDto.AverageSharePrice <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
