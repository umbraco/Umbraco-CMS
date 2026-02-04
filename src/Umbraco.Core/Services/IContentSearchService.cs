using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides search services for content items.
/// </summary>
/// <seealso cref="IContentSearchService{TContent}" />
public interface IContentSearchService : IContentSearchService<IContent>
{
}
