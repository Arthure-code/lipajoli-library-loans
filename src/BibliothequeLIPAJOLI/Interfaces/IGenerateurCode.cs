namespace BibliothequeLIPAJOLI.Interfaces
{
    public interface IGenerateurCode
    {
        Task<string> GenererCode(string categorie);
    }
}
