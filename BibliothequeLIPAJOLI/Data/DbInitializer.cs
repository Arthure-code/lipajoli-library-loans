using BibliothequeLIPAJOLI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BibliothequeLIPAJOLI.Data
{
    public class DbInitializer
    {

        public static void Initialize(BibliothequeContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Usagers.Any())
            {
                return;   // DB has been seeded
            }

            var usagers = new Usager[]
            {
            new Usager{ ID = 1,Courriel="abc@def.com", Prenom="Bob", Nom="Bricoleur", Statut=Statut.Enseignant, No=123456},
            new Usager{ ID = 2,Courriel="123@456.ca", Prenom="Dora", Nom="Exploratrice", Statut=Statut.Étudiant, No=098765, Defaillance=1}
            };

            foreach (Usager u in usagers)
            {
                context.Usagers.Add(u);
            }
            context.SaveChanges();

            var livres = new Livre[]
            {
            new Livre{Titre="Anna Karenina", Auteurs="Tolstoy",Categorie="Fiction", CodeUnique="FIC004", Isbn10="0393966429", Isbn13="9780393966428", Prix=10.99m, Quantite=2},
            new Livre{Titre="L'école des femmes", Auteurs="Molière",Categorie="Fiction", CodeUnique="FIC003", Isbn10="0151795800", Isbn13="9780151795802", Prix=6.99m, Quantite=6},
            new Livre{Titre="Test 2 auteurs", Auteurs="Tolstoy" + "," + " " + "Molière",Categorie="Fiction", CodeUnique="FIC001", Isbn10="3770121880", Isbn13="9783770121885", Prix=0.99m, Quantite=666},
            new Livre{Titre="Guerre et Paix", Auteurs="Tolstoy",Categorie="Fiction", CodeUnique="FIC002", Isbn10="8804682590", Isbn13="9788804682592", Prix=12.99m, Quantite=4}
            };
            foreach (Livre l in livres)
            {
                context.Livres.Add(l);
            }
            context.SaveChanges();

            var aujourdhui = DateTime.Today;

            var emprunts = new Emprunt[]
            {
            // En cours : emprunte il y a trois jours, a rendre dans sept.
            new Emprunt{LivreID=1, UsagerID=1, DateEmprunt=aujourdhui.AddDays(-3), DateRetourLimite=aujourdhui.AddDays(7)},
            // Rendu a temps, trois jours avant la date limite.
            new Emprunt{LivreID=2, UsagerID=1, DateEmprunt=aujourdhui.AddDays(-20), DateRetourLimite=aujourdhui.AddDays(-10), DateRetour=aujourdhui.AddDays(-13)},
            // Rendu cinq jours en retard : le dossier de l'usager en garde une defaillance.
            new Emprunt{LivreID=3, UsagerID=2, DateEmprunt=aujourdhui.AddDays(-30), DateRetourLimite=aujourdhui.AddDays(-20), DateRetour=aujourdhui.AddDays(-15)},
            // En cours pour le deuxieme usager.
            new Emprunt{LivreID=4, UsagerID=2, DateEmprunt=aujourdhui.AddDays(-1), DateRetourLimite=aujourdhui.AddDays(9)}
            };
            foreach (Emprunt e in emprunts)
            {
                context.Emprunts.Add(e);
            }
            context.SaveChanges();
        }
    }
}

