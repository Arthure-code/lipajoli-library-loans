using Bibliotheques.ApplicationCore.Entites;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheques.Infrastructure.Data
{
    public class EmpruntsContext : DbContext
    {
        public EmpruntsContext(DbContextOptions<EmpruntsContext> options) : base(options) { 
        
        }

        public DbSet<Livre> Livres { get; set; }
        public DbSet<Usager> Usagers { get; set; }
        public DbSet<Emprunt> Emprunts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Livre>().ToTable("Livre");
            modelBuilder.Entity<Usager>().ToTable("Usager");
            modelBuilder.Entity<Emprunt>().ToTable("Emprunt");
        }

    }
}
