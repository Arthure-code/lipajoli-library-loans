using Bibliotheques.ApplicationCore.Interfaces;
using Bibliotheques.ApplicationCore.Services;
using Bibliotheques.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Bibliotheques.API
{
    public class Program
    {
        // La classe ne sert qu'a porter le point d'entree, mais les tests
        // fonctionnels la designent : elle ne peut pas etre statique.
        protected Program()
        {
        }

        // Une adresse absente laisse simplement le lien vide dans la fiche.
        private static Uri? Adresse(string? valeur)
        {
            return string.IsNullOrWhiteSpace(valeur) ? null : new Uri(valeur);
        }

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API Emprunt",
                    Version = "v1",
                    Description = "Gestion des emprunts de la bibliothèque LIPAJOLI",
                    License = new OpenApiLicense
                    {
                        Name = builder.Configuration["Documentation:Licence"],
                        Url = Adresse(builder.Configuration["Documentation:LicenceUrl"])
                    },
                    Contact = new OpenApiContact
                    {
                        Name = builder.Configuration["Documentation:Auteur"],
                        Url = Adresse(builder.Configuration["Documentation:AuteurUrl"])
                    }
                });

                //Ajout documentation XML
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddDbContext<EmpruntsContext>(options =>
            options.UseLazyLoadingProxies()
            .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped(typeof(IAsyncRepository<>), typeof(AsyncRepository<>));
            builder.Services.AddScoped<IEmpruntsService, EmpruntsService>();
            builder.Services.AddScoped<ILivresService, LivresService>();
            builder.Services.AddScoped<IUsagersService, UsagersService>();
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
