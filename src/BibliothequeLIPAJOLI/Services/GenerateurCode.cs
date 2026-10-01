using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeLIPAJOLI.Services
{
    public class GenerateurCode : IGenerateurCode
    {


        private readonly BibliothequeContext _context;

        public GenerateurCode(BibliothequeContext context)
        {
            _context = context;
        }

        public async Task<string> GenererCode(string categorie)
        {
            var livresCategorie = await _context.Livres.Where(l => l.Categorie == categorie).ToListAsync();

            string strCategorie = categorie.Substring(0, 3).ToUpper(); // Code catégorie
            int numLivresCategorie = 1;
            if (livresCategorie.Count > 0)
            {
                numLivresCategorie = livresCategorie
                    .Select(l => Sequence(l.CodeUnique))
                    .DefaultIfEmpty(0)
                    .Max() + 1;
            }
            return strCategorie + numLivresCategorie.ToString("D3");
        }

        // Les trois chiffres qui suivent le prefixe. Un code absent ou mal
        // forme ne compte pas, plutot que d'arreter l'attribution.
        private static int Sequence(string? code)
        {
            if (code == null || code.Length < 6)
            {
                return 0;
            }

            return int.TryParse(code.Substring(3, 3), out int numero) ? numero : 0;
        }
    }
}
