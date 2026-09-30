using BibliothequeLIPAJOLI.Data;
using BibliothequeLIPAJOLI.Interfaces;
using BibliothequeLIPAJOLI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("fr-FR");
});

builder.Services.AddControllersWithViews();

string adresseDeLApi = builder.Configuration.GetValue<string>("urlAPI")
    ?? throw new InvalidOperationException("L'adresse de l'API est absente de la configuration.");

builder.Services.AddHttpClient<IEmpruntsService, EmpruntsServiceProxy>(client => client.BaseAddress =
      new Uri(adresseDeLApi));

builder.Services.AddHttpClient<ILivresServiceProxy, LivresServiceProxy>(client => client.BaseAddress =
      new Uri(adresseDeLApi));

builder.Services.AddHttpClient<IUsagersServiceProxy, UsagersServiceProxy>(client => client.BaseAddress =
      new Uri(adresseDeLApi));


builder.Services.AddDbContext<BibliothequeContext>(options =>
  options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IGenerateurCode, GenerateurCode>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<BibliothequeContext>();
   // context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();