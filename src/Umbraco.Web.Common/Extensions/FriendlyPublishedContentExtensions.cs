using System;
using System.Collections.Generic;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Extensions
{
    public static class FriendlyPublishedContentExtensions
    {
        private static IVariationContextAccessor VariationContextAccessor { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>();

        private static IPublishedModelFactory PublishedModelFactory { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IPublishedModelFactory>();

        private static IPublishedUrlProvider PublishedUrlProvider { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlProvider>();

        private static IUserService UserService { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IUserService>();

        private static IUmbracoContextAccessor UmbracoContextAccessor { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>();

        private static ISiteDomainHelper SiteDomainHelper { get; } =
            StaticServiceProvider.Instance.GetRequiredService<ISiteDomainHelper>();

        private static IExamineManager ExamineManager { get; } =
            StaticServiceProvider.Instance.GetRequiredService<IExamineManager>();


        /// <summary>
        /// Creates a strongly typed published content model for an internal published content.
        /// </summary>
        /// <param name="content">The internal published content.</param>
        /// <returns>The strongly typed published content model.</returns>
        public static IPublishedContent CreateModel(
            this IPublishedContent content)
            => content.CreateModel(PublishedModelFactory);

        /// <summary>
        /// Gets the name of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the name for. If null is used the current culture is used (Default is null).</param>
        public static string Name(
            this IPublishedContent content,
            string culture = null)
            => content.Name(VariationContextAccessor, culture);

        /// <summary>
        /// Gets the URL segment of the content item.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The specific culture to get the URL segment for. If null is used the current culture is used (Default is null).</param>
        public static string UrlSegment(
            this IPublishedContent content,
            string culture = null)
            => content.UrlSegment(VariationContextAccessor, culture);

        /// <summary>
        /// Gets the url for a media.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="culture">The culture (use current culture by default).</param>
        /// <param name="mode">The url mode (use site configuration by default).</param>
        /// <param name="propertyAlias">The alias of the property (use 'umbracoFile' by default).</param>
        /// <returns>The url for the media.</returns>
        /// <remarks>
        /// <para>The value of this property is contextual. It depends on the 'current' request uri,
        /// if any. In addition, when the content type is multi-lingual, this is the url for the
        /// specified culture. Otherwise, it is the invariant url.</para>
        /// </remarks>
        public static string MediaUrl(
            this IPublishedContent content,
            string culture = null,
            UrlMode mode = UrlMode.Default,
            string propertyAlias = Constants.Conventions.Media.File)
            => content.MediaUrl(PublishedUrlProvider, culture, mode, propertyAlias);

        /// <summary>
        /// Gets the name of the content item creator.
        /// </summary>
        /// <param name="content">The content item.</param>
        public static string CreatorName(this IPublishedContent content) =>
            content.CreatorName(UserService);

        /// <summary>
        /// Gets the name of the content item writer.
        /// </summary>
        /// <param name="content">The content item.</param>
        public static string WriterName(this IPublishedContent content) =>
            content.WriterName(UserService);

        /// <summary>
        /// Gets the culture assigned to a document by domains, in the context of a current Uri.
        /// </summary>
        /// <param name="content">The document.</param>
        /// <param name="current">An optional current Uri.</param>
        /// <returns>The culture assigned to the document by domains.</returns>
        /// <remarks>
        /// <para>In 1:1 multilingual setup, a document contains several cultures (there is not
        /// one document per culture), and domains, withing the context of a current Uri, assign
        /// a culture to that document.</para>
        /// </remarks>
        public static string GetCultureFromDomains(
            this IPublishedContent content,
            Uri current = null)
            => content.GetCultureFromDomains(UmbracoContextAccessor, SiteDomainHelper, current);


        public static IEnumerable<PublishedSearchResult> SearchDescendants(
            this IPublishedContent content,
            string term,
            string indexName = null)
            => content.SearchDescendants(ExamineManager, UmbracoContextAccessor, term, indexName);


        public static IEnumerable<PublishedSearchResult> SearchChildren(
            this IPublishedContent content,
            string term,
            string indexName = null)
            => content.SearchChildren(ExamineManager, UmbracoContextAccessor, term, indexName);


    }
}
