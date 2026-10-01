using Bibliotheques.ApplicationCore.Entites;
using Bibliotheques.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliotheques.ApplicationCore.Services
{
    public class EmpruntsService : IEmpruntsService
    {

        private readonly IAsyncRepository<Livre> _livreRepository;
        private readonly IAsyncRepository<Usager> _usagerRepository;
        private readonly IAsyncRepository<Emprunt> _empruntRepository;
        private readonly IConfiguration _configuration;

        public EmpruntsService(IAsyncRepository<Emprunt> empruntRepository, IConfiguration configuration,
                              IAsyncRepository<Livre> livreRepository, IAsyncRepository<Usager> usagerRepository)
        {
            _empruntRepository = empruntRepository;
            _usagerRepository = usagerRepository;
            _configuration = configuration;
            _livreRepository = livreRepository;
        }

        public Task<IEnumerable<Emprunt>> ObtenirToutEmprunts()
        {
            return _empruntRepository.ListAsync();
        }

        public Task<Emprunt?> ObtenirUnEmpruntParId(int id)
        {
            return _empruntRepository.GetByIdAsync(id);
        }

        public async Task SupprimerUnEmprunt(Emprunt emprunt)
        {
            // Sans livre, sans usager ou sans code, l'emprunt ne designe rien.
            if (emprunt?.Livre == null || emprunt.Usager == null
                || string.IsNullOrWhiteSpace(emprunt.Livre.CodeUnique))
            {
                return;
            }

            string codeDuLivre = emprunt.Livre.CodeUnique;
            int numeroDeLUsager = emprunt.Usager.No;

            IEnumerable<Livre> livres = await _livreRepository.ListAsync();
            Livre? livre = livres.FirstOrDefault(l => l.CodeUnique == codeDuLivre);
            if (livre == null)
            {
                return;
            }

            IEnumerable<Usager> usagers = await _usagerRepository.ListAsync();
            Usager? usagerTrouve = usagers.FirstOrDefault(u => u.No == numeroDeLUsager);
            if (usagerTrouve == null)
            {
                return;
            }

            // Les cles suffisent : interroger les proprietes de navigation
            // ferait charger chaque livre et chaque usager un par un.
            IEnumerable<Emprunt> emprunts = await _empruntRepository.ListAsync();
            Emprunt? empruntActif = emprunts.FirstOrDefault(e =>
                e.LivreID == livre.ID &&
                e.UsagerID == usagerTrouve.ID &&
                e.DateRetour == null);

            if (empruntActif == null)
            {
                return;
            }

            await _empruntRepository.DeleteAsync(empruntActif);

            livre.Quantite += 1;
            await _livreRepository.EditAsync(livre);
        }

        public async Task<Emprunt?> InscrireUnNouvelEmprunt(Usager usager, Livre livre)
        {
            
            var usagerSuivi = await _usagerRepository.GetByIdAsync(usager.ID);
            var livreSuivi = await _livreRepository.GetByIdAsync(livre.ID);

            if (usagerSuivi == null || livreSuivi == null || livreSuivi.Quantite <= 0)
                return null; 

            
            var empruntsActifs = await _empruntRepository.ListAsync(e =>
                e.UsagerID == usagerSuivi.ID &&
                e.DateRetour == null &&
                e.LivreID == livreSuivi.ID);

            if (empruntsActifs.Any())
                return null; 

            var nbJours = _configuration.GetValue<int>("ParametresEmprunt:DureeMax", 10);

         
            var nouvelEmprunt = new Emprunt
            {
                UsagerID = usagerSuivi.ID,
                LivreID = livreSuivi.ID,
                DateEmprunt = DateTime.Today,
                DateRetourLimite = DateTime.Today.AddDays(nbJours)
            };

           
            await _empruntRepository.AddAsync(nouvelEmprunt);

            livreSuivi.Quantite -= 1;
            await _livreRepository.EditAsync(livreSuivi);

            return nouvelEmprunt;
        }

        public async Task<Emprunt?> RetournerUnEmprunt(Usager usager, Livre livre)
        {
            if (usager == null || livre == null || string.IsNullOrWhiteSpace(livre.CodeUnique))
            {
                return null;
            }

            IEnumerable<Livre> livres = await _livreRepository.ListAsync();
            Livre? livreSuivi = livres.FirstOrDefault(l => l.CodeUnique == livre.CodeUnique);

            IEnumerable<Usager> usagers = await _usagerRepository.ListAsync();
            Usager? usagerTrouve = usagers.FirstOrDefault(u => u.No == usager.No);

            if (livreSuivi == null || usagerTrouve == null)
            {
                return null;
            }

            IEnumerable<Emprunt> emprunts = await _empruntRepository.ListAsync();
            Emprunt? empruntActif = emprunts.FirstOrDefault(e =>
                e.LivreID == livreSuivi.ID &&
                e.UsagerID == usagerTrouve.ID &&
                e.DateRetour == null);

            if (empruntActif == null)
            {
                return null;
            }

            empruntActif.DateRetour = DateTime.Today;
            await _empruntRepository.EditAsync(empruntActif);

            livreSuivi.Quantite += 1;
            await _livreRepository.EditAsync(livreSuivi);

            // Un retour apres la date limite laisse une trace au dossier.
            if (empruntActif.DateRetour > empruntActif.DateRetourLimite)
            {
                usagerTrouve.Defaillance += 1;
                await _usagerRepository.EditAsync(usagerTrouve);
            }

            return empruntActif;
        }
    }
}


