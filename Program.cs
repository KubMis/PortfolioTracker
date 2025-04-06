using Microsoft.EntityFrameworkCore;
using PortfolioTracker.PortfolioDbContext;
using PortfolioTracker.Service;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PortfolioTrackerContext>(options =>
    options.UseSqlite($"Data Source={Path.Join(path, "PortfolioTracker.db")}"));
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<PortfolioTickerService>();
builder.Services.AddScoped<DataFetcherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dataFetcher = scope.ServiceProvider.GetRequiredService<DataFetcherService>();
    dataFetcher.FetchAllAvaliableTickers();
}

app.Run();
