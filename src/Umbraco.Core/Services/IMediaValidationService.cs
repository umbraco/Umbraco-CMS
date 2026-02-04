using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides validation functionality for media content.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IContentValidationServiceBase{TContentType}"/> to provide media-specific validation capabilities.
/// </remarks>
internal interface IMediaValidationService : IContentValidationServiceBase<IMediaType>
{
}
