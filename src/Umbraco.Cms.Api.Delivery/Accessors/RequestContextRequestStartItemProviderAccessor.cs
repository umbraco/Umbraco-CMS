using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Accessors;

internal sealed class RequestContextRequestStartItemProviderAccessor : RequestContextServiceAccessorBase<IRequestStartItemProvider>, IRequestStartItemProviderAccessor
{
    public RequestContextRequestStartItemProviderAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
