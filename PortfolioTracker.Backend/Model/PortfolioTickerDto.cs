namespace PortfolioTracker.PortfolioTracker.Backend.Model
{
    public class PortfolioTickerDto
    {
        public int NumberOfShares { get; set; }
        public string TickerSymbol { get; set; }
        public decimal AverageSharePrice { get; set; }
    }
}
