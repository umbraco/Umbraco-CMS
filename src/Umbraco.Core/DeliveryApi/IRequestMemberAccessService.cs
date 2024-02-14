using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestMemberAccessService
{
    Task<PublicAccessStatus> MemberHasAccessToAsync(IPublishedContent content);

    Task<ProtectedAccess> MemberAccessAsync();
}
