using Examine;
using Umbraco.Core.Models;

namespace Umbraco.Examine
{
    /// <inheritdoc />
    /// <summary>
    /// Marker interface for a <see cref="T:Examine.ValueSet" /> builder for supporting unpublished content
    /// </summary>
    public interface IContentValueSetBuilder : IValueSetBuilder<IContent>
    {
    }
}
