using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioTracker.Model
{
    public class PortfolioTicker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PortfolioTickerId { get; set; }
        [ForeignKey(nameof(Ticker))]
        public int TickerId {  get; set; }
        public string TickerSymbol { get; set; }
        public int NumberOfShares { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public decimal AverageSharePirce { get; set; }
        [ForeignKey(nameof(Portfolio))]
        public int PortfolioId { get; set; }
    }
}
