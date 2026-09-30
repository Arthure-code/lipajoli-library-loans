using System.Net;
using System.Net.Http.Json;
using Bibliotheques.ApplicationCore.DTOs;

namespace Bibliotheques.API.TestsFonctionnels
{
    public class EmpruntsTest : IDisposable
    {
        private readonly ApplicationDeTest _application = new ApplicationDeTest();

        private static async Task<EmpruntDto> UnFormulaire(HttpClient client, string codeDuLivre, int numeroDeLUsager)
        {
            List<LivreDto> livres = (await client.GetFromJsonAsync<List<LivreDto>>("/api/Livres"))!;
            List<UsagerDto> usagers = (await client.GetFromJsonAsync<List<UsagerDto>>("/api/Usagers"))!;

            LivreDto livre = livres.Single(l => l.CodeUnique == codeDuLivre);
            UsagerDto usager = usagers.Single(u => u.No == numeroDeLUsager);

            return new EmpruntDto
            {
                UsagerID = usager.Id,
                LivreID = livre.Id,
                Usager = usager,
                Livre = livre
            };
        }

        [Theory]
        [InlineData("/api/Livres")]
        [InlineData("/api/Usagers")]
        [InlineData("/api/Emprunts")]
        public async Task ChaqueListeRepond(string adresse)
        {
            //Etant donne l'API montee sur sa base
            HttpClient client = _application.CreateClient();

            //Alors
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(adresse)).StatusCode);
        }

        [Fact]
        public async Task LesLivresArriventSansLeursEmprunts()
        {
            //Etant donne un livre deja emprunte une fois
            HttpClient client = _application.CreateClient();

            //Lorsque
            string json = await client.GetStringAsync("/api/Livres");

            //Alors la reponse ne repart pas en boucle vers les emprunts
            Assert.Contains("Anna Karenina", json, StringComparison.Ordinal);
            Assert.DoesNotContain("emprunts", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetParId_RepondIntrouvableQuandLEmpruntNExistePas()
        {
            HttpClient client = _application.CreateClient();

            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/Emprunts/404")).StatusCode);
        }

        [Fact]
        public async Task Post_InscritUnEmpruntEtRetireUnExemplaire()
        {
            //Etant donne un livre en rayon
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "FIC001", 123456);

            //Lorsque
            HttpResponseMessage reponse = await client.PostAsJsonAsync("/api/Emprunts", formulaire);

            //Alors l'emprunt est cree, avec sa date limite calculee
            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
            EmpruntDto? cree = await reponse.Content.ReadFromJsonAsync<EmpruntDto>();
            Assert.Equal(DateTime.Today.AddDays(10), cree?.DateRetourLimite);

            List<LivreDto> livres = (await client.GetFromJsonAsync<List<LivreDto>>("/api/Livres"))!;
            Assert.Equal(1, livres.Single(l => l.CodeUnique == "FIC001").Quantite);
        }

        [Fact]
        public async Task Post_AccepteUnLivreDejaEmprunteEtRendu()
        {
            //Etant donne un livre que Dora a deja emprunte et rendu
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "FIC001", 98765);

            //Lorsqu'elle le reprend
            HttpResponseMessage reponse = await client.PostAsJsonAsync("/api/Emprunts", formulaire);

            //Alors la base ne s'y oppose plus
            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        }

        [Fact]
        public async Task Post_RefuseUnLivreEpuise()
        {
            //Etant donne un livre dont il ne reste aucun exemplaire
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "ROM001", 123456);

            //Alors
            Assert.Equal(HttpStatusCode.BadRequest,
                (await client.PostAsJsonAsync("/api/Emprunts", formulaire)).StatusCode);
        }

        [Fact]
        public async Task Post_RefuseUnFormulaireSansUsagerNiLivre()
        {
            HttpClient client = _application.CreateClient();

            Assert.Equal(HttpStatusCode.BadRequest,
                (await client.PostAsJsonAsync("/api/Emprunts", new EmpruntDto())).StatusCode);
        }

        [Fact]
        public async Task Put_RetourneLEmpruntEtRemetLExemplaire()
        {
            //Etant donne un emprunt en cours
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "FIC001", 123456);
            EmpruntDto? cree = await (await client.PostAsJsonAsync("/api/Emprunts", formulaire))
                .Content.ReadFromJsonAsync<EmpruntDto>();

            //Lorsque le livre revient
            formulaire.Id = cree!.Id;
            HttpResponseMessage reponse = await client.PutAsJsonAsync($"/api/Emprunts/{cree.Id}", formulaire);

            //Alors l'exemplaire est de retour au rayon
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            List<LivreDto> livres = (await client.GetFromJsonAsync<List<LivreDto>>("/api/Livres"))!;
            Assert.Equal(2, livres.Single(l => l.CodeUnique == "FIC001").Quantite);
        }

        [Fact]
        public async Task Put_RefuseQuandLAdresseNeDesignePasLeMemeEmprunt()
        {
            //Etant donne un formulaire qui porte un autre identifiant
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "FIC001", 123456);
            formulaire.Id = 1;

            //Alors
            Assert.Equal(HttpStatusCode.BadRequest,
                (await client.PutAsJsonAsync("/api/Emprunts/99", formulaire)).StatusCode);
        }

        [Fact]
        public async Task Delete_RefuseUnEmpruntDejaRetourne()
        {
            //Etant donne l'emprunt rendu qui est dans l'historique
            HttpClient client = _application.CreateClient();
            List<EmpruntDto> emprunts = (await client.GetFromJsonAsync<List<EmpruntDto>>("/api/Emprunts"))!;
            EmpruntDto rendu = emprunts.Single(e => e.DateRetour != null);

            //Alors l'historique est protege
            Assert.Equal(HttpStatusCode.BadRequest,
                (await client.DeleteAsync($"/api/Emprunts/{rendu.Id}")).StatusCode);
        }

        [Fact]
        public async Task Delete_SupprimeUnEmpruntEnCoursEtRemetLExemplaire()
        {
            //Etant donne un emprunt qui vient d'etre inscrit
            HttpClient client = _application.CreateClient();
            EmpruntDto formulaire = await UnFormulaire(client, "FIC001", 123456);
            EmpruntDto? cree = await (await client.PostAsJsonAsync("/api/Emprunts", formulaire))
                .Content.ReadFromJsonAsync<EmpruntDto>();

            //Lorsque
            HttpResponseMessage reponse = await client.DeleteAsync($"/api/Emprunts/{cree!.Id}");

            //Alors
            Assert.Equal(HttpStatusCode.NoContent, reponse.StatusCode);
            List<LivreDto> livres = (await client.GetFromJsonAsync<List<LivreDto>>("/api/Livres"))!;
            Assert.Equal(2, livres.Single(l => l.CodeUnique == "FIC001").Quantite);
        }

        public void Dispose()
        {
            _application.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
