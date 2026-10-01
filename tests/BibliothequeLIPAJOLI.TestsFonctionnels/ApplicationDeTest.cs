using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    // L'application entiere, son routage, sa liaison de modele, sa
    // validation et ses vues, montee pour un seul test. La base devient une
    // SQLite en memoire, et l'API devient un double : c'est l'application
    // qu'on eprouve ici, pas le reseau.
    // Le type passe a la fabrique ne sert qu'a designer l'assemblage.
    public sealed class ApplicationDeTest : WebApplicationFactory<LivresController>
    {
        private readonly SqliteConnection _connexion = new SqliteConnection("DataSource=:memory:");

        public Mock<IEmpruntsService> Emprunts { get; } = new Mock<IEmpruntsService>();

        public Mock<IUsagersServiceProxy> UsagersDeLApi { get; } = new Mock<IUsagersServiceProxy>();

        public Mock<ILivresServiceProxy> LivresDeLApi { get; } = new Mock<ILivresServiceProxy>();

        public ApplicationDeTest()
        {
            Emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>());
            UsagersDeLApi.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<UsagerDto>());
            LivresDeLApi.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<LivreDto>());
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ConfigureServices(services =>
            {
                Remplacer<DbContextOptions<BibliothequeContext>>(services);
                Remplacer<IEmpruntsService>(services);
                Remplacer<IUsagersServiceProxy>(services);
                Remplacer<ILivresServiceProxy>(services);

                // La connexion reste ouverte : c'est elle qui tient la base.
                // Le demarrage de l'application cree le schema et seme ses
                // donnees dedans, comme il le ferait en vrai.
                _connexion.Open();
                services.AddDbContext<BibliothequeContext>(options => options.UseSqlite(_connexion));

                services.AddScoped(_ => Emprunts.Object);
                services.AddScoped(_ => UsagersDeLApi.Object);
                services.AddScoped(_ => LivresDeLApi.Object);
            });
        }

        private static void Remplacer<TService>(IServiceCollection services)
        {
            foreach (ServiceDescriptor ancien in services.Where(s => s.ServiceType == typeof(TService)).ToList())
            {
                services.Remove(ancien);
            }
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
