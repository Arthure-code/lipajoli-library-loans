using BibliothequeLIPAJOLI.DTOs;
using BibliothequeLIPAJOLI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliothequeLIPAJOLI.Controllers
{
    public class EmpruntsController : Controller
    {
        private readonly IEmpruntsService _empruntsService;
        private readonly IUsagersServiceProxy _usagersService;
        private readonly ILivresServiceProxy _livresService;
        public EmpruntsController(IEmpruntsService empruntsService,IUsagersServiceProxy usagersServiceProxy,ILivresServiceProxy livresServiceProxy)
        {
            _empruntsService = empruntsService;
            _usagersService = usagersServiceProxy;
            _livresService = livresServiceProxy;
        }

        // GET: Emprunts
        public async Task<IActionResult> Index(string statut = "tous")
        {
            var emprunts = await _empruntsService.ObtenirToutEmprunts();

            var empruntsFiltres = statut switch
            {
                "enCours" => emprunts.Where(e => e.DateRetour == null),
                "retournes" => emprunts.Where(e => e.DateRetour != null && e.DateRetour <= e.DateRetourLimite),
                "enRetard" => emprunts.Where(e => e.DateRetour != null && e.DateRetour > e.DateRetourLimite),
                _ => emprunts
            };

            ViewBag.Statut = statut;
            return View(empruntsFiltres.OrderByDescending(e=>e.DateEmprunt));
        }

        // GET: Emprunts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            if (emprunt == null)
                return NotFound();

            return View(emprunt);
        }

        public async Task<IActionResult> Create()
        {
            var emprunt = new EmpruntDto
            {
                Usager = new UsagerDto(),
                Livre = new LivreDto(),
                DateEmprunt = DateTime.Now,
                DateRetourLimite = DateTime.Now.AddDays(10),
                DateRetour = null  // AJOUTEZ cette ligne
            };

            return View(emprunt);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpruntDto emprunt)
        {
            ModelState.Clear();
            if (!ModelState.IsValid)
                return View(emprunt);

            var usagers = await _usagersService.ObtenirToutUsagers();
            var livres = await _livresService.ObtenirToutLivres();

            var usager = usagers.FirstOrDefault(u => u.No == emprunt.Usager?.No);
            var livre = livres.FirstOrDefault(l => l.CodeUnique == emprunt.Livre?.CodeUnique);

            if (usager == null || livre == null)
            {
                ModelState.AddModelError("", "Usager ou livre introuvable.");
                return View(emprunt);
            }

            var empruntsActifs = await _empruntsService.ObtenirToutEmprunts();
            var empruntsUsager = empruntsActifs
                .Where(e => e.UsagerID == usager.Id && e.DateRetour == null)
                .ToList();

            if (empruntsUsager.Count >= 3)
            {
                ModelState.AddModelError("", "L'usager a déjà emprunté 3 livres.");
                return View(emprunt);
            }

            if (empruntsUsager.Any(e => e.LivreID == livre.Id))
            {
                ModelState.AddModelError("", "L'usager a déjà emprunté cet exemplaire.");
                return View(emprunt);
            }

                       var nouvelEmprunt = new EmpruntDto
            {
                Id = 0,
                UsagerID = usager.Id,
                LivreID = livre.Id,
                DateEmprunt = DateTime.Now,
                DateRetourLimite = DateTime.Now.AddDays(10),
                DateRetour = null,
                Usager = usager,
                Livre = livre
            };

            try
            {
                await _empruntsService.InscrireUnNouvelEmprunt(nouvelEmprunt);
                TempData["Message"] = "Emprunt enregistré avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur lors de l'enregistrement: {ex.Message}");
                return View(emprunt);
            }
        }


        // PUT: Emprunts/Retourner/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retourner(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            await _empruntsService.RetournerUnEmprunt(emprunt);
            return RedirectToAction(nameof(Index));
        }

        // GET: Emprunts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            if (emprunt == null)
                return NotFound();

            return View(emprunt);
        }


        // POST: Emprunts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            if (emprunt == null || emprunt.DateRetour != null)
            {
                ModelState.AddModelError(string.Empty, "Impossible de supprimer un emprunt qui a été retourné ou qui n'existe pas.");
                return RedirectToAction(nameof(Index));
            }

             await _empruntsService.SuprimerUnEmprunt(emprunt);

            return RedirectToAction(nameof(Index));
        }

        // GET: Emprunts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var emprunt = await _empruntsService.ObtenirUnEmpruntParId(id);
            if (emprunt == null)
                return NotFound();

            return View(emprunt);
        }

        // POST: Emprunts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmpruntDto empruntDto)
        {
            if (id != empruntDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(empruntDto);

            try
            {
                await _empruntsService.RetournerUnEmprunt(empruntDto);
                TempData["Message"] = "Retour effectué avec succès.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors du retour.");
                return View(empruntDto);
            }
        }
    }
}
