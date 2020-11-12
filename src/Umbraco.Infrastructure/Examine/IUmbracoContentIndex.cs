using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Marker interface for indexes of Umbraco content
    /// </summary>
    public interface IUmbracoContentIndex : IIndex
    {
        bool PublishedValuesOnly { get; }
    }
}
