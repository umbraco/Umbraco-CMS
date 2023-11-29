using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Extensions;

public static class BackOfficeAuthBuilderOpenIddictExtensions
{
    [Obsolete("This is no longer used and will be removed in V15. Instead use the overload that specifies the DbContext type.")]
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
