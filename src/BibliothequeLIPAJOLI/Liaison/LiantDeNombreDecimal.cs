using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliothequeLIPAJOLI.Liaison
{
    public class LiantDeNombreDecimal : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            ValueProviderResult valeurEnvoyee = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valeurEnvoyee == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valeurEnvoyee);
            string? texte = valeurEnvoyee.FirstValue;

            if (string.IsNullOrWhiteSpace(texte))
            {
                // Un champ vide reste vide : c'est au modèle de dire s'il
                // était obligatoire.
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (!NombreDecimal.EssayerDeLire(texte, out decimal nombre))
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    $"Écrivez ce nombre avec la virgule ou le point, par exemple 18,50 ou 18.50.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(nombre);
            return Task.CompletedTask;
        }
    }

    public class FournisseurDeLiantDecimal : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Type type = context.Metadata.UnderlyingOrModelType;
            return type == typeof(decimal) ? new LiantDeNombreDecimal() : null;
        }
    }
}
