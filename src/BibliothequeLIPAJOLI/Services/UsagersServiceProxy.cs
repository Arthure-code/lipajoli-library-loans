using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Services
{
    public class UsagersServiceProxy : IUsagersServiceProxy
    {

        private readonly HttpClient _httpClient;
        private const string _baseUrl = "api/usagers/";

        public UsagersServiceProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<UsagerDto>> ObtenirToutUsagers()
        {
            return await _httpClient.GetFromJsonAsync<List<UsagerDto>>(_baseUrl) ?? new List<UsagerDto>();
        }
    }
}
