using Bibliotheques.API.Controllers;
using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Moq;

namespace Bibliotheques.API.Tests.Controleurs
{
    public class UsagersControllerTest
    {
        [Fact]
        public async Task Get_RendChaqueUsagerInscrit()
        {
            //Etant donne deux usagers au fichier
            var usagers = new Mock<IUsagersService>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<Usager>
            {
                new Usager { ID = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob", Statut = Statut.Enseignant },
                new Usager { ID = 2, No = 98765, Nom = "Exploratrice", Prenom = "Dora", Statut = Statut.Étudiant }
            });
            var controleur = new UsagersController(usagers.Object);

            //Lorsque
            IEnumerable<UsagerDto> rendus = await controleur.Get();

            //Alors
            Assert.Equal(2, rendus.Count());
            usagers.Verify(s => s.ObtenirToutUsagers(), Times.Once);
        }

        [Fact]
        public async Task Get_RecopieChaqueChampDeLUsagerDansSonDto()
        {
            //Etant donne un usager qui porte une defaillance
            var usagers = new Mock<IUsagersService>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<Usager>
            {
                new Usager
                {
                    ID = 3,
                    No = 123456,
                    Nom = "Bricoleur",
                    Prenom = "Bob",
                    Statut = Statut.Enseignant,
                    Defaillance = 2,
                    Courriel = "bob@example.com"
                }
            });
            var controleur = new UsagersController(usagers.Object);

            //Lorsque
            UsagerDto rendu = (await controleur.Get()).Single();

            //Alors
            Assert.Equal(3, rendu.Id);
            Assert.Equal(123456, rendu.No);
            Assert.Equal("Bricoleur", rendu.Nom);
            Assert.Equal("Bob", rendu.Prenom);
            Assert.Equal(Statut.Enseignant, rendu.Statut);
            Assert.Equal(2, rendu.Defaillance);
            Assert.Equal("bob@example.com", rendu.Courriel);
        }

        [Fact]
        public async Task Get_NEmporteAucuneProprieteDeNavigation()
        {
            //Etant donne un usager qui porte ses emprunts
            var usagers = new Mock<IUsagersService>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<Usager>
            {
                new Usager
                {
                    ID = 1,
                    Nom = "Bricoleur",
                    Prenom = "Bob",
                    Emprunts = new List<Emprunt> { new Emprunt { ID = 1, UsagerID = 1 } }
                }
            });
            var controleur = new UsagersController(usagers.Object);

            //Lorsque
            UsagerDto rendu = (await controleur.Get()).Single();

            //Alors
            Assert.Equal("Bricoleur", rendu.Nom);
            Assert.DoesNotContain(typeof(UsagerDto).GetProperties(), p => p.Name == "Emprunts");
        }

        [Fact]
        public async Task Get_RendUneListeVideQuandPersonneNEstInscrit()
        {
            //Etant donne aucun usager
            var usagers = new Mock<IUsagersService>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<Usager>());
            var controleur = new UsagersController(usagers.Object);

            //Alors
            Assert.Empty(await controleur.Get());
        }
    }
}
