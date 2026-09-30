using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BibliothequeLIPAJOLI.Models
{
    [Index(nameof(UsagerID), nameof(LivreID), IsUnique = true)] // Créer un index sur ces deux champs et les rend unique
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

        [Required(ErrorMessage = "Le champ est obligatoire")]  // AVF TP3 égal au DateTime.Today()
        [Display(Name = "Date d'emprunt")]
        [DataType(DataType.Date)]
        public DateTime DateEmprunt { get; set; }

        [Display(Name = "Date de retour attendu")]
        [DataType(DataType.Date)]
        public DateTime DateRetourLimite { get; set; } // AVF TP3 égal à DateEmprunt + NbJourIndiqueDansFichierConfiguration calculé en lien au temps autorisé pour l'emprunt

        [Display(Name = "Date de retour")]
        [DataType(DataType.Date)]
        public DateTime? DateRetour { get; set; } // Sera absente à l'instanciation car elle correspond a la date de retour du livre par l'Usager


        // Propriété navigation .....
        public virtual Livre? Livre { get; set; }
        public virtual Usager? Usager { get; set; }
    }
}
