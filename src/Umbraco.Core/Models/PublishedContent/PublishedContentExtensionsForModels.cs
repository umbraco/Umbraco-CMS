namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides strongly typed published content models services.
    /// </summary>
    internal static class PublishedContentExtensionsForModels
    {
        /// <summary>
        /// Creates a strongly typed published content model for an internal published content.
        /// </summary>
        /// <param name="content">The internal published content.</param>
        /// <returns>The strongly typed published content model.</returns>
        public static IPublishedContent CreateModel(this IPublishedContent content)
        {
            return PublishedContentModelFactoryResolver.Current.HasValue
                ? PublishedContentModelFactoryResolver.Current.Factory.CreateModel(content)
                : content;
        }
    }
}
