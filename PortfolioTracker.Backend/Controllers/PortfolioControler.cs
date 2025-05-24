using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.PortfolioTracker.Backend.Model;
using PortfolioTracker.PortfolioTracker.Backend.Service;

namespace PortfolioTracker.PortfolioTracker.Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioService _portfolioService;

        public PortfolioController(PortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePortfolio([FromBody] PortfolioDto portfolioDto)
        {
            try
            {
                if (_portfolioService.IsPortfolioDtoValid(portfolioDto))
                {
                    var portfolio = await _portfolioService.CreatePortfolio(portfolioDto);
                    return Ok(portfolioDto);
                }
                else
                {
                    return BadRequest("Portfolio is not valid");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error creating portfolio: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetPortfolioById([FromQuery] int id)
        {
            var portfolio = await _portfolioService.GetPortfolioById(id);
            
            if (portfolio == null)
            {
                return BadRequest("Portfolio not found");
            }

            if (portfolio.LastUpdateDate - DateTime.Now > TimeSpan.FromMinutes(15))
            {
                await UpdateSharePricesForPortfolio(id);    
            }
            
            return Ok(portfolio);
        }

        [HttpPatch]
        public async Task<ActionResult> UpdatePortfolioTickersById([FromQuery] int portfolioId,
            [FromBody] List<PortfolioTickerDto> portfolioTickerDtos)
        {
            return await _portfolioService.UpdatePortfolioTickers(portfolioId, portfolioTickerDtos);
        }

        [HttpPut]
        public async Task<ActionResult> AddNewTickersToPortfolioById([FromQuery] int portfolioId,
            [FromBody] List<PortfolioTickerDto> portfolioTickerDtos)
        {
            return await _portfolioService.AddNewTickersToPortfolio(portfolioId, portfolioTickerDtos);
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveTickersFromPortfolioById([FromQuery] int portfolioId,
            [FromBody] List<string> tickers)
        {
            return await _portfolioService.RemoveTickersFromPortfolio(portfolioId, tickers);
        }

        [HttpDelete]
        public async Task<ActionResult> DeletePortfolioById([FromQuery] int id)
        {
            return await _portfolioService.RemovePortfolioById(id);
        }

        [HttpGet]
        public async Task<ActionResult> UpdateSharePricesForPortfolio(int id)
        {
            return await _portfolioService.UpdateCurrentSharePriceForPortfolio(id);
        }
    }
}