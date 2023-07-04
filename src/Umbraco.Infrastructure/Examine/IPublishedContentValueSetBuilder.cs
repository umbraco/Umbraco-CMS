using Examine;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Marker interface for a <see cref="ValueSet" /> builder for only published content
/// </summary>
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public interface IPublishedContentValueSetBuilder : IValueSetBuilder<IContent>
{
}
