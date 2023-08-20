using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.ValueSet.ValueSetBuilders;

/// <inheritdoc />
/// <summary>
///     Marker interface for a <see cref="T:Examine.ValueSet" /> builder for supporting unpublished content
/// </summary>
public interface IContentValueSetBuilder : IValueSetBuilder<IContentBase>
{
}
