using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PortfolioTracker.Model
{
    public class Portfolio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PortfolioId { get; set; }
        public decimal TotalValue { get; set; }
        public decimal ExpectedDividendAmount { get; set; }
        public decimal DividendYield {  get; set; }
        public string PortfolioName { get; set; }
        public decimal Result { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public List<PortfolioTicker> TickerList { get; set; }
    }
}
