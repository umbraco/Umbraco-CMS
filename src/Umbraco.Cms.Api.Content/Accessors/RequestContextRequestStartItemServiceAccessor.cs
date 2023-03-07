using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Accessors;

public class RequestContextRequestStartItemServiceAccessor : RequestContextServiceAccessorBase<IRequestStartItemService>, IRequestStartItemServiceAccessor
{
    public RequestContextRequestStartItemServiceAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
