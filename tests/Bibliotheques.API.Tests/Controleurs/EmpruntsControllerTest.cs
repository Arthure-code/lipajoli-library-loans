using Bibliotheques.API.Controllers;
using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Bibliotheques.API.Tests.Controleurs
{
    public class EmpruntsControllerTest
    {
        private static EmpruntDto UnFormulaireComplet() => new EmpruntDto
        {
            Id = 1,
            UsagerID = 1,
            LivreID = 1,
            Usager = new UsagerDto { Id = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob", Statut = Statut.Enseignant },
            Livre = new LivreDto { Id = 1, CodeUnique = "FIC001", Titre = "Anna Karenina", Quantite = 2, Prix = 10.99m }
        };

        [Fact]
        public async Task Get_RendChaqueEmpruntAvecSonUsagerEtSonLivre()
        {
            //Etant donne un emprunt dont les deux liens sont charges
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<Emprunt>
            {
                new Emprunt
                {
                    ID = 1,
                    UsagerID = 1,
                    LivreID = 1,
                    DateEmprunt = new DateTime(2026, 1, 5),
                    DateRetourLimite = new DateTime(2026, 1, 15),
                    Usager = new Usager { ID = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob" },
                    Livre = new Livre { ID = 1, Titre = "Anna Karenina", Categorie = "Fiction" }
                }
            });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            EmpruntDto rendu = (await controleur.Get()).Single();

            //Alors la page a de quoi nommer l'usager et le livre
            Assert.Equal("Bricoleur", rendu.Usager?.Nom);
            Assert.Equal("Anna Karenina", rendu.Livre?.Titre);
            Assert.Null(rendu.DateRetour);
        }

        [Fact]
        public async Task Get_SupporteUnEmpruntDontLesLiensNeSontPasCharges()
        {
            //Etant donne un emprunt sans usager ni livre charges
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<Emprunt>
            {
                new Emprunt { ID = 1, UsagerID = 1, LivreID = 1 }
            });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            EmpruntDto rendu = (await controleur.Get()).Single();

            //Alors rien ne se brise, les deux restent absents
            Assert.Null(rendu.Usager);
            Assert.Null(rendu.Livre);
        }

        [Fact]
        public async Task GetParId_RepondIntrouvableQuandLEmpruntNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(404)).ReturnsAsync((Emprunt?)null);
            var controleur = new EmpruntsController(emprunts.Object);

            //Alors
            Assert.IsType<NotFoundResult>((await controleur.Get(404)).Result);
        }

        [Fact]
        public async Task GetParId_RendLEmpruntDemande()
        {
            //Etant donne un emprunt rendu en retard
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(1)).ReturnsAsync(new Emprunt
            {
                ID = 1,
                UsagerID = 1,
                LivreID = 1,
                DateEmprunt = new DateTime(2026, 1, 5),
                DateRetourLimite = new DateTime(2026, 1, 15),
                DateRetour = new DateTime(2026, 1, 20)
            });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult<EmpruntDto> resultat = await controleur.Get(1);

            //Alors les trois dates arrivent telles quelles
            EmpruntDto rendu = Assert.IsType<EmpruntDto>(Assert.IsType<OkObjectResult>(resultat.Result).Value);
            Assert.Equal(new DateTime(2026, 1, 5), rendu.DateEmprunt);
            Assert.Equal(new DateTime(2026, 1, 15), rendu.DateRetourLimite);
            Assert.Equal(new DateTime(2026, 1, 20), rendu.DateRetour);
        }

        [Fact]
        public async Task Post_RefuseUnFormulaireSansUsagerNiLivre()
        {
            //Etant donne un formulaire qui ne designe personne
            var emprunts = new Mock<IEmpruntsService>();
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult<EmpruntDto> resultat = await controleur.Post(new EmpruntDto());

            //Alors rien n'est inscrit
            Assert.IsType<BadRequestObjectResult>(resultat.Result);
            emprunts.Verify(s => s.InscrireUnNouvelEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()), Times.Never);
        }

        [Fact]
        public async Task Post_InscritLEmpruntEtRendSesDates()
        {
            //Etant donne un livre disponible
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.InscrireUnNouvelEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()))
                .ReturnsAsync(new Emprunt
                {
                    ID = 9,
                    UsagerID = 1,
                    LivreID = 1,
                    DateEmprunt = new DateTime(2026, 1, 5),
                    DateRetourLimite = new DateTime(2026, 1, 15)
                });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult<EmpruntDto> resultat = await controleur.Post(UnFormulaireComplet());

            //Alors l'emprunt inscrit revient avec sa date limite, et son
            //adresse de consultation
            var cree = Assert.IsType<CreatedAtActionResult>(resultat.Result);
            EmpruntDto rendu = Assert.IsType<EmpruntDto>(cree.Value);
            Assert.Equal(9, rendu.Id);
            Assert.Equal(9, cree.RouteValues!["id"]);
            Assert.Equal(new DateTime(2026, 1, 15), rendu.DateRetourLimite);
        }

        [Fact]
        public async Task Post_RefuseQuandLeServiceNePeutPasInscrire()
        {
            //Etant donne un livre indisponible, ou deja tenu par l'usager
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.InscrireUnNouvelEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()))
                .ReturnsAsync((Emprunt?)null);
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult<EmpruntDto> resultat = await controleur.Post(UnFormulaireComplet());

            //Alors
            Assert.IsType<BadRequestObjectResult>(resultat.Result);
        }

        [Fact]
        public async Task Post_PasseAuServiceCeQuePorteLeFormulaire()
        {
            //Etant donne un formulaire complet
            var emprunts = new Mock<IEmpruntsService>();
            Usager? usagerRecu = null;
            Livre? livreRecu = null;
            emprunts.Setup(s => s.InscrireUnNouvelEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()))
                .Callback<Usager, Livre>((u, l) => { usagerRecu = u; livreRecu = l; })
                .ReturnsAsync(new Emprunt { ID = 9 });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            await controleur.Post(UnFormulaireComplet());

            //Alors le service recoit des entites, pas des dto
            Assert.Equal(123456, usagerRecu?.No);
            Assert.Equal("FIC001", livreRecu?.CodeUnique);
            Assert.Equal(10.99m, livreRecu?.Prix);
        }

        [Fact]
        public async Task Put_RefuseQuandLAdresseNeDesignePasLeMemeEmprunt()
        {
            //Etant donne un formulaire qui porte un autre identifiant
            var emprunts = new Mock<IEmpruntsService>();
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque la requete arrive sur l'emprunt 99
            ActionResult resultat = await controleur.Put(99, UnFormulaireComplet());

            //Alors rien n'est retourne
            Assert.IsType<BadRequestObjectResult>(resultat);
            emprunts.Verify(s => s.RetournerUnEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()), Times.Never);
        }

        [Fact]
        public async Task Put_RepondIntrouvableQuandAucunEmpruntNEstActif()
        {
            //Etant donne un livre que cet usager ne tient pas
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.RetournerUnEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()))
                .ReturnsAsync((Emprunt?)null);
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult resultat = await controleur.Put(1, UnFormulaireComplet());

            //Alors
            Assert.IsType<NotFoundObjectResult>(resultat);
        }

        [Fact]
        public async Task Put_RetourneLEmpruntActif()
        {
            //Etant donne un emprunt en cours
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.RetournerUnEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()))
                .ReturnsAsync(new Emprunt { ID = 1, DateRetour = new DateTime(2026, 1, 20) });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            ActionResult resultat = await controleur.Put(1, UnFormulaireComplet());

            //Alors
            Assert.IsType<OkResult>(resultat);
            emprunts.Verify(s => s.RetournerUnEmprunt(It.IsAny<Usager>(), It.IsAny<Livre>()), Times.Once);
        }

        [Fact]
        public async Task Delete_RepondIntrouvableQuandLEmpruntNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(404)).ReturnsAsync((Emprunt?)null);
            var controleur = new EmpruntsController(emprunts.Object);

            //Alors
            Assert.IsType<NotFoundResult>(await controleur.Delete(404));
        }

        [Fact]
        public async Task Delete_RefuseDeSupprimerUnEmpruntDejaRetourne()
        {
            //Etant donne un emprunt rendu, qui appartient a l'historique
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(1)).ReturnsAsync(new Emprunt
            {
                ID = 1,
                DateRetour = new DateTime(2026, 1, 20)
            });
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            IActionResult resultat = await controleur.Delete(1);

            //Alors l'historique reste intact
            Assert.IsType<BadRequestObjectResult>(resultat);
            emprunts.Verify(s => s.SupprimerUnEmprunt(It.IsAny<Emprunt>()), Times.Never);
        }

        [Fact]
        public async Task Delete_SupprimeUnEmpruntEnCours()
        {
            //Etant donne un emprunt pas encore rendu
            var emprunt = new Emprunt { ID = 1, UsagerID = 1, LivreID = 1 };
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(1)).ReturnsAsync(emprunt);
            var controleur = new EmpruntsController(emprunts.Object);

            //Lorsque
            IActionResult resultat = await controleur.Delete(1);

            //Alors
            Assert.IsType<NoContentResult>(resultat);
            emprunts.Verify(s => s.SupprimerUnEmprunt(emprunt), Times.Once);
        }
    }
}
