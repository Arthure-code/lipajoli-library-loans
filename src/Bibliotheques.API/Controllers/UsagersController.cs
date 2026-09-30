using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Bibliotheques.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsagersController : ControllerBase
    {
        private readonly IUsagersService _usagerService;

        public UsagersController(IUsagersService usagerService)
        {
            _usagerService = usagerService;
        }


        /// <summary>
        /// Retourne la liste de tous les usagers enregistrés.
        /// </summary>
        /// <remarks>
        /// Récupère tous les usagers présents dans le système, avec leurs informations complètes :
        /// numéro, nom, prénom, statut, nombre de défaillances et courriel.
        /// </remarks>
        /// <response code="200">Liste des usagers retournée avec succès.</response>
        /// <response code="500">Erreur interne du serveur lors de la récupération des usagers.</response>
        
        // GET: api/<UsagersController>
        [HttpGet]
        public async Task<IEnumerable<UsagerDto>> Get() 
        {
            var usagers = await _usagerService.ObtenirToutUsagers();
            return usagers.Select(u => new UsagerDto
            {
                Id = u.ID,
                No = u.No,
                Nom = u.Nom,
                Prenom = u.Prenom,
                Statut = u.Statut,
                Defaillance = u.Defaillance,
                Courriel = u.Courriel
            });
        }
    }
}
