using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Services
{
    public class LivresService : ILivresService
    {
        private readonly IAsyncRepository<Livre> _livresService;

        public LivresService(IAsyncRepository<Livre> livresService)
        {
            _livresService = livresService;
        }

        public Task<IEnumerable<Livre>> ObtenirToutLivres()
        {
            return _livresService.ListAsync();
        }
    }
}
