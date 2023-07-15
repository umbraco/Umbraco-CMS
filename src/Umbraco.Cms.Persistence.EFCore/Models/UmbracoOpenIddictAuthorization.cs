using OpenIddict.EntityFrameworkCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictAuthorization : OpenIddictEntityFrameworkCoreAuthorization<string, UmbracoOpenIddictApplication, UmbracoOpenIddictToken>
{
    public string? ApplicationId { get; set; }
}
