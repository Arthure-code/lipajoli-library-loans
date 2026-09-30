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

namespace BibliothequeLIPAJOLI.Controllers
{
    public class UsagersController : Controller
    {
        private readonly BibliothequeContext _context;
        private readonly IConfiguration _config;

        public UsagersController(BibliothequeContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var usagers = from u in _context.Usagers
                          select u;



            if (!String.IsNullOrEmpty(searchString))
            {
                usagers = usagers.Where(u => u.Nom.ToLower().Contains(searchString.ToLower())
                                       || u.Prenom.ToLower().Contains(searchString.ToLower()));
            }

            return View(await usagers.AsNoTracking().ToListAsync());
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var usager = await _context.Usagers
                .Include(s => s.Emprunts!) // On voudra les informations des emprunts
                    .ThenInclude(e => e.Livre)// On voudra les informations sur le livre associé à l'emprunt
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (usager == null)
            {
                return View("Error");
            }

            return View(usager);
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Statut,Courriel")] Usager usager)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    usager.No = _context.Usagers
                        .Max(u => u.No) + 1;

                    _context.Add(usager);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Inpossible de faire les changement. " +
                    "Essayer plus tard, Si le problème persiste " +
                    "contacter votre opérateur système.");

            }
            return View(usager);
        }

        // GET: Usagers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var usager = await _context.Usagers.FindAsync(id);
            if (usager == null)
            {
                return View("Error");
            }

            return View(usager);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            var usagerAModifier = await _context.Usagers
                .FirstOrDefaultAsync(u => u.ID == id);

            if (usagerAModifier == null)         
            {
                return View("Error");
            }

            if (await TryUpdateModelAsync<Usager>(usagerAModifier, "", u => u.Nom, u => u.Prenom, u => u.Statut, u => u.Defaillance, u => u.Courriel)) // SH - Ajouter le nombre de livre emprunte si on determine que c'est pertinant pour la suite
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Une erreur est survenue dans la modification de l'entrée.");
                }
                return RedirectToAction(nameof(Index));
            }

            return View(usagerAModifier);

        }

        // GET: Usagers/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return View("Error");
            }

            var usager = await _context.Usagers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (usager == null)
            {
                return View("Error");
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["MessageErreur"] = "Cet usager ne peur pas être supprimé de la base de données puisque son dossier d'emprunt n'est pas vide.";
            }

            return View(usager);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usager = await _context.Usagers
                .Include(u => u.Emprunts)
                .FirstOrDefaultAsync(u => u.ID == id);

            if (usager == null)
            {
                return RedirectToAction(nameof(Index));
            }



                _context.Usagers.Remove(usager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           

        }

        private bool UsagerExists(int id)
        {
            return _context.Usagers.Any(e => e.ID == id);
        }

    }
}
