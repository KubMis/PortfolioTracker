using Microsoft.EntityFrameworkCore;
using PortfolioTracker.PortfolioTracker.Backend.PortfolioDbContext;
using PortfolioTracker.PortfolioTracker.Backend.Service;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", 
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:8080") 
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PortfolioTrackerContext>(options =>
    options.UseSqlite($"Data Source={Path.Join(path, "PortfolioTracker.db")}"));
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<TickerService>();
builder.Services.AddScoped<PortfolioTickerService>();
builder.Services.AddScoped<DataFetcherService>();

var app = builder.Build();
app.UseCors("AllowVueApp");

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
