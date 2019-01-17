namespace Umbraco.Web.Models.Trees
{
    
    /// <summary>
    /// Defines a back office section
    /// </summary>
    public interface IBackOfficeSection
    {
        string Alias { get; }
        string Name { get; }
    }
}
