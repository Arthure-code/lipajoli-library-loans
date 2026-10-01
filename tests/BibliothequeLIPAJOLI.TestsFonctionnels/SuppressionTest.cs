using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class SuppressionTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        private HttpClient Navigateur() => _application.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        private static async Task<string> Jeton(HttpClient navigateur, string adresse)
        {
            string page = await navigateur.GetStringAsync(adresse);
            Match jeton = Regex.Match(page,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
                RegexOptions.None, TimeSpan.FromSeconds(5));

            return jeton.Groups[1].Value;
        }

        private static async Task<FormUrlEncodedContent> Confirmation(HttpClient navigateur, string adresse)
        {
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = await Jeton(navigateur, adresse)
            });
        }

        // Les quatre livres semes portent tous un emprunt : pour eprouver une
        // suppression qui aboutit, il en faut un neuf.
        private static async Task<HttpResponseMessage> CataloguerUnLivre(HttpClient navigateur)
        {
            string jeton = await Jeton(navigateur, "/Livres/Create");

            return await navigateur.PostAsync("/Livres/Create", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = jeton,
                    ["Titre"] = "Un livre de passage",
                    ["Isbn10"] = "2222222222",
                    ["Isbn13"] = "9782222222222",
                    ["Prix"] = "18,50",
                    ["Quantite"] = "1",
                    ["Categorie"] = "Roman",
                    ["auteursSelectiones"] = "Tolstoy"
                }));
        }

        [Fact]
        public async Task UnLivreQuePersonneNAEmprunteSeSupprime()
        {
            //Etant donne un livre neuf, que personne n'a encore emprunte
            HttpClient navigateur = Navigateur();
            await CataloguerUnLivre(navigateur);

            //Lorsqu'on le supprime
            FormUrlEncodedContent confirmation = await Confirmation(navigateur, "/Livres/Delete/5");
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Delete/5", confirmation);

            //Alors il quitte le catalogue
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.DoesNotContain("Un livre de passage", await navigateur.GetStringAsync("/Livres"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task UnLivreEmprunteNeSeSupprimePas()
        {
            //Etant donne un livre que les emprunts de depart designent
            HttpClient navigateur = Navigateur();
            FormUrlEncodedContent confirmation = await Confirmation(navigateur, "/Livres/Delete/1");

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Delete/1", confirmation);

            //Alors la page de suppression revient en annoncant l'echec, et le
            //livre reste au catalogue
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Contains("saveChangesError=True", reponse.Headers.Location!.OriginalString,
                StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Anna Karenina", await navigateur.GetStringAsync("/Livres"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task UnLivreQuiNExistePasRenvoieALaListe()
        {
            //Etant donne une confirmation prise sur un livre qui existe
            HttpClient navigateur = Navigateur();
            FormUrlEncodedContent confirmation = await Confirmation(navigateur, "/Livres/Delete/1");

            //Lorsque l'envoi designe un livre absent
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Delete/404", confirmation);

            //Alors
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Equal("/Livres", reponse.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task LaPageDeSuppressionDUnLivreAbsentMontreLErreur()
        {
            //Etant donne un identifiant qui ne designe rien
            HttpClient navigateur = Navigateur();

            //Alors
            Assert.Contains("Oups", await navigateur.GetStringAsync("/Livres/Delete/404"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task UnUsagerSeSupprime()
        {
            //Etant donne un usager au fichier
            HttpClient navigateur = Navigateur();
            FormUrlEncodedContent confirmation = await Confirmation(navigateur, "/Usagers/Delete/2");

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usagers/Delete/2", confirmation);

            //Alors il quitte le fichier
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.DoesNotContain("Exploratrice", await navigateur.GetStringAsync("/Usagers"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task UnUsagerQuiNExistePasRenvoieALaListe()
        {
            //Etant donne une confirmation prise sur un usager qui existe
            HttpClient navigateur = Navigateur();
            FormUrlEncodedContent confirmation = await Confirmation(navigateur, "/Usagers/Delete/2");

            //Lorsque l'envoi designe un dossier absent
            HttpResponseMessage reponse = await navigateur.PostAsync("/Usagers/Delete/404", confirmation);

            //Alors
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Equal("/Usagers", reponse.Headers.Location?.OriginalString);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
