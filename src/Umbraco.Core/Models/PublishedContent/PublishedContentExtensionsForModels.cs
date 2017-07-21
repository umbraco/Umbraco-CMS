using System;
using Umbraco.Core.Composing;

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
            if (content == null)
                return null;

            // get model
            // if factory returns nothing, throw
            var model = Current.PublishedContentModelFactory.CreateModel(content);
            if (model == null)
                throw new Exception("Factory returned null.");

            // if factory returns a different type, throw
            var publishedContent = model as IPublishedContent;
            if (publishedContent == null)
                throw new Exception($"Factory returned model of type {model.GetType().FullName} which does not implement IPublishedContent.");

            return publishedContent;
        }
    }
}
