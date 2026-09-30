using BibliothequeLIPAJOLI.Controllers;
using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace BibliothequeLIPAJOLI.Tests.Controleurs
{
    public class EmpruntsControllerTest
    {
        private static EmpruntDto Emprunt(int id, int usagerId, int livreId,
            DateTime? retour = null, int joursDeRetard = 0)
        {
            DateTime depart = new DateTime(2026, 1, 5);

            return new EmpruntDto
            {
                Id = id,
                UsagerID = usagerId,
                LivreID = livreId,
                DateEmprunt = depart,
                DateRetourLimite = depart.AddDays(10),
                DateRetour = retour?.AddDays(joursDeRetard)
            };
        }

        [Fact]
        public async Task Index_SansFiltreMontreTousLesEmprunts()
        {
            //Etant donne trois emprunts, dont un rendu a temps et un en retard
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                Emprunt(1, 1, 1),
                Emprunt(2, 1, 2, new DateTime(2026, 1, 10)),
                Emprunt(3, 2, 3, new DateTime(2026, 1, 20))
            });
            var controleur = new EmpruntsController(emprunts.Object,
                new Mock<IUsagersServiceProxy>().Object, new Mock<ILivresServiceProxy>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Index();

            //Alors
            var montres = Assert.IsAssignableFrom<IEnumerable<EmpruntDto>>(
                Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(3, montres.Count());
        }

        [Theory]
        [InlineData("enCours", 1)]
        [InlineData("retournes", 2)]
        [InlineData("enRetard", 3)]
        public async Task Index_ChaqueFiltreNeGardeQueSonEtat(string statut, int identifiantAttendu)
        {
            //Etant donne un emprunt en cours, un rendu a temps, un rendu en retard
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                Emprunt(1, 1, 1),
                Emprunt(2, 1, 2, new DateTime(2026, 1, 10)),
                Emprunt(3, 2, 3, new DateTime(2026, 1, 20))
            });
            var controleur = new EmpruntsController(emprunts.Object,
                new Mock<IUsagersServiceProxy>().Object, new Mock<ILivresServiceProxy>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Index(statut);

            //Alors un seul reste, et la vue sait lequel elle montre
            var montres = Assert.IsAssignableFrom<IEnumerable<EmpruntDto>>(
                Assert.IsType<ViewResult>(resultat).Model);
            Assert.Equal(identifiantAttendu, Assert.Single(montres).Id);
            Assert.Equal(statut, controleur.ViewBag.Statut);
        }

        [Fact]
        public async Task Details_RepondIntrouvableQuandLEmpruntNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(404)).ReturnsAsync((EmpruntDto?)null);
            var controleur = new EmpruntsController(emprunts.Object,
                new Mock<IUsagersServiceProxy>().Object, new Mock<ILivresServiceProxy>().Object);

            //Alors
            Assert.IsType<NotFoundResult>(await controleur.Details(404));
        }

        [Fact]
        public async Task Create_RefuseUnUsagerOuUnLivreInconnu()
        {
            //Etant donne un numero d'usager qui n'existe pas
            var emprunts = new Mock<IEmpruntsService>();
            var usagers = new Mock<IUsagersServiceProxy>();
            var livres = new Mock<ILivresServiceProxy>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<UsagerDto>());
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<LivreDto>());
            var controleur = new EmpruntsController(emprunts.Object, usagers.Object, livres.Object);

            //Lorsque
            IActionResult resultat = await controleur.Create(new EmpruntDto
            {
                Usager = new UsagerDto { No = 999999 },
                Livre = new LivreDto { CodeUnique = "XXX999" }
            });

            //Alors la page revient en le disant, et rien n'est inscrit
            Assert.IsType<ViewResult>(resultat);
            Assert.Contains("introuvable", controleur.ModelState[string.Empty]!.Errors[0].ErrorMessage,
                StringComparison.OrdinalIgnoreCase);
            emprunts.Verify(s => s.InscrireUnNouvelEmprunt(It.IsAny<EmpruntDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_RefuseUnQuatriemeEmpruntEnCours()
        {
            //Etant donne un usager qui tient deja trois livres
            var emprunts = new Mock<IEmpruntsService>();
            var usagers = new Mock<IUsagersServiceProxy>();
            var livres = new Mock<ILivresServiceProxy>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<UsagerDto>
            {
                new UsagerDto { Id = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob" }
            });
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<LivreDto>
            {
                new LivreDto { Id = 9, CodeUnique = "FIC009", Titre = "Le quatrieme" }
            });
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                Emprunt(1, 1, 1), Emprunt(2, 1, 2), Emprunt(3, 1, 3)
            });
            var controleur = new EmpruntsController(emprunts.Object, usagers.Object, livres.Object);

            //Lorsque
            IActionResult resultat = await controleur.Create(new EmpruntDto
            {
                Usager = new UsagerDto { No = 123456 },
                Livre = new LivreDto { CodeUnique = "FIC009" }
            });

            //Alors la bibliotheque s'arrete a trois
            Assert.IsType<ViewResult>(resultat);
            Assert.Contains("3 livres", controleur.ModelState[string.Empty]!.Errors[0].ErrorMessage,
                StringComparison.Ordinal);
            emprunts.Verify(s => s.InscrireUnNouvelEmprunt(It.IsAny<EmpruntDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_RefuseDeuxFoisLeMemeLivreEnMemeTemps()
        {
            //Etant donne un usager qui tient deja ce livre
            var emprunts = new Mock<IEmpruntsService>();
            var usagers = new Mock<IUsagersServiceProxy>();
            var livres = new Mock<ILivresServiceProxy>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<UsagerDto>
            {
                new UsagerDto { Id = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob" }
            });
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<LivreDto>
            {
                new LivreDto { Id = 1, CodeUnique = "FIC001", Titre = "Anna Karenina" }
            });
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                Emprunt(1, 1, 1)
            });
            var controleur = new EmpruntsController(emprunts.Object, usagers.Object, livres.Object);

            //Lorsque
            IActionResult resultat = await controleur.Create(new EmpruntDto
            {
                Usager = new UsagerDto { No = 123456 },
                Livre = new LivreDto { CodeUnique = "FIC001" }
            });

            //Alors
            Assert.IsType<ViewResult>(resultat);
            Assert.Contains("cet exemplaire", controleur.ModelState[string.Empty]!.Errors[0].ErrorMessage,
                StringComparison.Ordinal);
            emprunts.Verify(s => s.InscrireUnNouvelEmprunt(It.IsAny<EmpruntDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_AccepteLeMemeLivreUneFoisQuIlEstRendu()
        {
            //Etant donne un usager qui a rendu ce livre
            var emprunts = new Mock<IEmpruntsService>();
            var usagers = new Mock<IUsagersServiceProxy>();
            var livres = new Mock<ILivresServiceProxy>();
            usagers.Setup(s => s.ObtenirToutUsagers()).ReturnsAsync(new List<UsagerDto>
            {
                new UsagerDto { Id = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob" }
            });
            livres.Setup(s => s.ObtenirToutLivres()).ReturnsAsync(new List<LivreDto>
            {
                new LivreDto { Id = 1, CodeUnique = "FIC001", Titre = "Anna Karenina" }
            });
            emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                Emprunt(1, 1, 1, new DateTime(2026, 1, 10))
            });

            EmpruntDto? inscrit = null;
            emprunts.Setup(s => s.InscrireUnNouvelEmprunt(It.IsAny<EmpruntDto>()))
                .Callback<EmpruntDto>(e => inscrit = e);

            var controleur = new EmpruntsController(emprunts.Object, usagers.Object, livres.Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            //Lorsque
            IActionResult resultat = await controleur.Create(new EmpruntDto
            {
                Usager = new UsagerDto { No = 123456 },
                Livre = new LivreDto { CodeUnique = "FIC001" }
            });

            //Alors l'emprunt part avec les cles retrouvees, pas celles du formulaire
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
            Assert.Equal(1, inscrit?.UsagerID);
            Assert.Equal(1, inscrit?.LivreID);
            Assert.Null(inscrit?.DateRetour);
        }

        [Fact]
        public async Task Retourner_RepondIntrouvableQuandLEmpruntNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(404)).ReturnsAsync((EmpruntDto?)null);
            var controleur = new EmpruntsController(emprunts.Object,
                new Mock<IUsagersServiceProxy>().Object, new Mock<ILivresServiceProxy>().Object);

            //Alors rien n'est retourne
            Assert.IsType<NotFoundResult>(await controleur.Retourner(404));
            emprunts.Verify(s => s.RetournerUnEmprunt(It.IsAny<EmpruntDto>()), Times.Never);
        }

        [Fact]
        public async Task Retourner_RendLEmpruntEtRevientALaListe()
        {
            //Etant donne un emprunt en cours
            EmpruntDto emprunt = Emprunt(1, 1, 1);
            var emprunts = new Mock<IEmpruntsService>();
            emprunts.Setup(s => s.ObtenirUnEmpruntParId(1)).ReturnsAsync(emprunt);
            var controleur = new EmpruntsController(emprunts.Object,
                new Mock<IUsagersServiceProxy>().Object, new Mock<ILivresServiceProxy>().Object);

            //Lorsque
            IActionResult resultat = await controleur.Retourner(1);

            //Alors
            emprunts.Verify(s => s.RetournerUnEmprunt(emprunt), Times.Once);
            Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultat).ActionName);
        }
    }
}
