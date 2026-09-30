using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bibliotheques.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivresController : ControllerBase
    {
        private readonly ILivresService _livresService;

        public LivresController(ILivresService livresService)
        {
            _livresService = livresService;
        }


        /// <summary>
        /// Retourne la liste de tous les livres disponibles dans le système.
        /// </summary>
        /// <remarks>
        /// Récupère tous les livres enregistrés, avec leurs informations complètes :
        /// code unique, ISBN, titre, quantité, prix, auteurs et catégorie.
        /// </remarks>
        /// <returns>Liste des livres sous forme de <see cref="LivreDto"/>.</returns>
        /// <response code="200">Liste des livres retournée avec succès.</response>
        /// <response code="500">Erreur interne du serveur lors de la récupération des livres.</response>
        
        // GET: api/<LivresController>
        [HttpGet]
        public async Task<IEnumerable<LivreDto>> Get() 
        {
            var livres = await _livresService.ObtenirToutLivres();
            return livres.Select(l => new LivreDto
            {
                Id = l.ID,
                CodeUnique = l.CodeUnique,
                Isbn10 = l.Isbn10,
                Isbn13 = l.Isbn13,
                Titre = l.Titre,
                Quantite = l.Quantite,
                Prix = l.Prix,
                Auteurs = l.Auteurs,
                Categorie = l.Categorie
            });
        }
    }
}
