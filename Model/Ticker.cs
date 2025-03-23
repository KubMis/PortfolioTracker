using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Model
{
    public class Ticker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TickerId { get; set; }
        public decimal SharePrice { get; set; }
        public string TickerSymbol { get; set; }
        public string CompanyName {  get; set; }
        public decimal DividendYield { get; set; }
        public decimal DividendPerShare { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
