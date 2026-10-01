using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BibliothequeLIPAJOLI.TestsFonctionnels
{
    public class CatalogueTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        // Sans cela, le client suit la redirection et on ne voit plus que
        // la page d'arrivee, jamais la reponse du formulaire.
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

        private static Dictionary<string, string> UnLivre(string jeton, string prix, string isbn10, string isbn13) =>
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = jeton,
                ["Titre"] = "Notre-Dame de Paris",
                ["Isbn10"] = isbn10,
                ["Isbn13"] = isbn13,
                ["Prix"] = prix,
                ["Quantite"] = "3",
                ["Categorie"] = "Roman",
                ["auteursSelectiones"] = "Tolstoy"
            };

        [Theory]
        [InlineData("/")]
        [InlineData("/Livres")]
        [InlineData("/Livres/Create")]
        [InlineData("/Livres/Details/1")]
        [InlineData("/Livres/Edit/1")]
        [InlineData("/Livres/Delete/1")]
        public async Task LesPagesDuCatalogueRepondent(string adresse)
        {
            //Etant donne l'application montee sur sa base
            HttpClient navigateur = Navigateur();

            //Alors
            Assert.Equal(HttpStatusCode.OK, (await navigateur.GetAsync(adresse)).StatusCode);
        }

        [Fact]
        public async Task Index_MontreLesLivresSemesAvecLeurCode()
        {
            //Etant donne le catalogue de depart
            HttpClient navigateur = Navigateur();

            //Lorsque
            string page = await navigateur.GetStringAsync("/Livres");

            //Alors
            Assert.Contains("FIC001", page, StringComparison.Ordinal);
            Assert.Contains("Anna Karenina", page, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("tolstoy")]
        [InlineData("TOLSTOY")]
        public async Task Index_LaRechercheIgnoreLaCasse(string terme)
        {
            //Etant donne une recherche ecrite dans un sens ou dans l'autre
            HttpClient navigateur = Navigateur();

            //Lorsque
            string page = await navigateur.GetStringAsync("/Livres?SearchString=" + terme);

            //Alors les livres de cet auteur sortent
            Assert.Contains("Anna Karenina", page, StringComparison.Ordinal);
            Assert.DoesNotContain("Fleurs du Mal", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Index_UneRechercheSansResultatRendUneListeVide()
        {
            //Etant donne un terme qui ne designe rien
            HttpClient navigateur = Navigateur();

            //Alors
            string page = await navigateur.GetStringAsync("/Livres?SearchString=zzzzz");
            Assert.DoesNotContain("Anna Karenina", page, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Details_RenvoieLaPageDErreurQuandLeLivreNExistePas()
        {
            //Etant donne un identifiant qui ne designe rien
            HttpClient navigateur = Navigateur();

            //Alors la page d'erreur s'affiche
            string page = await navigateur.GetStringAsync("/Livres/Details/404");
            Assert.Contains("Oups", page, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("18,50")]
        [InlineData("18.50")]
        public async Task Create_LePrixSEcritDesDeuxFacons(string prix)
        {
            //Etant donne un formulaire rempli, le prix a la francaise ou a
            //l'anglaise
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivre(jeton, prix, "2222222222", "9782222222222")));

            //Alors le livre entre au catalogue
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            Assert.Contains("Notre-Dame de Paris", await navigateur.GetStringAsync("/Livres"),
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_LaBibliothequeAttribueLeCodeDeLaCategorie()
        {
            //Etant donne un livre de la categorie Roman, qui n'en compte aucun
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsque
            await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivre(jeton, "18,50", "2222222222", "9782222222222")));

            //Alors il prend le prefixe de sa categorie, sans que personne ne
            //l'ait saisi
            Assert.Contains("ROM001", await navigateur.GetStringAsync("/Livres"), StringComparison.Ordinal);
        }

        [Fact]
        public async Task Create_RefuseUnIsbnDejaEnregistre()
        {
            //Etant donne l'ISBN d'un livre deja au catalogue
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Create");

            //Lorsque
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Create",
                new FormUrlEncodedContent(UnLivre(jeton, "18,50", "0393966429", "9780393966428")));

            //Alors la page revient en le disant
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            Assert.Contains("ISBN10 est d", await reponse.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_GardeLaCleEtLeCodeQuoiQuEnDiseLeFormulaire()
        {
            //Etant donne un formulaire qui pretend changer la cle et le code
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Edit/1");
            Dictionary<string, string> champs = UnLivre(jeton, "11,50", "0393966429", "9780393966428");
            champs["Titre"] = "Anna Karenina, edition revue";
            champs["Categorie"] = "Fiction";
            champs["ID"] = "99";
            champs["CodeUnique"] = "PIRATE";

            //Lorsque la requete arrive sur le livre 1
            HttpResponseMessage reponse = await navigateur.PostAsync("/Livres/Edit/1",
                new FormUrlEncodedContent(champs));

            //Alors le titre suit, mais la cle et le code restent
            Assert.Equal(HttpStatusCode.Found, reponse.StatusCode);
            string catalogue = await navigateur.GetStringAsync("/Livres");
            Assert.Contains("edition revue", catalogue, StringComparison.Ordinal);
            Assert.Contains("FIC001", catalogue, StringComparison.Ordinal);
            Assert.DoesNotContain("PIRATE", catalogue, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Edit_UnCodeNeufSuitLeChangementDeCategorie()
        {
            //Etant donne un livre de la categorie Fiction
            HttpClient navigateur = Navigateur();
            string jeton = await Jeton(navigateur, "/Livres/Edit/1");
            Dictionary<string, string> champs = UnLivre(jeton, "11,50", "0393966429", "9780393966428");
            champs["Titre"] = "Anna Karenina";
            champs["Categorie"] = "Roman";

            //Lorsqu'il change de categorie
            await navigateur.PostAsync("/Livres/Edit/1", new FormUrlEncodedContent(champs));

            //Alors son code change avec elle
            Assert.Contains("ROM001", await navigateur.GetStringAsync("/Livres"), StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
