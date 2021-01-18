using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Manifest;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.Trees;
using Umbraco.Web.Media.EmbedProviders;

namespace Umbraco.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Gets the mappers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static MapperCollectionBuilder Mappers(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<MapperCollectionBuilder>();
    }
}
