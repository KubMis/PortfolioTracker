using PortfolioTracker.Model;
using PortfolioTracker.PortfolioDbContext;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PortfolioTracker.Service
{
    public class DataFetcherService
    {
        private readonly ILogger<DataFetcherService> _logger;
        private readonly IConfiguration _configuration;

        public DataFetcherService(ILogger<DataFetcherService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
                var context = new PortfolioTrackerContext(); ;
                var data = await response.Content.ReadAsStringAsync();
                var tickerSymbol = ReadRowFromCsv(data, 0);
                var companyNames = ReadRowFromCsv(data, 1);
               
                foreach (var item in tickerSymbol)
                {
                    var doesTickerExist = context.tickers.Any(x => x.TickerSymbol.Equals(item));
                    if (doesTickerExist == false)
                    {
                        var newTicker = new Ticker()
                        {
                            CompanyName = companyNames[tickerSymbol.IndexOf(item)],
                            DividendPerShare = await getNumericalInformationFromMetrics(item, "dividendPerShareAnnual"),
                            DividendYield = await getNumericalInformationFromMetrics(item, "dividendYieldIndicatedAnnual"),
                            SharePrice = await getCurrentSharePrice(item),
                            TickerSymbol = item,
                            LastUpdateDate = DateTime.Now,
                        };
                        context.tickers.Add(newTicker);
                        await context.SaveChangesAsync();
                        await Task.Delay(TimeSpan.FromSeconds(10)); // wait so the we dont get 429
                    }
                    else
                    {
                        _logger.LogInformation($"ticker {item} has been already saved");
                    }
                }
                context.Dispose();
            }
            else
            {
                _logger.LogError("Failed to fetch tickers");
            }

            _logger.LogInformation("Fetching finished");
        }

        private async Task<decimal> getNumericalInformationFromMetrics(string companyTicker, string information)
        {
            var client = new HttpClient() { BaseAddress = new Uri(_configuration["FinnhubBaseUrl"]) };
            var response = await client.GetFromJsonAsync<JsonObject>($"/api/v1/stock/metric?symbol={companyTicker}&token="
                + _configuration["FinnhubApiKey"]);

            Console.WriteLine(response);
            return response["metric"][$"{information}"] != null ? decimal.Parse(response["metric"][$"{information}"].ToString(), CultureInfo.InvariantCulture) : 0.0m;
        }

        private async Task<decimal> getCurrentSharePrice(string companyTicker)
        {
            var client = new HttpClient() { BaseAddress = new Uri(_configuration["FinnhubBaseUrl"]) };
            var response = await client.GetFromJsonAsync<JsonObject>($"/api/v1/quote?&symbol={companyTicker}&token="
                + _configuration["FinnhubApiKey"]);
            Console.WriteLine(response);
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
