using Bibliotheques.API.Controllers;
using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Moq;

namespace Bibliotheques.API.Tests.Controleurs
{
    public class LivresControllerTest
    {
        [Fact]
        public async Task Get_RendChaqueLivreDuCatalogue()
        {
            //Etant donne deux livres au catalogue
            var livres = new Mock<ILivresService>();
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<Livre>
            {
                new Livre { ID = 1, CodeUnique = "FIC001", Titre = "Anna Karenina", Categorie = "Fiction", Quantite = 2, Prix = 10.99m },
                new Livre { ID = 2, CodeUnique = "ROM001", Titre = "Guerre et Paix", Categorie = "Roman", Quantite = 4, Prix = 12.99m }
            });
            var controleur = new LivresController(livres.Object);

            //Lorsque
            IEnumerable<LivreDto> rendus = await controleur.Get();

            //Alors
            Assert.Equal(2, rendus.Count());
            livres.Verify(s => s.ObtenirToutLivres(), Times.Once);
        }

        [Fact]
        public async Task Get_RecopieChaqueChampDuLivreDansSonDto()
        {
            //Etant donne un livre complet
            var livres = new Mock<ILivresService>();
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<Livre>
            {
                new Livre
                {
                    ID = 7,
                    CodeUnique = "FIC004",
                    Isbn10 = "0393966429",
                    Isbn13 = "9780393966428",
                    Titre = "Anna Karenina",
                    Quantite = 2,
                    Prix = 10.99m,
                    Auteurs = "Tolstoy",
                    Categorie = "Fiction"
                }
            });
            var controleur = new LivresController(livres.Object);

            //Lorsque
            LivreDto rendu = (await controleur.Get()).Single();

            //Alors le dto porte tout ce que la page a besoin de montrer
            Assert.Equal(7, rendu.Id);
            Assert.Equal("FIC004", rendu.CodeUnique);
            Assert.Equal("0393966429", rendu.Isbn10);
            Assert.Equal("9780393966428", rendu.Isbn13);
            Assert.Equal("Anna Karenina", rendu.Titre);
            Assert.Equal(2, rendu.Quantite);
            Assert.Equal(10.99m, rendu.Prix);
            Assert.Equal("Tolstoy", rendu.Auteurs);
            Assert.Equal("Fiction", rendu.Categorie);
        }

        [Fact]
        public async Task Get_NEmporteAucuneProprieteDeNavigation()
        {
            //Etant donne un livre qui porte ses emprunts
            var livres = new Mock<ILivresService>();
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<Livre>
            {
                new Livre
                {
                    ID = 1,
                    Titre = "Anna Karenina",
                    Categorie = "Fiction",
                    Emprunts = new List<Emprunt> { new Emprunt { ID = 1, UsagerID = 1, LivreID = 1 } }
                }
            });
            var controleur = new LivresController(livres.Object);

            //Lorsque
            LivreDto rendu = (await controleur.Get()).Single();

            //Alors le dto ne contient que les donnees, ce qui evite de
            //repartir en boucle vers les emprunts et leurs livres
            Assert.Equal("Anna Karenina", rendu.Titre);
            Assert.DoesNotContain(typeof(LivreDto).GetProperties(), p => p.Name == "Emprunts");
        }

        [Fact]
        public async Task Get_RendUneListeVideQuandLeCatalogueEstVide()
        {
            //Etant donne aucun livre
            var livres = new Mock<ILivresService>();
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<Livre>());
            var controleur = new LivresController(livres.Object);

            //Alors
            Assert.Empty(await controleur.Get());
        }
    }
}
