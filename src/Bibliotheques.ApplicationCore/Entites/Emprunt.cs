using Bibliotheques.ApplicationCore.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Entites
{

    // Un usager peut reemprunter un livre qu'il a deja rendu : l'index
    // accelere la recherche de ses emprunts sans interdire le second.
    [Index(nameof(UsagerID), nameof(LivreID))]
    public class Emprunt : BaseEntity
    {
        public int UsagerID { get; set; }
        public int LivreID { get; set; }
        public DateTime DateEmprunt { get; set; }

        // La date limite se calcule a l'emprunt, a partir de la duree lue
        // dans la configuration.
        public DateTime DateRetourLimite { get; set; }

        public DateTime? DateRetour { get; set; } // Sera absente à l'instanciation car elle correspond a la date de retour du livre par l'Usager

        // Propriété navigation .....
        public virtual Usager? Usager { get; set; }
        public virtual Livre? Livre { get; set; }
    }
}
