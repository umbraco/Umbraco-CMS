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
                .Append<HasDocumentScheduleFlagProvider>()
                .Append<HasElementScheduleFlagProvider>()
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
    }
}
