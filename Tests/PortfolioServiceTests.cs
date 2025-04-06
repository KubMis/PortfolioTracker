using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PortfolioTracker.Model;
using PortfolioTracker.PortfolioDbContext;
using PortfolioTracker.Service;
using Xunit;

namespace PortfolioTracker.Tests
{
    public class PortfolioServiceTests : IDisposable
    {
        private readonly PortfolioService _service;
        private readonly ILogger<PortfolioService> _logger;
        private readonly PortfolioTrackerContext _context;
        private readonly PortfolioTickerService _portfolioTickerService;

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        public PortfolioServiceTests()
        {
            _logger = Substitute.For<ILogger<PortfolioService>>();
            var options = new DbContextOptionsBuilder<PortfolioTrackerContext>()
            .UseInMemoryDatabase(databaseName: "TestPortfolioDb")
            .Options;

            _context = new PortfolioTrackerContext(options);
            _portfolioTickerService = new PortfolioTickerService(_context);
            _service = new PortfolioService(_logger, _context, _portfolioTickerService);
        }

        [Fact]
        public async Task CreatePortfolio_ShouldCreatePortfolioSuccessfully()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "New Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 100,
                        NumberOfShares = 10,
                        TickerSymbol = "A"
                    },
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 200,
                        NumberOfShares = 5,
                        TickerSymbol = "B"
                    }
                }
            };

            _context.tickers.AddRange(
                new Ticker { TickerSymbol = "A", DividendYield = 0.05m, DividendPerShare = 2, CompanyName = "Test" },
                new Ticker { TickerSymbol = "B", DividendYield = 0.10m, DividendPerShare = 1.5m, CompanyName = "Test2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CreatePortfolio(portfolioDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Portfolio", result.PortfolioName);
            Assert.Equal(2, result.TickerList.Count);
            Assert.Equal(100, result.TickerList[0].AverageSharePirce);
            Assert.Equal(10, result.TickerList[0].NumberOfShares);
            Assert.Equal("A", result.TickerList[0].TickerSymbol);
            Assert.Equal(200, result.TickerList[1].AverageSharePirce);
            Assert.Equal(5, result.TickerList[1].NumberOfShares);
            Assert.Equal("B", result.TickerList[1].TickerSymbol);
            Assert.Equal(0.075m, result.DividendYield);
            Assert.Equal(1.75m, result.ExpectedDividendAmount);
        }
        
        [Fact]
        public async Task GetPortfolioById_ShouldReturnPortfolio_WhenPortfolioExists()
        {
            // Arrange
            var portfolio = new Portfolio
            {
                PortfolioName = "Test Portfolio",
                TickerList = new List<PortfolioTicker>
                {
                    new PortfolioTicker
                    {
                        AverageSharePirce = 100,
                        NumberOfShares = 10,
                        TickerSymbol = "A"
                    }
                }
            };

            _context.portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetPortfolioById(portfolio.PortfolioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.PortfolioId, portfolio.PortfolioId);
            Assert.Equal(result.PortfolioName, portfolio.PortfolioName);
            Assert.Equal(result.TickerList.Count, portfolio.TickerList.Count);
            Assert.Equal(result.TickerList[0].AverageSharePirce, portfolio.TickerList[0].AverageSharePirce);
            Assert.Equal(result.TickerList[0].NumberOfShares, portfolio.TickerList[0].NumberOfShares);
            Assert.Equal(result.TickerList[0].TickerSymbol, portfolio.TickerList[0].TickerSymbol);
        }

        [Fact]
        public async Task CalculatePortfolioValue_ShouldReturnCorrectValue_ForSingleStock()
        {
            //Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "Test Portfolio",
                TickerList = new List<PortfolioTickerDto>
                    {
                        new PortfolioTickerDto
                        {
                            AverageSharePrice = 100,
                            NumberOfShares = 1,
                            TickerSymbol = "A"
                        }
                    }
            };

            //Act
            var result = await _service.CalculatePortfolioValue(portfolioDto.TickerList);

            //Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public async Task CalculatePortfolioValue_ShouldReturnCorrectValue_ForMultipleStocks()
        {
            //Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "Test Portfolio",
                TickerList = new List<PortfolioTickerDto>
                    {
                        new PortfolioTickerDto
                        {
                            AverageSharePrice = 100,
                            NumberOfShares = 1,
                            TickerSymbol = "A"
                        },
                        new PortfolioTickerDto
                        {
                            AverageSharePrice = 218.5m,
                            NumberOfShares = 2,
                            TickerSymbol = "AAPL"
                        }
                    }
            };

            //Act
            var result = await _service.CalculatePortfolioValue(portfolioDto.TickerList);

            //Assert
            Assert.Equal(537, result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnTrue_ForValidPortfolioDto()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "Test Portfolio",
                TickerList = new List<PortfolioTickerDto>
                    {
                        new PortfolioTickerDto
                        {
                            AverageSharePrice = 69,
                            NumberOfShares = 420,
                            TickerSymbol = "XD"
                        }
                    }
            };

            //Act
            var isValid = _service.IsPortfolioDtoValid(portfolioDto);

            //Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenPortfolioDtoIsNull()
        {
            // Arrange
            PortfolioDto portfolioDto = null;

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenPortfolioNameIsNullOrWhitespace()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "AAPL", NumberOfShares = 10, AverageSharePrice = 150 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenTickerListIsNull()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = null
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenTickerListIsEmpty()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>()
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenTickerSymbolIsNullOrWhitespace()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "", NumberOfShares = 10, AverageSharePrice = 150 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenNumberOfSharesIsZeroOrNegative()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "AAPL", NumberOfShares = 0, AverageSharePrice = 150 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenNumberOfSharesIsNegative()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "AAPL", NumberOfShares = -41, AverageSharePrice = 150 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnFalse_WhenAverageSharePriceIsZeroOrNegative()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "AAPL", NumberOfShares = 10, AverageSharePrice = -2 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPortfolioDtoValid_ShouldReturnTrue_WhenPortfolioDtoIsValid()
        {
            // Arrange
            var portfolioDto = new PortfolioDto
            {
                PortfolioName = "My Portfolio",
                TickerList = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto { TickerSymbol = "AAPL", NumberOfShares = 10, AverageSharePrice = 150 },
                    new PortfolioTickerDto { TickerSymbol = "GOOG", NumberOfShares = 5, AverageSharePrice = 2000 }
                }
            };

            // Act
            bool result = _service.IsPortfolioDtoValid(portfolioDto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CalculatePortfolioDividendYield_ShouldReturnCorrectYield()
        {
            // Arrange
            var portfolioDto = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 100,
                        NumberOfShares = 10,
                        TickerSymbol = "A"
                    },
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 200,
                        NumberOfShares = 5,
                        TickerSymbol = "B"
                    }
                };

            _context.tickers.AddRange(
                new Ticker { TickerSymbol = "A", DividendYield = 0.05m, CompanyName = "Test" },
                new Ticker { TickerSymbol = "B", DividendYield = 0.10m, CompanyName = "Test2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CalculatePortfolioDividendYield(portfolioDto);

            // Assert
            Assert.Equal(0.075m, result);
        }

        [Fact]
        public async Task CalculatePortfolioDividendAmount_ShouldReturnCorrectAmount()
        {
            // Arrange
            var portfolioDto = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 100,
                        NumberOfShares = 4,
                        TickerSymbol = "A"
                    },
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 200,
                        NumberOfShares = 3,
                        TickerSymbol = "B"
                    }
                };

            _context.tickers.AddRange(
                new Ticker { TickerSymbol = "A", DividendPerShare = 15.21m, CompanyName = "Test" },
                new Ticker { TickerSymbol = "B", DividendPerShare = 6.69m, CompanyName = "Test2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CalculatePortfolioDividendAmount(portfolioDto);

            // Assert
            Assert.Equal(80.91m, result);
        }

        [Fact]
        public async Task CalculatePortfolioResult_ShouldReturnCorrectResult()
        {
            // Arrange
            var portfolioDto = new List<PortfolioTickerDto>
                {
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 100,
                        NumberOfShares = 10,
                        TickerSymbol = "A"
                    },
                    new PortfolioTickerDto
                    {
                        AverageSharePrice = 200,
                        NumberOfShares = 5,
                        TickerSymbol = "B"
                    }
                };

            _context.tickers.AddRange(
                new Ticker { TickerSymbol = "A", SharePrice = 110, CompanyName = "Test" },
                new Ticker { TickerSymbol = "B", SharePrice = 210, CompanyName = "Test2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CalculatePortfolioResult(portfolioDto);

            // Assert
            Assert.Equal(150, result);
        }
    }
}