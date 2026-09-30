using Bibliotheques.ApplicationCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Interfaces
{
    public interface IEmpruntsService
    {
        Task<IEnumerable<Emprunt>> ObtenirToutEmprunts();
        Task<Emprunt> InscrireUnNouvelEmprunt(Usager usager, Livre livre);
        Task<Emprunt> ObtenirUnEmpruntParId(int id);
        Task<Emprunt> RetournerUnEmprunt(Usager usager, Livre codeUnique);
        Task SupprimerUnEmprunt(Emprunt emprunt);
    }
}
