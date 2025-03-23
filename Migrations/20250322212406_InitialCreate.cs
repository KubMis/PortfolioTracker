using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    totalValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    expectedDividendAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    portfolioName = table.Column<string>(type: "TEXT", nullable: false),
                    result = table.Column<decimal>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.PortfolioId);
                });

            migrationBuilder.CreateTable(
                name: "tickers",
                columns: table => new
                {
                    TickerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SharePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    TickerSymbol = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: false),
                    DividendYield = table.Column<decimal>(type: "TEXT", nullable: false),
                    DividendPerShare = table.Column<decimal>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickers", x => x.TickerId);
                });

            migrationBuilder.CreateTable(
                name: "portfolioTickers",
                columns: table => new
                {
                    PortfolioTickerId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TickerId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AverageSharePirce = table.Column<decimal>(type: "TEXT", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolioTickers", x => x.PortfolioTickerId);
                    table.ForeignKey(
                        name: "FK_portfolioTickers_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_portfolioTickers_PortfolioId",
                table: "portfolioTickers",
                column: "PortfolioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolioTickers");

            migrationBuilder.DropTable(
                name: "tickers");

            migrationBuilder.DropTable(
                name: "portfolios");
        }
    }
}
