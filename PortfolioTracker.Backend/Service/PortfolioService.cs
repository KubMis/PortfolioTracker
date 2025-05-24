using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.PortfolioTracker.Backend.Model;
using PortfolioTracker.PortfolioTracker.Backend.PortfolioDbContext;

namespace PortfolioTracker.PortfolioTracker.Backend.Service
{
    public class PortfolioService
    {
        private readonly ILogger<PortfolioService> _logger;
        private readonly PortfolioTrackerContext _context;
        private readonly PortfolioTickerService _portfolioTickerService;
        private DataFetcherService _dataFetcherService;

        public PortfolioService(ILogger<PortfolioService> logger, PortfolioTrackerContext context,
            PortfolioTickerService portfolioTickerService, DataFetcherService dataFetcherService)
        {
            _logger = logger;
            _context = context;
            _portfolioTickerService = portfolioTickerService;
            _dataFetcherService = dataFetcherService;
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
                var dividendYield = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol))
                    .DividendYield;
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
                var currentSharePrice = _context.tickers.FirstOrDefault(x => x.TickerSymbol.Equals(ticker.TickerSymbol))
                    .SharePrice;
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
                .CreatePortfolioTickerFromDtosList(portfolioDto.TickerList, newPortfolio.PortfolioId);

            newPortfolio.TickerList.AddRange(tickers);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Portfolio created successfully with ID: {PortfolioId}", newPortfolio.PortfolioId);

            return newPortfolio;
        }

        public async Task<Portfolio> GetPortfolioById(int id)
        {
            return _context.portfolios.Include(x => x.TickerList).FirstOrDefault(x => x.PortfolioId == id);
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

        public async Task<ActionResult> RemoveTickersFromPortfolio(int portfolioId, List<string> tickerSymbols)
        {
            var portfolio = _context.portfolios.FirstOrDefault(x => x.PortfolioId == portfolioId);
            if (portfolio == null)
            {
                return new BadRequestResult();
            }
            else
            {
                foreach (var ticker in tickerSymbols)
                {
                    var tickerToRemove = portfolio.TickerList.FirstOrDefault(x => x.TickerSymbol == ticker.ToUpper());

                    if (tickerToRemove != null)
                    {
                        portfolio.TickerList.Remove(tickerToRemove);
                    }
                }

                await _context.SaveChangesAsync();
                return new OkResult();
            }
        }

        public async Task<ActionResult> AddNewTickersToPortfolio(int portfolioId, List<PortfolioTickerDto> dtos)
        {
            var portfolio = _context.portfolios.FirstOrDefault(x => x.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                return new BadRequestResult();
            }
            else
            {
                List<PortfolioTicker> newTickers = new List<PortfolioTicker>();
                foreach (var dto in dtos)
                {
                    if (!_portfolioTickerService.IsPortfolioTickerDtoValid(dto))
                    {
                        return new BadRequestResult();
                    }

                    var newTicker = await _portfolioTickerService.CreatePortfolioTickerFromDto(dto, portfolioId);
                    newTickers.Add(newTicker);
                }

                portfolio.TickerList.AddRange(newTickers);
                await _context.SaveChangesAsync();
                return new OkResult();
            }
        }

        public async Task<ActionResult> UpdateCurrentSharePriceForPortfolio(int portfolioId)
        {
            var tickersToUpdate = _context.portfolios.Include(portfolio => portfolio.TickerList)
                .FirstOrDefault(x => x.PortfolioId == portfolioId)
                .TickerList;

            if (tickersToUpdate == null)
            {
                return new BadRequestObjectResult("Portfolio has no tickers or does not exist.");
            }

            foreach (var portfolioTicker in tickersToUpdate)
            {
                try
                {
                    _context.tickers
                            .FirstOrDefault(x => x.TickerId == portfolioTicker.TickerId).SharePrice =
                        await _dataFetcherService.GetCurrentSharePrice(portfolioTicker.TickerSymbol);
                    await Task.Delay(3000); // avoid getting 429 from external service 
                }
                catch (Exception ex)
                {
                    return new UnprocessableEntityObjectResult("There was problem updating portfolio share price.");
                }
            }

            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<ActionResult> UpdatePortfolioTickers(int portfolioId,
            List<PortfolioTickerDto> portfolioTickerDtos)
        {
            var portfolio = _context.portfolios.FirstOrDefault(x => x.PortfolioId == portfolioId);

            if (portfolio == null)
            {
                return new BadRequestResult();
            }
            
            foreach (var dto in portfolioTickerDtos)
            {
                if (!_portfolioTickerService.IsPortfolioTickerDtoValid(dto))
                {
                    return new BadRequestResult();
                }

                var existingTicker = portfolio.TickerList.FirstOrDefault(x => x.TickerSymbol == dto.TickerSymbol);
                if (existingTicker != null)
                {
                    existingTicker.LastUpdateDate = DateTime.UtcNow;
                    existingTicker.NumberOfShares = dto.NumberOfShares;
                    existingTicker.AverageSharePirce = dto.AverageSharePrice;
                }
            }

            await _context.SaveChangesAsync();
            return new OkResult();
        }
    }
}