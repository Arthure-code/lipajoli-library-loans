using BibliothequeLIPAJOLI.Models;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.DTOs
{
    public class UsagerDto
    {
        public int Id { get; set; }

        public int No { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public Statut Statut { get; set; }
        public int Defaillance { get; set; }
        public string Courriel { get; set; }
    }
}
