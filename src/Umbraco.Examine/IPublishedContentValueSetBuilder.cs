using Examine;
using Umbraco.Core.Models;

namespace Umbraco.Examine
{
    /// <summary>
    /// Marker interface for a <see cref="ValueSet"/> builder for only published content
    /// </summary>
    public interface IPublishedContentValueSetBuilder : IValueSetBuilder<IContent>
    {
    }
}