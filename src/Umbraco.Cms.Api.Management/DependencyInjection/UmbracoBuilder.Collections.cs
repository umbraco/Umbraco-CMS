using Umbraco.Cms.Api.Management.Services.Signs;
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
            builder.SignProviders()
                .Append<HasScheduleSignProvider>()
                .Append<IsProtectedSignProvider>()
                .Append<HasPendingChangesSignProvider>()
                .Append<HasCollectionSignProvider>();
        }

        /// <summary>
        /// Gets the sign providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static SignProviderCollectionBuilder SignProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<SignProviderCollectionBuilder>();
    }
}
