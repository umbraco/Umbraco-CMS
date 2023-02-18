using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

/// <summary>
///     Marker interface for a <see cref="ValueSet" /> builder for only published content
/// </summary>
public interface IPublishedContentValueSetBuilder : IValueSetBuilder<IContent>
{
}
