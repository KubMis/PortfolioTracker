using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<decimal> CalculatePortfolioDividendAmount(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var item in portfolioTickers)
            {
                var ticker = _context.tickers.FirstOrDefault(t => t.TickerSymbol == item.TickerSymbol);
                result += ticker.DividendPerShare * item.NumberOfShares;
            }

            return result;
        }

        public async Task<decimal> CalculatePortfolioDividendYield(List<PortfolioTickerDto> portfolioTickers)
        {
            _logger.LogInformation("Calculating portfolio dividend yield.");
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                var dividendYield = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol)).DividendYield;
                result += dividendYield;
            }
            var finalResult = result / portfolioTickers.Count();
            _logger.LogInformation("Calculated portfolio dividend yield: {Result}", finalResult);
            return finalResult;
        }

        public async Task<decimal> CalculatePortfolioResult(List<PortfolioTickerDto> portfolioTickers)
        {
            _logger.LogInformation("Calculating portfolio result.");
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                var currentSharePrice = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol)).SharePrice;
                result += (currentSharePrice - ticker.AverageSharePrice) * ticker.NumberOfShares;
            }
            _logger.LogInformation("Calculated portfolio result: {Result}", result);
            return result;
        }

        public async Task<decimal> CalculatePortfolioValue(List<PortfolioTickerDto> portfolioTickers)
        {
            decimal result = 0.0m;
            foreach (var ticker in portfolioTickers)
            {
                result += ticker.AverageSharePrice * ticker.NumberOfShares;
            }
            return result;
        }

         public async Task<Portfolio> CreatePortfolio(PortfolioDto portfolioDto)
        {
            _logger.LogInformation("Creating new portfolio with name: {PortfolioName}", portfolioDto.PortfolioName);
            var totalValue = await CalculatePortfolioValue(portfolioDto.TickerList);
            var expectedDividend = await CalculatePortfolioDividendAmount(portfolioDto.TickerList);
            var dividendYield = await CalculatePortfolioDividendYield(portfolioDto.TickerList);
            var result = await CalculatePortfolioResult(portfolioDto.TickerList);

            var newPortfolio = new Portfolio()
            {
                LastUpdateDate = DateTime.UtcNow,
                PortfolioName = portfolioDto.PortfolioName,
                TotalValue = totalValue,
                ExpectedDividendAmount = expectedDividend,
                DividendYield = dividendYield,
                Result = result,
                TickerList = new List<PortfolioTicker>()
            };

            _logger.LogInformation("Object created");
            _context.Add(newPortfolio);

            var tickers = await _portfolioTickerService
                .CreatePortfolioTickerFromDto(portfolioDto.TickerList, newPortfolio.PortfolioId);

            newPortfolio.TickerList.AddRange(tickers);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio created successfully with ID: {PortfolioId}", newPortfolio.PortfolioId);

            return newPortfolio;

        }

        public async Task<Portfolio> GetPortfolioById(int id)
        {
            return  _context.portfolios.FirstOrDefault(x => x.PortfolioId == id);
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

        public async Task<ActionResult> RemovePortfolioById(int id)
        {
            var portfolio = await _context.portfolios.FirstOrDefaultAsync(x => x.PortfolioId == id);
            if (portfolio != null)
            {
                _context.portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Portfolio with ID: {PortfolioId} has been removed.", id);
                return new OkResult();
            }
            else
            {
                _logger.LogWarning("Portfolio with ID: {PortfolioId} not found.", id);
                return new NotFoundResult();
            }
        }
    }
}
