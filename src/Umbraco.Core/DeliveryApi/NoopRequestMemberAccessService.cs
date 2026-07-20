using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestMemberAccessService"/> that always grants access.
/// </summary>
public sealed class NoopRequestMemberAccessService : IRequestMemberAccessService
{
    /// <inheritdoc />
    public Task<PublicAccessStatus> MemberHasAccessToAsync(IPublishedContent content) => Task.FromResult(PublicAccessStatus.AccessAccepted);

    /// <inheritdoc />
    public Task<ProtectedAccess> MemberAccessAsync() => Task.FromResult(ProtectedAccess.None);
}
