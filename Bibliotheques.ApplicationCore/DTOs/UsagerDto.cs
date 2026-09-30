using Bibliotheques.ApplicationCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.DTOs
{
    public class UsagerDto
    {
        public int Id { get; set; }
        public int No { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public Statut Statut { get; set; }
        public int Defaillance { get; set; }
        public string? Courriel { get; set; }
    }
}
