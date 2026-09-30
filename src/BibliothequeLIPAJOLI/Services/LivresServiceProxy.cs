using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Services
{
    public class LivresServiceProxy : ILivresServiceProxy
    {

        private readonly HttpClient _httpClient;
        private const string _baseUrl = "api/livres/";

        public LivresServiceProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<LivreDto>> ObtenirToutLivres()
        {
            return await _httpClient.GetFromJsonAsync<List<LivreDto>>(_baseUrl) ?? new List<LivreDto>();
        }
    }
}
