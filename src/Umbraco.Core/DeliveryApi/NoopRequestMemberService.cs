using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public class NoopRequestMemberService : IRequestMemberService
{
    public Task<bool> MemberHasAccessToAsync(IPublishedContent content) => Task.FromResult(true);

    public Task<ProtectedAccess> MemberAccessAsync() => Task.FromResult(new ProtectedAccess(null, null));
}
