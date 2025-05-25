using Microsoft.EntityFrameworkCore;
using PortfolioTracker.PortfolioTracker.Backend.Model;
using PortfolioTracker.PortfolioTracker.Backend.PortfolioDbContext;

namespace PortfolioTracker.PortfolioTracker.Backend.Service;

public class TickerService
{
    private readonly PortfolioTrackerContext _context;

    public TickerService(PortfolioTrackerContext context)
    {
        _context = context;
    }

    public List<TickerListDto> GetAllTickers()
    {
        return _context.tickers
            .AsNoTracking()
            .Select(x => new TickerListDto 
            { 
                TickerSymbol = x.TickerSymbol, 
                CompanyName = x.CompanyName 
            })
            .ToList();
    }
}