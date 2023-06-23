using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.OpenIddict;

public static class BackOfficeAuthBuilderOpenIddictExtensions
{
    public static IUmbracoBuilder AddUmbracoOpenIddictDbContext(this IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoEFCoreContext<UmbracoOpenIddictDbContext>((options, connectionString, providerName) =>
        {
            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict();
        });

        return builder;
    }
}
