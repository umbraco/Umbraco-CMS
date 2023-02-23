using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Accessors;

public class RequestContextRequestStartNodeServiceAccessor : RequestContextServiceAccessorBase<IRequestStartNodeService>, IRequestStartNodeServiceAccessor
{
    public RequestContextRequestStartNodeServiceAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
