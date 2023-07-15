using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Extensions;

public static class DbContextServiceExtensions
{
    public static IUmbracoBuilder AddUmbracoEFCoreDbContext(this IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoEFCoreContext<UmbracoInternalDbContext>((options, connectionString, providerName) =>
        {
            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict<UmbracoOpenIddictApplication, UmbracoOpenIddictAuthorization, UmbracoOpenIddictScope, UmbracoOpenIddictToken, string>();
        });

        return builder;
    }
}
