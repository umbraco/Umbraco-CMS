using OpenIddict.EntityFrameworkCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Models;

public class UmbracoOpenIddictApplication : OpenIddictEntityFrameworkCoreApplication<string, UmbracoOpenIddictAuthorization, UmbracoOpenIddictToken>
{
}
