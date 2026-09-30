using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Entites
{
    [Index(nameof(Isbn10))]
    [Index(nameof(Isbn13))]
    public class Livre : BaseEntity
    {
        public string? CodeUnique { get; set; }
        public string Isbn10 { get; set; } = string.Empty;
        public string Isbn13 { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal Prix { get; set; }
        public string? Auteurs { get; set; }
        public string Categorie { get; set; } = string.Empty;

        public virtual ICollection<Emprunt>? Emprunts { get; set; }


    }
}
