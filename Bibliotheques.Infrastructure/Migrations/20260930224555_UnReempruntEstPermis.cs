using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibliotheques.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnReempruntEstPermis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt",
                columns: new[] { "UsagerID", "LivreID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt",
                columns: new[] { "UsagerID", "LivreID" },
                unique: true);
        }
    }
}
