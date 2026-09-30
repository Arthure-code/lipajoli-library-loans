using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.DTOs
{
    public class EmpruntDto
    {
        public int Id { get; set; }
        public int UsagerID { get; set; }
        public int  LivreID { get; set; }
        public DateTime DateEmprunt { get; set; }
        public DateTime DateRetourLimite { get; set; }
        public DateTime? DateRetour { get; set; }
        public UsagerDto? Usager { get; set; }
        public LivreDto? Livre { get; set; }
    }
}
