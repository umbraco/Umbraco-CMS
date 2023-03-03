using Examine;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.Examine.ValueSetBuilders;

/// <summary>
///     Marker interface for a <see cref="ValueSet" /> builder for only published content
/// </summary>
public interface IPublishedContentValueSetBuilder : IValueSetBuilder<IContent>
{
}
