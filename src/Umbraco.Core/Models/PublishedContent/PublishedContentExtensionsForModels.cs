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

            // in order to provide a nice, "fluent" experience, this extension method
            // needs to access Current, which is not always initialized in tests - not
            // very elegant, but works
            if (!Current.HasFactory) return content;

            // get model
            // if factory returns nothing, throw
            var model = Current.PublishedModelFactory.CreateModel(content);
            if (model == null)
                throw new Exception("Factory returned null.");

            // if factory returns a different type, throw
            if (!(model is IPublishedContent publishedContent))
                throw new Exception($"Factory returned model of type {model.GetType().FullName} which does not implement IPublishedContent.");

            return publishedContent;
        }
    }
}
