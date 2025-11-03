using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Web.Common.Accessors;

public sealed class RequestContextOutputExpansionStrategyAccessor : RequestContextServiceAccessorBase<IOutputExpansionStrategy>, IOutputExpansionStrategyAccessor
{
    public RequestContextOutputExpansionStrategyAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
