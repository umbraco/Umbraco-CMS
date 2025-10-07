using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds the necessary components to support isolated caches in a load balanced environment.
    /// </summary>
    /// <remarks>This is require to load balance back office.</remarks>
    public static IUmbracoBuilder LoadBalanceIsolatedCaches(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IRepositoryCacheVersionService, RepositoryCacheVersionService>();
        return builder;
    }
}
