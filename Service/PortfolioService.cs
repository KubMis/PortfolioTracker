using PortfolioTracker.Model;
using PortfolioTracker.PortfolioDbContext;

namespace PortfolioTracker.Service
{
    public class PortfolioService
    {
        private readonly ILogger<PortfolioService> _logger;
        private readonly PortfolioTrackerContext _context;
        private readonly PortfolioTickerService _portfolioTickerService;

        public PortfolioService(ILogger<PortfolioService> logger, PortfolioTrackerContext context, PortfolioTickerService portfolioTickerService)
        {
            _logger = logger;
            _context = context;
            _portfolioTickerService = portfolioTickerService;
        }

        public async Task<Portfolio> CreatePortfolio(PortfolioDto portfolioDto)
        {
            _logger.LogInformation("Creating new portfolio with name: {PortfolioName}", portfolioDto.PortfolioName);

            var newPortfolio = new Portfolio()
            {
                LastUpdateDate = DateTime.UtcNow,
                PortfolioName = portfolioDto.PortfolioName,
                TotalValue = await CalculatePortfolioValue(portfolioDto.TickerList),
                ExpectedDividendAmount = await CalculatePortfolioDividendAmount(portfolioDto.TickerList),
                DividendYield = await CalculatePortfolioDividendYield(portfolioDto.TickerList),
                Result = await CalculatePortfolioResult(portfolioDto.TickerList),
                TickerList = new List<PortfolioTicker>()
            };

            _context.Add(newPortfolio);

            var tickers = await _portfolioTickerService
                .CreatePortfolioTickerFromDto(portfolioDto.TickerList, newPortfolio.PortfolioId);
            
            newPortfolio.TickerList.AddRange(tickers);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio created successfully with ID: {PortfolioId}", newPortfolio.PortfolioId);

            return newPortfolio;

        }

        private async Task<decimal> CalculatePortfolioDividendAmount(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var item in portfolioTickers)
            {
                var dividendAmount = _context.tickers.FirstOrDefault(t => t.TickerSymbol == item.TickerSymbol).DividendPerShare;
                result += dividendAmount;
            }
            return result;
        }

        private async Task<decimal> CalculatePortfolioDividendYield(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                var dividendYield = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol)).DividendYield;
                result += dividendYield;
            }

            return result / portfolioTickers.Count();
        }

        private async Task<decimal> CalculatePortfolioResult(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                var currentSharePrice = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol)).SharePrice;
                result += currentSharePrice - ticker.AverageSharePrice;
            }
            return result;
        }

        private async Task<decimal> CalculatePortfolioValue(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                result += ticker.AverageSharePrice * ticker.NumberOfShares;
            }
            return result;
        }

        public bool IsPortfolioDtoValid(PortfolioDto portfolioDto)
        {
            if (portfolioDto == null
                || string.IsNullOrWhiteSpace(portfolioDto.PortfolioName)
                || portfolioDto.TickerList == null
                || !portfolioDto.TickerList.Any())
            {
                return false;
            }

            return portfolioDto.TickerList.All(ticker =>
                ticker != null
                && !string.IsNullOrWhiteSpace(ticker.TickerSymbol)
                && ticker.NumberOfShares > 0
                && ticker.AverageSharePrice > 0);
        }
    }
}
