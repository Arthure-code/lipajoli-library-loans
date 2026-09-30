using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface IUsagersServiceProxy
    {
        public Task<List<UsagerDto>> ObtenirToutUsagers();
    }
}
