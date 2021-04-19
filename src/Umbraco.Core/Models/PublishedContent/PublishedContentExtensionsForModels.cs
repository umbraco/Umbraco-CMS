using System;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides strongly typed published content models services.
    /// </summary>
    public static class PublishedContentExtensionsForModels
    {
        /// <summary>
        /// Creates a strongly typed published content model for an internal published content.
        /// </summary>
        /// <param name="content">The internal published content.</param>
        /// <returns>The strongly typed published content model.</returns>
        public static TPublishedContent CreateModel<TPublishedContent>(this TPublishedContent content, IPublishedModelFactory publishedModelFactory)
            where TPublishedContent : class, IPublishedContent
        {
            if (publishedModelFactory == null) throw new ArgumentNullException(nameof(publishedModelFactory));
            if (content == null)
                return null;

            // get model
            // if factory returns nothing, throw
            var model = publishedModelFactory.CreateModel(content);
            if (model == null)
                throw new InvalidOperationException("Factory returned null.");

            // if factory returns a different type, throw
            if (!(model is TPublishedContent publishedContent))
                throw new InvalidOperationException($"Factory returned model of type {model.GetType().FullName} which does not implement IPublishedContent.");

            return publishedContent;
        }
    }
}
