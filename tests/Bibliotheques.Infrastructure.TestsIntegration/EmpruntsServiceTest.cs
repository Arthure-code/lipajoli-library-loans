using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Bibliotheques.ApplicationCore.Services;
using Bibliotheques.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Bibliotheques.Infrastructure.TestsIntegration
{
    public class EmpruntsServiceTest : IDisposable
    {
        private const int JoursDePret = 10;

        private readonly BaseNeuve _base = new BaseNeuve();
        private readonly AsyncRepository<Livre> _livres;
        private readonly AsyncRepository<Usager> _usagers;
        private readonly AsyncRepository<Emprunt> _emprunts;
        private readonly EmpruntsService _service;

        public EmpruntsServiceTest()
        {
            _livres = new AsyncRepository<Livre>(_base.Context);
            _usagers = new AsyncRepository<Usager>(_base.Context);
            _emprunts = new AsyncRepository<Emprunt>(_base.Context);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ParametresEmprunt:DureeMax"] = JoursDePret.ToString()
                })
                .Build();

            _service = new EmpruntsService(_emprunts, configuration, _livres, _usagers);
        }

        private async Task<Livre> UnLivre(string code = "FIC001", int quantite = 2)
        {
            var livre = new Livre
            {
                CodeUnique = code,
                Titre = "Anna Karenina",
                Isbn10 = "0393966429",
                Isbn13 = "9780393966428",
                Categorie = "Fiction",
                Quantite = quantite,
                Prix = 10.99m
            };

            await _livres.AddAsync(livre);
            return livre;
        }

        private async Task<Usager> UnUsager(int numero = 123456)
        {
            var usager = new Usager
            {
                No = numero,
                Nom = "Bricoleur",
                Prenom = "Bob",
                Statut = Statut.Enseignant,
                Courriel = "bob@example.com"
            };

            await _usagers.AddAsync(usager);
            return usager;
        }

        [Fact]
        public async Task InscrireUnNouvelEmprunt_PoseLaDateLimiteLueDansLaConfiguration()
        {
            //Etant donne un livre en rayon et un usager inscrit
            Livre livre = await UnLivre();
            Usager usager = await UnUsager();

            //Lorsque
            Emprunt? emprunt = await _service.InscrireUnNouvelEmprunt(usager, livre);

            //Alors la date limite suit la duree configuree
            Assert.NotNull(emprunt);
            Assert.Equal(DateTime.Today, emprunt!.DateEmprunt);
            Assert.Equal(DateTime.Today.AddDays(JoursDePret), emprunt.DateRetourLimite);
            Assert.Null(emprunt.DateRetour);
        }

        [Fact]
        public async Task InscrireUnNouvelEmprunt_RetireUnExemplaireDuRayon()
        {
            //Etant donne deux exemplaires
            Livre livre = await UnLivre(quantite: 2);
            Usager usager = await UnUsager();

            //Lorsque
            await _service.InscrireUnNouvelEmprunt(usager, livre);
            _base.Oublier();

            //Alors il en reste un
            Assert.Equal(1, (await _livres.GetByIdAsync(livre.ID))?.Quantite);
        }

        [Fact]
        public async Task InscrireUnNouvelEmprunt_RefuseUnLivreEpuise()
        {
            //Etant donne un livre dont tous les exemplaires sont sortis
            Livre livre = await UnLivre(quantite: 0);
            Usager usager = await UnUsager();

            //Alors rien n'est inscrit
            Assert.Null(await _service.InscrireUnNouvelEmprunt(usager, livre));
            Assert.Empty(await _emprunts.ListAsync());
        }

        [Fact]
        public async Task InscrireUnNouvelEmprunt_RefuseDeuxFoisLeMemeLivreEnMemeTemps()
        {
            //Etant donne un livre deja tenu par cet usager
            Livre livre = await UnLivre(quantite: 5);
            Usager usager = await UnUsager();
            await _service.InscrireUnNouvelEmprunt(usager, livre);
            _base.Oublier();

            //Lorsqu'il le redemande
            Emprunt? deuxieme = await _service.InscrireUnNouvelEmprunt(usager, livre);

            //Alors
            Assert.Null(deuxieme);
            Assert.Single(await _emprunts.ListAsync());
        }

        [Fact]
        public async Task InscrireUnNouvelEmprunt_AccepteLeMemeLivreUneFoisQuIlEstRendu()
        {
            //Etant donne un livre emprunte puis rendu
            Livre livre = await UnLivre(quantite: 5);
            Usager usager = await UnUsager();
            await _service.InscrireUnNouvelEmprunt(usager, livre);
            await _service.RetournerUnEmprunt(usager, livre);
            _base.Oublier();

            //Lorsqu'il le reemprunte
            Emprunt? second = await _service.InscrireUnNouvelEmprunt(usager, livre);

            //Alors la base ne s'y oppose plus
            Assert.NotNull(second);
            Assert.Equal(2, (await _emprunts.ListAsync()).Count());
        }

        [Fact]
        public async Task RetournerUnEmprunt_PoseLaDateDuJourEtRemetLExemplaire()
        {
            //Etant donne un emprunt en cours
            Livre livre = await UnLivre(quantite: 2);
            Usager usager = await UnUsager();
            await _service.InscrireUnNouvelEmprunt(usager, livre);
            _base.Oublier();

            //Lorsque
            Emprunt? rendu = await _service.RetournerUnEmprunt(usager, livre);
            _base.Oublier();

            //Alors
            Assert.Equal(DateTime.Today, rendu?.DateRetour);
            Assert.Equal(2, (await _livres.GetByIdAsync(livre.ID))?.Quantite);
        }

        [Fact]
        public async Task RetournerUnEmprunt_NePorteAucuneDefaillanceQuandLeRetourEstATemps()
        {
            //Etant donne un emprunt rendu le jour meme
            Livre livre = await UnLivre();
            Usager usager = await UnUsager();
            await _service.InscrireUnNouvelEmprunt(usager, livre);
            _base.Oublier();

            //Lorsque
            await _service.RetournerUnEmprunt(usager, livre);
            _base.Oublier();

            //Alors le dossier reste propre
            Assert.Equal(0, (await _usagers.GetByIdAsync(usager.ID))?.Defaillance);
        }

        [Fact]
        public async Task RetournerUnEmprunt_PorteUneDefaillanceQuandLaDateLimiteEstDepassee()
        {
            //Etant donne un emprunt dont la date limite est passee
            Livre livre = await UnLivre();
            Usager usager = await UnUsager();
            Emprunt? emprunt = await _service.InscrireUnNouvelEmprunt(usager, livre);
            emprunt!.DateRetourLimite = DateTime.Today.AddDays(-1);
            await _emprunts.EditAsync(emprunt);
            _base.Oublier();

            //Lorsque le livre revient aujourd'hui
            await _service.RetournerUnEmprunt(usager, livre);
            _base.Oublier();

            //Alors le dossier en garde la trace
            Assert.Equal(1, (await _usagers.GetByIdAsync(usager.ID))?.Defaillance);
        }

        [Fact]
        public async Task RetournerUnEmprunt_RendNullQuandRienNEstEnCours()
        {
            //Etant donne un livre que cet usager ne tient pas
            Livre livre = await UnLivre();
            Usager usager = await UnUsager();
            _base.Oublier();

            //Alors
            Assert.Null(await _service.RetournerUnEmprunt(usager, livre));
        }

        [Fact]
        public async Task SupprimerUnEmprunt_RetireLEmpruntEtRemetLExemplaire()
        {
            //Etant donne un emprunt en cours
            Livre livre = await UnLivre(quantite: 2);
            Usager usager = await UnUsager();
            Emprunt? emprunt = await _service.InscrireUnNouvelEmprunt(usager, livre);
            emprunt!.Usager = usager;
            emprunt.Livre = livre;
            _base.Oublier();

            //Lorsque
            await _service.SupprimerUnEmprunt(emprunt);
            _base.Oublier();

            //Alors
            Assert.Empty(await _emprunts.ListAsync());
            Assert.Equal(2, (await _livres.GetByIdAsync(livre.ID))?.Quantite);
        }

        [Fact]
        public async Task SupprimerUnEmprunt_NeFaitRienQuandLeLivreOuLUsagerManque()
        {
            //Etant donne un emprunt sans livre ni usager, celui qui faisait
            //lever une exception
            var emprunt = new Emprunt { ID = 1, UsagerID = 1, LivreID = 1 };

            //Alors la suppression sort proprement
            await _service.SupprimerUnEmprunt(emprunt);
            Assert.Empty(await _emprunts.ListAsync());
        }

        public void Dispose()
        {
            _base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
