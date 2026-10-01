using BibliothequeLIPAJOLI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.DTOs
{
    public class UsagerDto
    {
        [BindNever]
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Le numéro d'usager est requis.")]
        public int No { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        [BindNever]
        public Statut Statut { get; set; }
        [BindNever]
        public int Defaillance { get; set; }
        public string? Courriel { get; set; }
    }
}
