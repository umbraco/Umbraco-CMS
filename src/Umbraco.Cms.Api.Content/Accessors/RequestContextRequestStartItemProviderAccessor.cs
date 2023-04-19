﻿using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Content.Accessors;

public class RequestContextRequestStartItemProviderAccessor : RequestContextServiceAccessorBase<IRequestStartItemProvider>, IRequestStartItemProviderAccessor
{
    public RequestContextRequestStartItemProviderAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
