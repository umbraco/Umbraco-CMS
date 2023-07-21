using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestMemberService
{
    Task<bool> MemberHasAccessToAsync(IPublishedContent content);

    Task<ProtectedAccess> MemberAccessAsync();
}
