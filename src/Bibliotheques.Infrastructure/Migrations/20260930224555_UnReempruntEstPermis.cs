using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibliotheques.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnReempruntEstPermis : Migration
    {
        private static readonly string[] Colonnes = { "UsagerID", "LivreID" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt",
                columns: Colonnes);
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
                columns: Colonnes,
                unique: true);
        }
    }
}
