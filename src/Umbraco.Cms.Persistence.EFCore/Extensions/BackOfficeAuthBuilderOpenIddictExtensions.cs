using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Extensions;

public static class BackOfficeAuthBuilderOpenIddictExtensions
{
    public static IUmbracoBuilder AddUmbracoEFCoreDbContext(this IUmbracoBuilder builder)
    {
        builder.Services.AddUmbracoEFCoreContext<UmbracoDbContext>((options, connectionString, providerName) =>
        {
            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict();
        });

        return builder;
    }
}
