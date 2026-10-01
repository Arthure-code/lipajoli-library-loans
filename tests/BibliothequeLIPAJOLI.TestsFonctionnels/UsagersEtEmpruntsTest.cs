using System.Net;
using System.Text.RegularExpressions;
using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class UsagersEtEmpruntsTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        private HttpClient Navigateur() => _application.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        private static async Task<string> Jeton(HttpClient navigateur, string adresse)
        {
            string page = await navigateur.GetStringAsync(adresse);
            System.Text.RegularExpressions.Match jeton = Regex.Match(page,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
                RegexOptions.None, TimeSpan.FromSeconds(5));

            return jeton.Groups[1].Value;
        }

        private static EmpruntDto UnEmprunt(int identifiant, DateTime? retour = null) => new EmpruntDto
        {
            Id = identifiant,
            UsagerID = 1,
            LivreID = 1,
            DateEmprunt = new DateTime(2026, 1, 5),
            DateRetourLimite = new DateTime(2026, 1, 15),
            DateRetour = retour,
            Usager = new UsagerDto { Id = 1, No = 123456, Nom = "Bricoleur", Prenom = "Bob" },
            Livre = new LivreDto { Id = 1, CodeUnique = "FIC001", Titre = "Anna Karenina" }
        };

        [Theory]
        [InlineData("/Usagers")]
        [InlineData("/Usagers/Create")]
        [InlineData("/Usagers/Details/1")]
        [InlineData("/Usagers/Edit/1")]
        [InlineData("/Usagers/Delete/1")]
        public async Task LesPagesDesUsagersRepondent(string adresse)
        {
            HttpClient navigateur = Navigateur();

            Assert.Equal(HttpStatusCode.OK, (await navigateur.GetAsync(adresse)).StatusCode);
        }

        [Fact]
        public async Task Index_MontreLesUsagersSemes()
        {
            //Etant donne les deux usagers de depart
            HttpClient navigateur = Navigateur();

            //Alors
            string page = await navigateur.GetStringAsync("/Usagers");
            Assert.Contains("Bricoleur", page, StringComparison.Ordinal);
            Assert.Contains("Exploratrice", page, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("dora")]
        [InlineData("DORA")]
        public async Task Index_LaRechercheIgnoreLaCasse(string terme)
        {
            //Etant donne une recherche sur un prenom
            HttpClient navigateur = Navigateur();

            //Alors
            string page = await navigateur.GetStringAsync("/Usagers?SearchString=" + terme);
            Assert.Contains("Exploratrice", page, StringComparison.Ordinal);
            Assert.DoesNotContain("Bricoleur", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_InscritUnUsagerEtLuiDonneLeNumeroSuivant()
        {
            //Etant donne deux usagers deja inscrits
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Usagers/Create");

            //Lorsqu'un troisieme s'inscrit sans numero
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usagers/Create",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = jeton,
                    ["Nom"] = "Beaulieu",
                    ["Prenom"] = "Luc",
                    ["Statut"] = nameof(Statut.Étudiant),
                    ["Courriel"] = "luc.beaulieu@example.com"
                }));

            //Alors la bibliotheque lui attribue le numero, qui suit le plus
            //grand deja donne
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            string liste = await navigateur.GetStringAsync("/Usagers");
            Assert.Contains("Beaulieu", liste, StringComparison.Ordinal);
            Assert.Contains("123457", liste, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_CorrigeLeDossierSansToucherAuNumero()
        {
            //Etant donne un dossier a corriger
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Usagers/Edit/1");

            //Lorsque le formulaire part, en pretendant changer le numero
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usagers/Edit/1",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = jeton,
                    ["Nom"] = "Bricoleur-Roy",
                    ["Prenom"] = "Bob",
                    ["Statut"] = nameof(Statut.Enseignant),
                    ["Courriel"] = "bob.roy@example.com",
                    ["No"] = "999999"
                }));

            //Alors le nom suit, le numero ne bouge pas
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            string liste = await navigateur.GetStringAsync("/Usagers");
            Assert.Contains("Bricoleur-Roy", liste, StringComparison.Ordinal);
            Assert.Contains("123456", liste, StringComparison.Ordinal);
            Assert.DoesNotContain("999999", liste, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Details_RenvoieLaPageDErreurQuandLeDossierNExistePas()
        {
            HttpClient navigateur = Navigateur();

            Assert.Contains("Oups", await navigateur.GetStringAsync("/Usagers/Details/404"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task Emprunts_LaListeMontreCeQueLApiRepond()
        {
            //Etant donne une API qui rend deux emprunts, l'un rendu
            _application.Emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                UnEmprunt(1),
                UnEmprunt(2, new DateTime(2026, 1, 12))
            });
            HttpClient navigateur = Navigateur();

            //Lorsque
            string page = await navigateur.GetStringAsync("/Emprunts");

            //Alors les deux etats s'affichent
            Assert.Contains("En cours", page, StringComparison.Ordinal);
            Assert.Contains("Retourn", page, StringComparison.Ordinal);
            Assert.Contains("Anna Karenina", page, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("enCours", "1")]
        [InlineData("retournes", "2")]
        public async Task Emprunts_ChaqueFiltreNeGardeQueSonEtat(string statut, string identifiantAttendu)
        {
            //Etant donne un emprunt en cours et un rendu a temps
            _application.Emprunts.Setup(s => s.ObtenirToutEmprunts()).ReturnsAsync(new List<EmpruntDto>
            {
                UnEmprunt(1),
                UnEmprunt(2, new DateTime(2026, 1, 12))
            });
            HttpClient navigateur = Navigateur();

            //Lorsque
            string page = await navigateur.GetStringAsync("/Emprunts?statut=" + statut);

            //Alors un seul emprunt reste, celui qu'on peut encore manipuler
            Assert.Contains("Details/" + identifiantAttendu, page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Emprunts_LaPageDeRetourMontreLEmpruntEnCours()
        {
            //Etant donne un emprunt en cours
            _application.Emprunts.Setup(s => s.ObtenirUnEmpruntParId(1)).ReturnsAsync(UnEmprunt(1));
            HttpClient navigateur = Navigateur();

            //Alors la fiche recapitulative s'affiche
            string page = await navigateur.GetStringAsync("/Emprunts/Retourner/1");
            Assert.Contains("Anna Karenina", page, StringComparison.Ordinal);
            Assert.Contains("Bricoleur", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Emprunts_LaPageDeRetourRenvoieALaListeQuandLeLivreEstDejaRendu()
        {
            //Etant donne un emprunt deja rendu
            _application.Emprunts.Setup(s => s.ObtenirUnEmpruntParId(2))
                .ReturnsAsync(UnEmprunt(2, new DateTime(2026, 1, 12)));
            HttpClient navigateur = Navigateur();

            //Alors
            HttpResponseMessage reponse = await navigateur.GetAsync("/Emprunts/Retourner/2");
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Equal("/Emprunts", reponse.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task Emprunts_RefuseUnUsagerOuUnLivreInconnu()
        {
            //Etant donne une API qui ne connait ni l'un ni l'autre
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Emprunts/Create");

            //Lorsque le formulaire part
            HttpResponseMessage reponse = await navigateur.PostAsync("/Emprunts/Create",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = jeton,
                    ["Usager.No"] = "999999",
                    ["Livre.CodeUnique"] = "XXX999"
                }));

            //Alors la page revient en le disant, et rien n'est inscrit
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            Assert.Contains("introuvable", await reponse.Content.ReadAsStringAsync(),
                StringComparison.OrdinalIgnoreCase);
            _application.Emprunts.Verify(s => s.InscrireUnNouvelEmprunt(It.IsAny<EmpruntDto>()), Times.Never);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
