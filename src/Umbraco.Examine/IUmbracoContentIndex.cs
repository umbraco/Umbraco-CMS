using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// Marker interface for indexes of Umbraco content
    /// </summary>
    /// <remarks>
    /// This is a backwards compat change, in next major version remove the need for this and just have a single interface
    /// </remarks>
    public interface IUmbracoContentIndex2 : IUmbracoContentIndex
    {
        bool PublishedValuesOnly { get; }
    }

    /// <summary>
    /// Marker interface for indexes of Umbraco content
    /// </summary>
    public interface IUmbracoContentIndex : IIndex
    {

    }
}
