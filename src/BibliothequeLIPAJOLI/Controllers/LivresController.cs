using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using BibliothequeLIPAJOLI.Interfaces;

namespace BibliothequeLIPAJOLI.Controllers
{
    public class LivresController : Controller
    {
        private const string Erreur = "Error";
        private const string TitreDescendant = "titre_desc";

        private readonly BibliothequeContext _context;
        private readonly IConfiguration _config;
        private readonly IGenerateurCode _generateurCode;

        public LivresController(BibliothequeContext context, IConfiguration config, IGenerateurCode generateurCode)
        {
            _context = context;
            _config = config;
            _generateurCode = generateurCode;
        }

        public async Task<IActionResult> Index(string ordreTri, string searchString)
        {
            ViewData["TriParCodeUnique"] = "";
            if (string.IsNullOrEmpty(ordreTri))
            {
                ViewData["TriParCodeUnique"] = "code_unique_desc";
            }

            ViewData["TriParTitre"] = TitreDescendant;
            if (ordreTri == TitreDescendant)
            {
                ViewData["TriParTitre"] = "titre";
            }
            else if (ordreTri == "titre")
            {
                ViewData["TriParTitre"] = TitreDescendant;
            }

            var livres = from l in _context.Livres
                         select l;

            switch (ordreTri)
            {
                case "code_unique_desc":
                    livres = livres.OrderByDescending(l => l.CodeUnique);
                    break;
                case "titre":
                    livres = livres.OrderBy(l => l.Titre);
                    break;
                case TitreDescendant:
                    livres = livres.OrderByDescending(l => l.Titre);
                    break;
                default:
                    livres = livres.OrderBy(l => l.CodeUnique);
                    break;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                livres = livres.Where(l => l.Titre.ToLower().Contains(searchString.ToLower())
                                       || (l.Auteurs ?? string.Empty).ToLower().Contains(searchString.ToLower())
                                       || l.Categorie.ToLower().Contains(searchString.ToLower()));
            }

            return View(await livres.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View(Erreur);
            }

            var livre = await _context.Livres
                .Include(l => l.Emprunts!)
                .ThenInclude(e => e.Usager)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (livre == null)
            {
                return View(Erreur);
            }

            return View(livre);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = RecupererCategories();
            ViewBag.Auteurs = RecupererAuteurs();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodeUnique,Isbn10,Isbn13,Titre,Quantite,Prix,Auteurs,Categorie")] Livre livre, string[] auteursSelectiones)
        {
            ViewBag.Categories = RecupererCategories();
            ViewBag.Auteurs = RecupererAuteurs();
            
            if (!auteursSelectiones.Any())
            {
                ModelState.AddModelError("Auteurs", "Vous devez choisir au moins un auteur");
                
            }

            VerifierISBN10et13(livre);

            livre.Auteurs = ListeAuteursEnString(auteursSelectiones);

           
             if (ModelState.IsValid)
                {
                    //pour ajouter le code
                    livre.CodeUnique = await _generateurCode.GenererCode(livre.Categorie);

                    _context.Add(livre);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            
           
            return View(livre);
        }

        public async Task<IActionResult> Edit(int? id)
        {


            ViewBag.Categories = RecupererCategories();
            ViewBag.Auteurs = RecupererAuteurs();

            if (id == null)
            {
                return View(Erreur);
            }

            var livre = await _context.Livres.FindAsync(id);

            if (livre == null)
            {
                return View(Erreur);
            }
            return View(livre);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLivre([FromRoute] int? id, Livre livre, string[] auteursSelectiones)
        {
            if (id == null)
            {
                return View(Erreur);
            }

            ViewBag.Categories = RecupererCategories();
            ViewBag.Auteurs = RecupererAuteurs();

            Livre? livreEnBase = await _context.Livres.AsNoTracking()
                .FirstOrDefaultAsync(l => l.ID == id);

            if (livreEnBase == null)
            {
                return View(Erreur);
            }

            VerifierISBN10et13(livre, livreEnBase.ID);

            // L'identite du livre ne vient pas du formulaire : sa cle est
            // dans l'adresse, et son code appartient a la bibliotheque, qui
            // ne le refait que si la categorie change.
            livre.ID = livreEnBase.ID;
            livre.CodeUnique = livreEnBase.Categorie == livre.Categorie
                ? livreEnBase.CodeUnique
                : await _generateurCode.GenererCode(livre.Categorie);

            livre.Auteurs = ListeAuteursEnString(auteursSelectiones);

            if (ModelState.IsValid)
            {
                _context.Update(livre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(livre);
        }

        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return View(Erreur);
            }

            var livre = await _context.Livres
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (livre == null)
            {
                return View(Erreur);
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["MessageErreur"] = "Ce livre ne peut pas être supprimé de la base de données puisque sa liste d'emprunts n'est pas vide.";
            }

            return View(livre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            var livre = await _context.Livres
                .Include(e => e.Emprunts)
                .FirstOrDefaultAsync(l => l.ID == id);

            if (livre == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (livre.Emprunts?.Count > 0)
                {

                    throw new DbUpdateException();
                }

                if (ModelState.IsValid)
                {
                    _context.Livres.Remove(livre);
                    await _context.SaveChangesAsync();            
                    return RedirectToAction(nameof(Index));
                }


            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });

            }
            return View(livre);

        }

        private bool LivreExists(int id)
        {
            return _context.Livres.Any(e => e.ID == id);
        }

        /// <summary>
        /// Une méthode pour récupérer la liste des auteurs à partir de appsettings.json
        /// </summary>
        /// <returns>Liste des auteurs</returns>
        private List<string> RecupererAuteurs()
        {
            return _config.GetSection("Livre:Auteurs").Get<string[]>()?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Une méthode pour récupérer la liste des catégories à partir de appsettings.json
        /// </summary>
        /// <returns>Liste des catégories</returns>
        private List<string> RecupererCategories()
        {
            return _config.GetSection("Livre:Categories").Get<string[]>()?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Une méthode pour joindre la liste des auteurs sélectionnés pour un livre en une string, chaque auteur est sépraré par une virgule
        /// </summary>
        /// <param name="auteurs">Un tableau d'auteurs</param>
        /// <returns>La liste des auteurs en string</returns>
        private static string ListeAuteursEnString(string[] auteurs)
        {
            return string.Join(", ", auteurs);
        }



        // Un livre n'est pas son propre doublon : quand on le modifie, on
        // compare son ISBN a celui des autres, pas au sien.
        private void VerifierISBN10et13(Livre livreAVerifier, int identifiantAIgnorer = 0)
        {
            IQueryable<Livre> autresLivres = _context.Livres.Where(l => l.ID != identifiantAIgnorer);

            if (autresLivres.Any(l => l.Isbn10 == livreAVerifier.Isbn10))
            {
                ModelState.AddModelError("Isbn10", "Cet ISBN10 est déjà enregistré.");
            }

            if (autresLivres.Any(l => l.Isbn13 == livreAVerifier.Isbn13))
            {
                ModelState.AddModelError("Isbn13", "Cet ISBN13 est déjà enregistré.");
            }
        }
    }
}
