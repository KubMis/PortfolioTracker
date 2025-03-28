using PortfolioTracker.PortfolioDbContext;
using PortfolioTracker.Service;

var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<DataFetcherService>();
var configuration = builder.Configuration;
var dataFetcher = new DataFetcherService(logger, configuration);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<PortfolioTickerService>();
builder.Services.AddDbContext<PortfolioTrackerContext>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

dataFetcher.FetchAllAvaliableTickers();

app.Run();

