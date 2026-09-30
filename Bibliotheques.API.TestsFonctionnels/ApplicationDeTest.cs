using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bibliotheques.API.TestsFonctionnels
{
    // L'API entiere, son routage, sa serialisation et ses controleurs,
    // montee pour un seul test. Seule la base est remplacee : une SQLite en
    // memoire qui meurt avec le test.
    public sealed class ApplicationDeTest : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connexion = new SqliteConnection("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? ancien = services.SingleOrDefault(
                    s => s.ServiceType == typeof(DbContextOptions<EmpruntsContext>));

                if (ancien != null)
                {
                    services.Remove(ancien);
                }

                _connexion.Open();
                services.AddDbContext<EmpruntsContext>(options =>
                    options.UseLazyLoadingProxies().UseSqlite(_connexion));

                using ServiceProvider fournisseur = services.BuildServiceProvider();
                using IServiceScope portee = fournisseur.CreateScope();

                var context = portee.ServiceProvider.GetRequiredService<EmpruntsContext>();
                context.Database.EnsureCreated();
                Semer(context);
            });
        }

        // De quoi emprunter : deux livres, deux usagers, et un emprunt deja
        // rendu, pour verifier qu'on peut reprendre le meme livre.
        private static void Semer(EmpruntsContext context)
        {
            var anna = new Livre
            {
                CodeUnique = "FIC001",
                Titre = "Anna Karenina",
                Isbn10 = "0393966429",
                Isbn13 = "9780393966428",
                Categorie = "Fiction",
                Quantite = 2,
                Prix = 10.99m,
                Auteurs = "Tolstoy"
            };
            var guerre = new Livre
            {
                CodeUnique = "ROM001",
                Titre = "Guerre et Paix",
                Isbn10 = "8804682590",
                Isbn13 = "9788804682592",
                Categorie = "Roman",
                Quantite = 0,
                Prix = 12.99m,
                Auteurs = "Tolstoy"
            };
            var bob = new Usager
            {
                No = 123456,
                Nom = "Bricoleur",
                Prenom = "Bob",
                Statut = Statut.Enseignant,
                Courriel = "bob@example.com"
            };
            var dora = new Usager
            {
                No = 98765,
                Nom = "Exploratrice",
                Prenom = "Dora",
                Statut = Statut.Étudiant,
                Courriel = "dora@example.com"
            };

            context.Livres.AddRange(anna, guerre);
            context.Usagers.AddRange(bob, dora);
            context.SaveChanges();

            context.Emprunts.Add(new Emprunt
            {
                UsagerID = dora.ID,
                LivreID = anna.ID,
                DateEmprunt = new DateTime(2026, 1, 5),
                DateRetourLimite = new DateTime(2026, 1, 15),
                DateRetour = new DateTime(2026, 1, 12)
            });
            context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connexion.Dispose();
            }
        }
    }
}
