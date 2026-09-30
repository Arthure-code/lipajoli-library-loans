using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Models;
namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface IEmpruntsService
    {
        Task<List<EmpruntDto>> ObtenirToutEmprunts();
        Task InscrireUnNouvelEmprunt(EmpruntDto empruntDto);
        public Task<EmpruntDto?> ObtenirUnEmpruntParId(int id);
        Task RetournerUnEmprunt(EmpruntDto empruntDto);
        public Task SupprimerUnEmprunt(EmpruntDto emprunt);
    }
}
