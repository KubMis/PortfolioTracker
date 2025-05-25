using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.PortfolioTracker.Backend.Service;

namespace PortfolioTracker.PortfolioTracker.Backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TickerController : ControllerBase
    {
        private readonly TickerService _tickerService;

        public TickerController(TickerService tickerService)
        {
            _tickerService = tickerService;
        }
        
        [HttpGet]
        public Task<ActionResult> GetAllTickers()
        {
            var tickers = _tickerService.GetAllTickers();
            return Task.FromResult<ActionResult>(Ok(tickers));
        }
    }
}