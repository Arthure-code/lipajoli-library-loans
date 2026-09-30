using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeLIPAJOLI.Services
{
    public class EmpruntsServiceProxy : IEmpruntsService
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "api/emprunts/";

        public EmpruntsServiceProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EmpruntDto>> ObtenirToutEmprunts()
        {
            return await _httpClient.GetFromJsonAsync<List<EmpruntDto>>(_baseUrl) ?? new List<EmpruntDto>();
        }

        public async Task<EmpruntDto?> ObtenirUnEmpruntParId(int id)
        {
            return await _httpClient.GetFromJsonAsync<EmpruntDto>(_baseUrl + id);
        }
        public async Task InscrireUnNouvelEmprunt(EmpruntDto empruntDto)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(empruntDto), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(_baseUrl, content);
        }

        public async Task RetournerUnEmprunt(EmpruntDto empruntDto)
        {
            var content = new StringContent(
                 JsonConvert.SerializeObject(empruntDto),
                 Encoding.UTF8,
                 "application/json"
             );

            await _httpClient.PutAsync(_baseUrl + empruntDto.Id, content);
        }

        public async Task SupprimerUnEmprunt(EmpruntDto empruntDto)
        {
            await _httpClient.DeleteAsync(_baseUrl + empruntDto.Id);
        }
    }
}