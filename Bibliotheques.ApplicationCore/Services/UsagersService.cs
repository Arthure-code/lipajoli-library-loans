using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Services
{
    public class UsagersService : IUsagersService
    {
        private readonly IAsyncRepository<Usager> _usagersRepository;

        public UsagersService(IAsyncRepository<Usager> usagersRepository)
        {
            _usagersRepository = usagersRepository;
        }   

        public Task<IEnumerable<Usager>> ObtenirToutUsagers()
        {
           return _usagersRepository.ListAsync();
        }
    }
}
