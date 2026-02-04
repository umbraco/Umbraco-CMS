using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that handles member access checks for the Delivery API.
/// </summary>
public interface IRequestMemberAccessService
{
    /// <summary>
    ///     Determines whether the current member has access to the specified content.
    /// </summary>
    /// <param name="content">The published content to check access for.</param>
    /// <returns>The access status indicating whether the member can access the content.</returns>
    Task<PublicAccessStatus> MemberHasAccessToAsync(IPublishedContent content);

    /// <summary>
    ///     Gets the protected access information for the current member.
    /// </summary>
    /// <returns>The protected access information.</returns>
    Task<ProtectedAccess> MemberAccessAsync();
}
