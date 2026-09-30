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

    [Index(nameof(UsagerID), nameof(LivreID), IsUnique = true)] // Créer un index sur ces deux champs et les rend unique
    public class Emprunt : BaseEntity
    {
        public int UsagerID { get; set; }
        public int LivreID { get; set; }
        public DateTime DateEmprunt { get; set; }

        public DateTime DateRetourLimite { get; set; } // AVF TP3 égal à DateEmprunt + NbJourIndiqueDansFichierConfiguration calculé en lien au temps autorisé pour l'emprunt

        public DateTime? DateRetour { get; set; } // Sera absente à l'instanciation car elle correspond a la date de retour du livre par l'Usager

        // Propriété navigation .....
        public virtual Usager Usager { get; set; }
        public virtual Livre Livre { get; set; }
    }
}
