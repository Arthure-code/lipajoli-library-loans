using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeLIPAJOLI.Migrations
{
    /// <inheritdoc />
    public partial class CreationInitiale : Migration
    {
        private static readonly string[] Colonnes = { "UsagerID", "LivreID" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Livre",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeUnique = table.Column<string>(type: "TEXT", nullable: true),
                    Isbn10 = table.Column<string>(type: "TEXT", nullable: false),
                    Isbn13 = table.Column<string>(type: "TEXT", nullable: false),
                    Titre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: false),
                    Prix = table.Column<double>(type: "REAL", nullable: false),
                    Auteurs = table.Column<string>(type: "TEXT", nullable: true),
                    Categorie = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livre", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Usager",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    No = table.Column<int>(type: "INTEGER", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Statut = table.Column<int>(type: "INTEGER", nullable: false),
                    Defaillance = table.Column<int>(type: "INTEGER", nullable: false),
                    Courriel = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usager", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Emprunt",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsagerID = table.Column<int>(type: "INTEGER", nullable: false),
                    LivreID = table.Column<int>(type: "INTEGER", nullable: false),
                    DateEmprunt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateRetourLimite = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateRetour = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emprunt", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Emprunt_Livre_LivreID",
                        column: x => x.LivreID,
                        principalTable: "Livre",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Emprunt_Usager_UsagerID",
                        column: x => x.UsagerID,
                        principalTable: "Usager",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_LivreID",
                table: "Emprunt",
                column: "LivreID");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunt_UsagerID_LivreID",
                table: "Emprunt",
                columns: Colonnes,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Livre_Isbn10",
                table: "Livre",
                column: "Isbn10");

            migrationBuilder.CreateIndex(
                name: "IX_Livre_Isbn13",
                table: "Livre",
                column: "Isbn13");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emprunt");

            migrationBuilder.DropTable(
                name: "Livre");

            migrationBuilder.DropTable(
                name: "Usager");
        }
    }
}
