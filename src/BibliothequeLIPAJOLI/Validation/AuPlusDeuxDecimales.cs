using System.ComponentModel.DataAnnotations;

namespace BibliothequeLIPAJOLI.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AuPlusDeuxDecimalesAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
            {
                return true;
            }

            if (value is not decimal nombre)
            {
                return false;
            }

            return nombre == decimal.Round(nombre, 2);
        }
    }
}
