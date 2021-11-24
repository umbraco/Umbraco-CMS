using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.DependencyInjection
{
    /// <summary>
    /// Contains extensions methods for <see cref="IUmbracoBuilder"/> used for registering content apps.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Register a content app.
        /// </summary>
        /// <typeparam name="T">The type of the content app.</typeparam>
        /// <param name="builder">The builder.</param>
        public static IUmbracoBuilder AddContentApp<T>(this IUmbracoBuilder builder)
            where T : class, IContentAppFactory
        {
            builder.ContentApps().Append<T>();
            return builder;
        }
    }
}
