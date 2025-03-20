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
            var client = new HttpClient();
            Console.WriteLine(_configuration["AlphaVentageBaseUrl"]);
            client.BaseAddress = new Uri(_configuration["AlphaVentageBaseUrl"]);
            _logger.LogInformation("Fetching data");
            var response = await client.GetAsync(
                "/query?function=LISTING_STATUS&apikey=" + _configuration["AlphaVentageApiKey"]
            );

            if (response.IsSuccessStatusCode)
            {
                var csvData = await response.Content.ReadAsStringAsync();
                var tickers = ParseCsv(csvData);
                _logger.LogInformation("Fetched tickers: {Tickers}", tickers);
            }
            else
            {
                _logger.LogError("Failed to fetch tickers. Status code: {StatusCode}", response.StatusCode);
            }
        }

        private List<string> ParseCsv(string csvData)
        {
            var tickers = new List<string>();
            using (var reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    if (values.Length > 0 && values[6].ToLower()=="active")
                    {
                        tickers.Add(values[0]);
                    }
                }
            }
            return tickers;
        }
    }
}
