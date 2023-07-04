using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <inheritdoc />
/// <summary>
///     Marker interface for a <see cref="T:Examine.ValueSet" /> builder for supporting unpublished content
/// </summary>
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]
public interface IContentValueSetBuilder : IValueSetBuilder<IContent>
{
}
