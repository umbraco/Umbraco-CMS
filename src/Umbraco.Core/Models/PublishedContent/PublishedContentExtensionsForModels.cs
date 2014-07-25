using System;

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

            if (PublishedContentModelFactoryResolver.Current.HasValue == false)
                return content;

            // get model
            // if factory returns nothing, throw
            // if factory just returns what it got, return
            var model = PublishedContentModelFactoryResolver.Current.Factory.CreateModel(content);
            if (model == null)
                throw new Exception("IPublishedContentFactory returned null.");
            if (ReferenceEquals(model, content))
                return content;

            // at the moment, other parts of our code assume that all models will
            // somehow implement IPublishedContentExtended and not just be IPublishedContent,
            // so we'd better check this here to fail as soon as we can.
            //
            // see also PublishedContentExtended.Extend

            var extended = model as IPublishedContentExtended;
            if (extended == null)
                throw new Exception("IPublishedContentFactory created an object that does not implement IPublishedContentModelExtended.");

            return model;
        }
    }
}
