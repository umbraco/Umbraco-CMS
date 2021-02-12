using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
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
