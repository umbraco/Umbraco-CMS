using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides validation functionality for member content.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IContentValidationServiceBase{TContentType}"/> to provide member-specific validation capabilities.
/// </remarks>
internal interface IMemberValidationService : IContentValidationServiceBase<IMemberType>
{
}
