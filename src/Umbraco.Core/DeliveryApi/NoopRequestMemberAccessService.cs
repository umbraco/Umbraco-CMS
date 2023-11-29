using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopRequestMemberAccessService : IRequestMemberAccessService
{
    public Task<PublicAccessStatus> MemberHasAccessToAsync(IPublishedContent content) => Task.FromResult(PublicAccessStatus.AccessAccepted);

    public Task<ProtectedAccess> MemberAccessAsync() => Task.FromResult(ProtectedAccess.None);
}
