using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        internal static void AddCollectionBuilders(this IUmbracoBuilder builder)
        {
            builder.FlagProviders()
                .Append<HasScheduleFlagProvider>()
                .Append<IsProtectedFlagProvider>()
                .Append<HasPendingChangesFlagProvider>()
                .Append<HasCollectionFlagProvider>();
        }

        /// <summary>
        /// Gets the flag providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static FlagProviderCollectionBuilder FlagProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<FlagProviderCollectionBuilder>();

/// <summary>
/// Gets the flag providers collection builder.
/// </summary>
/// <param name="builder">The Umbraco builder instance.</param>
/// <returns>The <see cref="FlagProviderCollectionBuilder"/> instance.</returns>
        [Obsolete("Please use the correctly named FlagProviders. Scheduled for removal in Umbraco 19.")]
        public static FlagProviderCollectionBuilder SignProviders(this IUmbracoBuilder builder)
            => builder.FlagProviders();
    }
}
