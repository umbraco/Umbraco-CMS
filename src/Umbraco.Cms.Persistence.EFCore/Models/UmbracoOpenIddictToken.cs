using System;
using System.Collections.Generic;
using OpenIddict.EntityFrameworkCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictToken : OpenIddictEntityFrameworkCoreToken<string, UmbracoOpenIddictApplication, UmbracoOpenIddictAuthorization>
{
    public string? ApplicationId { get; set; }

    public string? AuthorizationId { get; set; }
}
