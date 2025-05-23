﻿using System.Globalization;
using System.Text.Json.Nodes;
using PortfolioTracker.PortfolioTracker.Backend.Model;
using PortfolioTracker.PortfolioTracker.Backend.PortfolioDbContext;

namespace PortfolioTracker.PortfolioTracker.Backend.Service
{
    public class DataFetcherService
    {
        private readonly ILogger<DataFetcherService> _logger;
        private readonly IConfiguration _configuration;
        private readonly PortfolioTrackerContext _context;

        public DataFetcherService(ILogger<DataFetcherService> logger, IConfiguration configuration, PortfolioTrackerContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task FetchAllAvaliableTickers()
        {
            _logger.LogInformation("Fetching data");
            var client = new HttpClient() { BaseAddress = new Uri(_configuration["AlphaVentageBaseUrl"]) };
            var response = await client.GetAsync(
                "/query?function=LISTING_STATUS&apikey=" + _configuration["AlphaVentageApiKey"]
            );

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var tickerSymbol = ReadRowFromCsv(data, 0);
                var companyNames = ReadRowFromCsv(data, 1);
               
                foreach (var item in tickerSymbol)
                {
                    var doesTickerExist = _context.tickers.Any(x => x.TickerSymbol.Equals(item));
                    _logger.LogInformation($"Checking item {item}");
                    if (doesTickerExist == false)
                    {
                        var newTicker = new Ticker()
                        {
                            CompanyName = companyNames[tickerSymbol.IndexOf(item)],
                            DividendPerShare = await GetNumericalInformationFromMetrics(item, "dividendPerShareAnnual"),
                            DividendYield = await GetNumericalInformationFromMetrics(item, "dividendYieldIndicatedAnnual"),
                            SharePrice = await GetCurrentSharePrice(item),
                            TickerSymbol = item,
                            LastUpdateDate = DateTime.Now,
                        };
                        _context.tickers.Add(newTicker);
                        await _context.SaveChangesAsync();
                        await Task.Delay(TimeSpan.FromSeconds(3)); // wait so the we dont get 429
                    }
                    else
                    {
                        _logger.LogInformation($"ticker {item} has been already saved");
                    }
                }
            }
            else
            {
                _logger.LogError("Failed to fetch tickers");
            }

            _logger.LogInformation("Fetching finished");
        }
        
        private async Task<decimal> GetNumericalInformationFromMetrics(string companyTicker, string information)
        {
            var client = new HttpClient() { BaseAddress = new Uri(_configuration["FinnhubBaseUrl"]) };
            var response = await client.GetFromJsonAsync<JsonObject>($"/api/v1/stock/metric?symbol={companyTicker}&token="
                + _configuration["FinnhubApiKey"]);

            return response["metric"][$"{information}"] != null ? decimal.Parse(response["metric"][$"{information}"].ToString(), CultureInfo.InvariantCulture) : 0.0m;
        }

        public async Task<decimal> GetCurrentSharePrice(string companyTicker)
        {
            var client = new HttpClient() { BaseAddress = new Uri(_configuration["FinnhubBaseUrl"]) };
            var response = await client.GetFromJsonAsync<JsonObject>($"/api/v1/quote?&symbol={companyTicker}&token="
                + _configuration["FinnhubApiKey"]);

            return decimal.Parse(response["c"].ToString(), CultureInfo.InvariantCulture);
        }

        private List<string> ReadRowFromCsv(string csvData, int row)
        {
            var dataExtracted = new List<string>();
            using (var reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    if (values.Length > 0 && values[6].ToLower() == "active")
                    {
                        dataExtracted.Add(values[row]);
                    }
                }
            }
            return dataExtracted;
        }
    } 
}
