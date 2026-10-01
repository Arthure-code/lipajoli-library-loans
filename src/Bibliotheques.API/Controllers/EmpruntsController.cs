
using Bibliotheques.ApplicationCore.DTOs;
using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Bibliotheques.ApplicationCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bibliotheques.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpruntsController : ControllerBase
    {
        private readonly IEmpruntsService _empruntsService;

        public EmpruntsController(IEmpruntsService empruntsService)
        {
            _empruntsService = empruntsService;
        }


        /// <summary>
        /// Retourne la liste de tous les emprunts
        /// </summary>
        /// <remarks>
        /// Récupère tous les emprunts avec les informations complètes des usagers et livres associés.
        /// Les emprunts incluent les dates d'emprunt, de retour limite et de retour effectif.
        /// </remarks>
        /// <returns>Liste des emprunts avec leurs détails</returns>
        /// <response code="200">Liste des emprunts retournée avec succès</response>
        /// <response code="500">Erreur interne du serveur</response>

        // GET: api/<EmpruntsController>
        [HttpGet]
        public async Task<IEnumerable<EmpruntDto>> Get()
        {
            var emprunts = await _empruntsService.ObtenirToutEmprunts();
            return emprunts.Select(VersDto);
        }


        /// <summary>
        /// Retourne un emprunt spécifique à partir de son identifiant
        /// </summary>
        /// <remarks>
        /// Récupère un emprunt avec toutes les informations de l'usager et du livre associé.
        /// </remarks>
        /// <param name="id">Identifiant unique de l'emprunt à retourner</param>
        /// <returns>Emprunt correspondant à l'identifiant fourni</returns>
        /// <response code="200">Emprunt trouvé et retourné avec succès</response>
        /// <response code="404">Emprunt introuvable pour l'identifiant spécifié</response>
        /// <response code="500">Erreur interne du serveur</response>

        // GET api/<EmpruntsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmpruntDto>> Get(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            if (emprunt == null)
            {
                return NotFound();
            }

            return Ok(VersDto(emprunt));
        }


        /// <summary>
        /// Crée un nouvel emprunt de livre
        /// </summary>
        /// <remarks>
        /// Enregistre un nouvel emprunt pour un usager et un livre donnés.
        /// Vérifie automatiquement :
        /// - La disponibilité du livre (quantité en stock)
        /// - Que l'usager n'a pas déjà emprunté ce livre
        /// - Les limites d'emprunt de l'usager
        /// 
        /// La date d'emprunt est fixée au jour courant et la date limite est calculée 
        /// selon la configuration (par défaut 10 jours).
        /// </remarks>
        /// <param name="empruntDto">Données de l'emprunt à créer (doit inclure usager et livre complets)</param>
        /// <returns>Emprunt créé avec son identifiant unique</returns>
        /// <response code="201">Emprunt créé avec succès</response>
        /// <response code="400">Données invalides ou conditions d'emprunt non respectées</response>
        /// <response code="500">Erreur interne du serveur</response>

        [HttpPost]
        public async Task<ActionResult<EmpruntDto>> Post([FromBody] EmpruntDto empruntDto)
        {
            if (empruntDto == null || empruntDto.Usager == null || empruntDto.Livre == null)
                return BadRequest("Les données de l'usager ou du livre sont manquantes.");

            // Mapping manuel des DTO vers les entites
            Usager usager = VersEntite(empruntDto.Usager);

            Livre livre = VersEntite(empruntDto.Livre);

            var nouvelEmprunt = await _empruntsService.InscrireUnNouvelEmprunt(usager, livre);

            if (nouvelEmprunt == null)
                return BadRequest("Impossible de créer l'emprunt. Vérifiez la disponibilité du livre ou les informations de l'usager.");

            EmpruntDto empruntResultDto = VersDto(nouvelEmprunt);
            empruntResultDto.Usager = empruntDto.Usager;
            empruntResultDto.Livre = empruntDto.Livre;

            return CreatedAtAction(nameof(Get), new { id = nouvelEmprunt.ID }, empruntResultDto);
        }



        /// <summary>
        /// Traite le retour d'un livre emprunté
        /// </summary>
        /// <remarks>
        /// Met à jour un emprunt pour marquer le retour du livre.
        /// Actions automatiques :
        /// - Définit la date de retour au jour courant
        /// - Remet le livre en stock (quantité +1)
        /// - Applique une défaillance à l'usager si retour en retard
        /// 
        /// L'identifiant dans l'URL doit correspondre à celui dans les données.
        /// </remarks>
        /// <param name="id">Identifiant de l'emprunt à retourner</param>
        /// <param name="empruntDto">Données complètes de l'emprunt (usager et livre)</param>
        /// <returns>Confirmation du traitement</returns>
        /// <response code="200">Retour traité avec succès</response>
        /// <response code="400">Données invalides ou identifiants incohérents</response>
        /// <response code="404">Aucun emprunt actif trouvé pour ce livre et cet usager</response>
        /// <response code="500">Erreur interne du serveur</response>

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] EmpruntDto empruntDto)
        {
            if (empruntDto == null || empruntDto.Usager == null || empruntDto.Livre == null)
                return BadRequest("Les données de l'usager ou du livre sont manquantes.");

            if (id != empruntDto.Id)
                return BadRequest("L'ID dans l'URL ne correspond pas à l'emprunt.");

            Usager usager = VersEntite(empruntDto.Usager);

            Livre livre = VersEntite(empruntDto.Livre);

            var empruntRetourné = await _empruntsService.RetournerUnEmprunt(usager, livre);

            if (empruntRetourné == null)
                return NotFound("Aucun emprunt actif trouvé pour ce livre et cet usager.");

            return Ok();
        }


        /// <summary>
        /// Supprime un emprunt (annulation)
        /// </summary>
        /// <remarks>
        /// Supprime définitivement un emprunt de la base de données.
        /// Cette opération est irréversible et ne doit être utilisée que pour 
        /// annuler des emprunts qui n'ont pas encore été retournés.
        /// 
        /// Le livre est automatiquement remis en stock.
        /// </remarks>
        /// <param name="id">Identifiant de l'emprunt à supprimer</param>
        /// <returns>Confirmation de suppression</returns>
        /// <response code="204">Emprunt supprimé avec succès</response>
        /// <response code="400">Impossible de supprimer un emprunt déjà retourné</response>
        /// <response code="404">Emprunt introuvable pour l'identifiant spécifié</response>
        /// <response code="500">Erreur interne du serveur</response>
        // DELETE api/<EmpruntsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);

            if (emprunt == null)
                return NotFound();

            if (emprunt.DateRetour != null)
                return BadRequest("Cet emprunt a déjà été retourné et ne peut pas être supprimé.");

            await _empruntsService.SupprimerUnEmprunt(emprunt);

            return NoContent(); 
        }

        // Un emprunt tel que les pages le lisent : ses donnees, et celles de
        // l'usager et du livre, sans les proprietes de navigation qui
        // ramenent la serialisation sur ses pas.
        private static EmpruntDto VersDto(Emprunt emprunt)
        {
            return new EmpruntDto
            {
                Id = emprunt.ID,
                UsagerID = emprunt.UsagerID,
                LivreID = emprunt.LivreID,
                DateEmprunt = emprunt.DateEmprunt,
                DateRetourLimite = emprunt.DateRetourLimite,
                DateRetour = emprunt.DateRetour,
                Usager = emprunt.Usager == null ? null : VersDto(emprunt.Usager),
                Livre = emprunt.Livre == null ? null : VersDto(emprunt.Livre)
            };
        }

        private static UsagerDto VersDto(Usager usager)
        {
            return new UsagerDto
            {
                Id = usager.ID,
                No = usager.No,
                Nom = usager.Nom,
                Prenom = usager.Prenom,
                Statut = usager.Statut,
                Defaillance = usager.Defaillance,
                Courriel = usager.Courriel
            };
        }

        private static LivreDto VersDto(Livre livre)
        {
            return new LivreDto
            {
                Id = livre.ID,
                CodeUnique = livre.CodeUnique,
                Isbn10 = livre.Isbn10,
                Isbn13 = livre.Isbn13,
                Titre = livre.Titre,
                Quantite = livre.Quantite,
                Prix = livre.Prix,
                Auteurs = livre.Auteurs,
                Categorie = livre.Categorie
            };
        }

        private static Usager VersEntite(UsagerDto usager)
        {
            return new Usager
            {
                ID = usager.Id,
                No = usager.No,
                Nom = usager.Nom,
                Prenom = usager.Prenom,
                Statut = usager.Statut,
                Defaillance = usager.Defaillance,
                Courriel = usager.Courriel
            };
        }

        private static Livre VersEntite(LivreDto livre)
        {
            return new Livre
            {
                ID = livre.Id,
                CodeUnique = livre.CodeUnique,
                Isbn10 = livre.Isbn10,
                Isbn13 = livre.Isbn13,
                Titre = livre.Titre,
                Quantite = livre.Quantite,
                Prix = livre.Prix,
                Auteurs = livre.Auteurs,
                Categorie = livre.Categorie
            };
        }
    }
}
