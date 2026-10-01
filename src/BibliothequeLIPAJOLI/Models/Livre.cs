using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BibliothequeLIPAJOLI.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Models

{
    [Index(nameof(Isbn10))]
    [Index(nameof(Isbn13))]
    public class Livre
    {
        [BindNever]
        public int ID { get; set; }

        [Display(Name = "Code")]
        [BindNever]
        public string? CodeUnique { get; set; }


        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        [Display(Name = "ISBN-10")]
        [RegularExpression(@"(?=[0-9X]{10}$|(?=(?:[0-9]+[-\ ]){3})[-\ 0-9X]{13}$)[0-9]{1,5}[-\]?[0-9]+[-\]?[0-9]+[-\]?[0-9X]$", ErrorMessage = "Vous devez entrer une valeur de ISBN10 valide. Il est possible de séparer les caractères avec un trait d'union.")]
        public string Isbn10 { get; set; } = string.Empty;


        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        [Display(Name = "ISBN-13")]
        [RegularExpression(@"(?=[0-9]{13}$|(?=(?:[0-9]+[-\ ]){4})[-\ 0-9]{17}$)97[89][-\ ]?[0-9]{1,5}[-\ ]?[0-9]+[-\ ]?[0-9]+[-\ ]?[0-9]$", ErrorMessage = "Vous devez entrer une valeur de ISBN13 valide. Il est possible de séparer les caractères avec un trait d'union.")]
        public string Isbn13 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        [MaxLength(80, ErrorMessage = "La taille maximale du champ est de 80 caractères")]
        public string Titre { get; set; } = string.Empty;

        [Display(Name = "Quantité")]
        [Required(ErrorMessage = "Le champ est obligatoire")]
        [Range(0, int.MaxValue, ErrorMessage = "Entrez une quantité valide")]
        public int Quantite { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Le prix ne peut pas être négatif.")]
        [AuPlusDeuxDecimales(ErrorMessage = "Le prix s'écrit avec au plus deux décimales.")]
        public decimal Prix { get; set; }

        [DataType(DataType.Text)]
        public string? Auteurs { get; set; }

        [Display(Name = "Catégorie")]
        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        public string Categorie { get; set; } = string.Empty;

        public virtual ICollection<Emprunt>? Emprunts { get; set; }


    }
}
