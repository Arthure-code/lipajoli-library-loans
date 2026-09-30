using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Models;

namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface ILivresServiceProxy
    {
        public Task<List<LivreDto>> ObtenirToutLivres();
    }
}
