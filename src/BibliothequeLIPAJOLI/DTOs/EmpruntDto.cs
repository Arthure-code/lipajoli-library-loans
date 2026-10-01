using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI.DTOs
{
    public class EmpruntDto
    {
        [BindNever]
        public int Id { get; set; }
        [BindNever]
        public int UsagerID { get; set; }
        [BindNever]
        public int LivreID { get; set; }
        [BindNever]
        public DateTime DateEmprunt { get; set; }
        [BindNever]
        public DateTime DateRetourLimite { get; set; }
        [BindNever]
        public DateTime? DateRetour { get; set; }
        public UsagerDto? Usager { get; set; }
        public LivreDto? Livre { get; set; }
    }
}
