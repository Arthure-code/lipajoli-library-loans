using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Bibliotheques.Infrastructure.Data;

namespace Bibliotheques.Infrastructure.TestsIntegration
{
    public class AsyncRepositoryTest : IDisposable
    {
        private readonly BaseNeuve _base = new BaseNeuve();
        private readonly AsyncRepository<Livre> _livres;

        public AsyncRepositoryTest()
        {
            _livres = new AsyncRepository<Livre>(_base.Context);
        }

        private static Livre UnLivre(string code, string titre, int quantite = 2) => new Livre
        {
            CodeUnique = code,
            Titre = titre,
            Isbn10 = "0393966429",
            Isbn13 = "9780393966428",
            Categorie = "Fiction",
            Quantite = quantite,
            Prix = 10.99m
        };

        [Fact]
        public async Task AddAsync_EcritLEntiteEtLuiDonneUneCle()
        {
            //Etant donne un livre qui n'est pas encore en base
            Livre livre = UnLivre("FIC001", "Anna Karenina");

            //Lorsque
            await _livres.AddAsync(livre);
            _base.Oublier();

            //Alors la base lui a attribue sa cle, et le relit tel quel
            Assert.NotEqual(0, livre.ID);
            Livre? relu = await _livres.GetByIdAsync(livre.ID);
            Assert.Equal("Anna Karenina", relu?.Titre);
            Assert.Equal(10.99m, relu?.Prix);
        }

        [Fact]
        public async Task GetByIdAsync_RendNullQuandLaCleNeDesigneRien()
        {
            Assert.Null(await _livres.GetByIdAsync(404));
        }

        [Fact]
        public async Task ListAsync_RendTouteLaTable()
        {
            //Etant donne deux livres
            await _livres.AddAsync(UnLivre("FIC001", "Anna Karenina"));
            await _livres.AddAsync(UnLivre("ROM001", "Guerre et Paix"));
            _base.Oublier();

            //Alors
            Assert.Equal(2, (await _livres.ListAsync()).Count());
        }

        [Fact]
        public async Task ListAsync_AvecUnePredicatNeRendQueCeQuiCorrespond()
        {
            //Etant donne un livre epuise et un livre en rayon
            await _livres.AddAsync(UnLivre("FIC001", "Anna Karenina", quantite: 0));
            await _livres.AddAsync(UnLivre("ROM001", "Guerre et Paix", quantite: 4));
            _base.Oublier();

            //Lorsque la question porte sur ce qui reste disponible
            IEnumerable<Livre> disponibles = await _livres.ListAsync(l => l.Quantite > 0);

            //Alors
            Assert.Equal("Guerre et Paix", Assert.Single(disponibles).Titre);
        }

        [Fact]
        public async Task EditAsync_EcritLaCorrection()
        {
            //Etant donne un livre en base
            Livre livre = UnLivre("FIC001", "Anna Karenina");
            await _livres.AddAsync(livre);

            //Lorsque sa quantite change
            livre.Quantite = 7;
            await _livres.EditAsync(livre);
            _base.Oublier();

            //Alors
            Assert.Equal(7, (await _livres.GetByIdAsync(livre.ID))?.Quantite);
        }

        [Fact]
        public async Task DeleteAsync_RetireLEntite()
        {
            //Etant donne un livre en base
            Livre livre = UnLivre("FIC001", "Anna Karenina");
            await _livres.AddAsync(livre);

            //Lorsque
            await _livres.DeleteAsync(livre);
            _base.Oublier();

            //Alors
            Assert.Null(await _livres.GetByIdAsync(livre.ID));
        }

        [Fact]
        public async Task LeMemeDepotServaitPourChaqueEntite()
        {
            //Etant donne le depot generique monte sur les usagers
            AsyncRepository<Usager> usagers = new AsyncRepository<Usager>(_base.Context);

            //Lorsque
            await usagers.AddAsync(new Usager
            {
                No = 123456,
                Nom = "Bricoleur",
                Prenom = "Bob",
                Statut = Statut.Enseignant,
                Courriel = "bob@example.com"
            });
            _base.Oublier();

            //Alors il ecrit dans la table des usagers sans code particulier
            Assert.Equal("Bricoleur", Assert.Single(await usagers.ListAsync()).Nom);
        }

        public void Dispose()
        {
            _base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
