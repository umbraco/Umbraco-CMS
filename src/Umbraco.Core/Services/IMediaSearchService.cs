using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides search functionality for media items.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IContentSearchService{TContent}"/> to provide media-specific search capabilities.
/// </remarks>
public interface IMediaSearchService : IContentSearchService<IMedia>
{
}
