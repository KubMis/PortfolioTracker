﻿namespace PortfolioTracker.Model
{
    public class PortfolioDto
    {
        public string PortfolioName { get; set; }
        public List<PortfolioTickerDto> TickerList { get; set; }
    }
}
