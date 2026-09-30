using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace BibliothequeLIPAJOLI.Models
{
    public enum Statut
    {

        Étudiant,
        Enseignant       
        
    }
    public class Usager
    {
        public int ID { get; set; }

        [DisplayFormat(DataFormatString = "{0:D10}")]
        public int No { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        [MaxLength(50, ErrorMessage = "La taille maximale du champ est de 50 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [DataType(DataType.Text)]
        [Display(Name = "Prénom")]
        [MaxLength(50, ErrorMessage = "La taille maximale du champ est de 50 caractères")]
        public string Prenom { get; set; } = string.Empty;

        [DisplayFormat(NullDisplayText = "Choisir un statut")]
        [Required(ErrorMessage = "Le champ est obligatoire")]
        public Statut Statut { get; set; }

        [Display(Name = "Nombre de défaillances")]
        public int Defaillance { get; set; } = 0 ; // Initialisée à zéro 

         [DataType(DataType.EmailAddress)]
        public string? Courriel { get; set; }



        // Propriété de Navigation:
        public virtual ICollection<Emprunt>? Emprunts { get; set; }

        
        

    }
}
