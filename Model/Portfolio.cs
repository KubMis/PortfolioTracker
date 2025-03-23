using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Model
{
    public class Portfolio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PortfolioId { get; set; }
        public decimal totalValue { get; set; }
        public decimal expectedDividendAmount { get; set; }
        public string portfolioName { get; set; }
        public decimal result { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public List<PortfolioTicker> tickerList { get; set; }
    }
}
