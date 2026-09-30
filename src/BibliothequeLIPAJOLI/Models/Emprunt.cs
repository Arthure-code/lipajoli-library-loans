using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BibliothequeLIPAJOLI.Models
{
    // Un usager peut reemprunter un livre qu'il a deja rendu : l'index
    // accelere la recherche de ses emprunts sans interdire le second.
    [Index(nameof(UsagerID), nameof(LivreID))]
    public class Emprunt
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [Display(Name = "ID de l'usager")]
        public int UsagerID { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [Display(Name = "ID du livre")]
        public int LivreID { get; set; }

        [Required(ErrorMessage = "Le champ est obligatoire")]
        [Display(Name = "Date d'emprunt")]
        [DataType(DataType.Date)]
        public DateTime DateEmprunt { get; set; }

        [Display(Name = "Date de retour attendu")]
        [DataType(DataType.Date)]
        // La date limite se calcule a l'emprunt, a partir de la duree lue
        // dans la configuration.
        public DateTime DateRetourLimite { get; set; }

        [Display(Name = "Date de retour")]
        [DataType(DataType.Date)]
        public DateTime? DateRetour { get; set; } // Sera absente à l'instanciation car elle correspond a la date de retour du livre par l'Usager


        // Propriété navigation .....
        public virtual Livre? Livre { get; set; }
        public virtual Usager? Usager { get; set; }
    }
}
