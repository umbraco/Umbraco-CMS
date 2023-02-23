﻿using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Accessors;

public class RequestContextOutputExpansionStrategyAccessor : RequestContextServiceAccessorBase<IOutputExpansionStrategy>, IOutputExpansionStrategyAccessor
{
    public RequestContextOutputExpansionStrategyAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
