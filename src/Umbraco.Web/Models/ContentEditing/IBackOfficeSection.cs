using Umbraco.Core.Composing;

namespace Umbraco.Web.Models.ContentEditing
{
    
    /// <summary>
    /// Defines a back office section
    /// </summary>
    public interface IBackOfficeSection : IDiscoverable
    {
        string Alias { get; }
        string Name { get; }
        int SortOrder { get; }
    }
}
