using Bibliotheques.ApplicationCore.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Interfaces
{
    public interface IUsagersService
    {
        public Task<IEnumerable<Usager>> ObtenirToutUsagers();
    }
}
