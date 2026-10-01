using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI.DTOs
{
    public class LivreDto
    {
        [BindNever]
        public int Id { get; set; }

        public string? CodeUnique { get; set; }
        public string Isbn10 { get; set; } = string.Empty;
        public string Isbn13 { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        [BindNever]
        public int Quantite { get; set; }
        [BindNever]
        public decimal Prix { get; set; }
        public string? Auteurs { get; set; }
        public string Categorie { get; set; } = string.Empty;
    }
}
