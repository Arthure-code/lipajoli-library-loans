using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Entites
{
    public enum Statut
    {
        Étudiant,
        Enseignant
    }
    public class Usager : BaseEntity
    {
        public int No { get; set; }

        public string Nom { get; set; }

        public string Prenom { get; set; }

        public Statut Statut { get; set; }

        public int Defaillance { get; set; } = 0; // Initialisée à zéro 

        public string? Courriel { get; set; }

        // Propriété de Navigation:
        public virtual ICollection<Emprunt>? Emprunts { get; set; }
    }
}
