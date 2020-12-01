using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedContent</c>.
    /// </summary>
    public static class PublishedContentExtensions
    {
        // see notes in PublishedElementExtensions
        // (yes, this is not pretty, but works for now)
        //
        private static IPublishedValueFallback PublishedValueFallback => Current.PublishedValueFallback;
        private static IPublishedSnapshot PublishedSnapshot => Current.PublishedSnapshot;
        private static IUmbracoContext UmbracoContext => Current.UmbracoContext;
        private static ISiteDomainHelper SiteDomainHelper => Current.Factory.GetRequiredService<ISiteDomainHelper>();
        private static IVariationContextAccessor VariationContextAccessor => Current.VariationContextAccessor;
        private static IExamineManager ExamineManager => Current.Factory.GetRequiredService<IExamineManager>();
        private static IUserService UserService => Current.Services.UserService;


        #region Creator/Writer Names

        public static string CreatorName(this IPublishedContent content, IUserService userService)
        {
            return userService.GetProfileById(content.CreatorId)?.Name;
        }

        public static string WriterName(this IPublishedContent content, IUserService userService)
        {
            return userService.GetProfileById(content.WriterId)?.Name;
        }

        public static string CreatorName(this IPublishedContent content)
        {
            return content.GetCreatorName(UserService);
        }

        public static string WriterName(this IPublishedContent content)
        {
            return content.GetWriterName(UserService);
        }

        #endregion

        #region Template

        /// <summary>
        /// Returns the current template Alias
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Empty string if none is set.</returns>
        public static string GetTemplateAlias(this IPublishedContent content)
        {
            return content.GetTemplateAlias(Current.Services.FileService);
        }

        public static bool IsAllowedTemplate(this IPublishedContent content, int templateId)
        {
            return content.IsAllowedTemplate(
                Current.Services.ContentTypeService,
                /*Current.Configs.WebRouting().DisableAlternativeTemplates,
                Current.Configs.WebRouting().ValidateAlternativeTemplates,
                TODO get values from config*/
                 false, false,
                templateId);
        }

        public static bool IsAllowedTemplate(this IPublishedContent content, string templateAlias)
        {
            return content.IsAllowedTemplate(
                Current.Services.FileService,
                Current.Services.ContentTypeService,
                /*Current.Configs.WebRouting().DisableAlternativeTemplates,
                    Current.Configs.WebRouting().ValidateAlternativeTemplates,
                    TODO get values from config*/
                false, false,
                templateAlias);
        }

        #endregion

        #region HasValue, Value, Value<T>

        /// <summary>
        /// Gets a value indicating whether the content has a value for a property identified by its alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The variation language.</param>
        /// <param name="segment">The variation segment.</param>
        /// <param name="fallback">Optional fallback strategy.</param>
        /// <returns>A value indicating whether the content has a value for the property identified by the alias.</returns>
        /// <remarks>Returns true if HasValue is true, or a fallback strategy can provide a value.</remarks>
        public static bool HasValue(this IPublishedContent content, string alias, string culture = null, string segment = null, Fallback fallback = default)
        {
            return content.HasValue(PublishedValueFallback, alias, culture, segment, fallback);
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias, if it exists, otherwise a default value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The variation language.</param>
        /// <param name="segment">The variation segment.</param>
        /// <param name="fallback">Optional fallback strategy.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, if it exists, otherwise a default value.</returns>
        public static object Value(this IPublishedContent content, string alias, string culture = null, string segment = null, Fallback fallback = default, object defaultValue = default)
        {
            return content.Value(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias, converted to a specified type.
        /// </summary>
        /// <typeparam name="T">The target property type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="culture">The variation language.</param>
        /// <param name="segment">The variation segment.</param>
        /// <param name="fallback">Optional fallback strategy.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value of the content's property identified by the alias, converted to the specified type.</returns>
        public static T Value<T>(this IPublishedContent content, string alias, string culture = null, string segment = null, Fallback fallback = default, T defaultValue = default)
        {
            return content.Value<T>(PublishedValueFallback, alias, culture, segment, fallback, defaultValue);
        }

        #endregion

        #region Variations

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
        public static string GetCultureFromDomains(this IPublishedContent content, Uri current = null)
        {
            var umbracoContext = UmbracoContext;

            if (umbracoContext == null)
                throw new InvalidOperationException("A current UmbracoContext is required.");

            return DomainUtilities.GetCultureFromDomains(content.Id, content.Path, current, umbracoContext, SiteDomainHelper);
        }

        #endregion

        #region Search

        public static IEnumerable<PublishedSearchResult> SearchDescendants(this IPublishedContent content, string term, string indexName = null)
        {
            // TODO: inject examine manager

            indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
            if (!ExamineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException("No index found with name " + indexName);

            var searcher = index.GetSearcher();

            //var t = term.Escape().Value;
            //var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;

            var query = searcher.CreateQuery()
                .Field(UmbracoExamineFieldNames.IndexPathFieldName, (content.Path + ",").MultipleCharacterWildcard())
                .And()
                .ManagedQuery(term);

            return query.Execute().ToPublishedSearchResults(Current.UmbracoContext.Content);
        }

        public static IEnumerable<PublishedSearchResult> SearchChildren(this IPublishedContent content, string term, string indexName = null)
        {
            // TODO: inject examine manager

            indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
            if (!ExamineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException("No index found with name " + indexName);

            var searcher = index.GetSearcher();

            //var t = term.Escape().Value;
            //var luceneQuery = "+parentID:" + content.Id + " +" + t;

            var query = searcher.CreateQuery()
                .Field("parentID", content.Id)
                .And()
                .ManagedQuery(term);

            return query.Execute().ToPublishedSearchResults(Current.UmbracoContext.Content);
        }

        #endregion

         #region IsSomething: equality

        public static bool IsEqual(this IPublishedContent content, IPublishedContent other)
        {
            return content.Id == other.Id;
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is equal to <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsEqual(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is equal to <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsEqual(other) ? valueIfTrue : valueIfFalse));
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is not equal to <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static bool IsNotEqual(this IPublishedContent content, IPublishedContent other)
        {
            return content.IsEqual(other) == false;
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is not equal to <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsNotEqual(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is not equal to <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsNotEqual(other) ? valueIfTrue : valueIfFalse));
        }

        #endregion

        #region IsSomething: ancestors and descendants


        /// <summary>
        /// If the specified <paramref name="content" /> is a decendant of <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsDescendant(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is a decendant of <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsDescendant(other) ? valueIfTrue : valueIfFalse));
        }

        public static HtmlString IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsDescendantOrSelf(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is a decendant of <paramref name="other" /> or are the same, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsDescendantOrSelf(other) ? valueIfTrue : valueIfFalse));
        }


        public static HtmlString IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsAncestor(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is an ancestor of <paramref name="other" />, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsAncestor(other) ? valueIfTrue : valueIfFalse));
        }

        public static HtmlString IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        {
            return content.IsAncestorOrSelf(other, valueIfTrue, string.Empty);
        }

        /// <summary>
        /// If the specified <paramref name="content" /> is an ancestor of <paramref name="other" /> or are the same, the HTML encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="other">The other content.</param>
        /// <param name="valueIfTrue">The value if <c>true</c>.</param>
        /// <param name="valueIfFalse">The value if <c>false</c>.</param>
        /// <returns>
        /// The HTML encoded value.
        /// </returns>
        // TODO: This method should be removed or moved to an extension method on HtmlHelper.
        public static HtmlString IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content.IsAncestorOrSelf(other) ? valueIfTrue : valueIfFalse));
        }

        #endregion

        #region Axes: descendants, descendants-or-self

        /// <summary>
        /// Returns all DescendantsOrSelf of all content referenced
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <param name="docTypeAlias"></param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with <see cref="UmbracoHelper.ContentAtRoot"/> or similar.
        /// </remarks>
        public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IEnumerable<IPublishedContent> parentNodes, string docTypeAlias, string culture = null)
        {
            return parentNodes.DescendantsOrSelfOfType(VariationContextAccessor, docTypeAlias, culture);
        }

        /// <summary>
        /// Returns all DescendantsOrSelf of all content referenced
        /// </summary>
        /// <param name="parentNodes"></param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns></returns>
        /// <remarks>
        /// This can be useful in order to return all nodes in an entire site by a type when combined with <see cref="UmbracoHelper.ContentAtRoot"/> or similar.
        /// </remarks>
        public static IEnumerable<T> DescendantsOrSelf<T>(this IEnumerable<IPublishedContent> parentNodes, string culture = null)
            where T : class, IPublishedContent
        {
            return parentNodes.DescendantsOrSelf<T>(VariationContextAccessor, culture);
        }

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, string culture = null)
        {
            return content.Descendants(VariationContextAccessor, culture);
        }

        public static IEnumerable<IPublishedContent> Descendants(this IPublishedContent content, int level, string culture = null)
        {
            return content.Descendants(VariationContextAccessor, level, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.DescendantsOfType(VariationContextAccessor, contentTypeAlias, culture);
        }

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendants<T>(VariationContextAccessor, culture);
        }

        public static IEnumerable<T> Descendants<T>(this IPublishedContent content, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendants<T>(VariationContextAccessor, level, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, string culture = null)
        {
            return content.DescendantsOrSelf(VariationContextAccessor, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelf(this IPublishedContent content, int level, string culture = null)
        {
            return content.DescendantsOrSelf(VariationContextAccessor, level, culture);
        }

        public static IEnumerable<IPublishedContent> DescendantsOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.DescendantsOrSelfOfType(VariationContextAccessor, contentTypeAlias, culture);
        }

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf<T>(VariationContextAccessor, culture);
        }

        public static IEnumerable<T> DescendantsOrSelf<T>(this IPublishedContent content, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantsOrSelf<T>(VariationContextAccessor, level, culture);
        }

        public static IPublishedContent Descendant(this IPublishedContent content, string culture = null)
        {
            return content.Descendant(VariationContextAccessor, culture);
        }

        public static IPublishedContent Descendant(this IPublishedContent content, int level, string culture = null)
        {
            return content.Descendant(VariationContextAccessor, level, culture);
        }

        public static IPublishedContent DescendantOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.DescendantOfType(VariationContextAccessor, contentTypeAlias, culture);
        }

        public static T Descendant<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendant<T>(VariationContextAccessor, culture);
        }

        public static T Descendant<T>(this IPublishedContent content, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Descendant<T>(VariationContextAccessor, level, culture);
        }

        public static IPublishedContent DescendantOrSelf(this IPublishedContent content, int level, string culture = null)
        {
            return content.DescendantOrSelf(VariationContextAccessor, level, culture);
        }

        public static IPublishedContent DescendantOrSelfOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.DescendantOrSelfOfType(VariationContextAccessor, contentTypeAlias, culture);
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantOrSelf<T>(VariationContextAccessor, culture);
        }

        public static T DescendantOrSelf<T>(this IPublishedContent content, int level, string culture = null)
            where T : class, IPublishedContent
        {
            return content.DescendantOrSelf<T>(VariationContextAccessor, level, culture);
        }

        #endregion

        #region Axes: children

        /// <summary>
        /// Gets the children of the content, filtered by a predicate.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of the content, filtered by the predicate.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Children(this IPublishedContent content, Func<IPublishedContent, bool> predicate, string culture = null)
        {
            return content.Children(VariationContextAccessor, culture).Where(predicate);
        }

        /// <summary>
        /// Gets the children of the content, of any of the specified types.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The children of the content, of any of the specified types.</returns>
        public static IEnumerable<IPublishedContent> ChildrenOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.Children(VariationContextAccessor, x => x.ContentType.Alias.InvariantEquals(contentTypeAlias), culture);
        }

        /// <summary>
        /// Gets the children of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of content, of the given content type.</returns>
        /// <remarks>
        /// <para>Children are sorted by their sortOrder.</para>
        /// </remarks>
        public static IEnumerable<T> Children<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children(VariationContextAccessor, culture).OfType<T>();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, string culture = null)
        {
            return content.Children(VariationContextAccessor, culture).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first child of the content, of a given content type.
        /// </summary>
        public static IPublishedContent FirstChildOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.ChildrenOfType(VariationContextAccessor, contentTypeAlias, culture).FirstOrDefault();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, Func<IPublishedContent, bool> predicate, string culture = null)
        {
            return content.Children(VariationContextAccessor, predicate, culture).FirstOrDefault();
        }

        public static IPublishedContent FirstChild(this IPublishedContent content, Guid uniqueId, string culture = null)
        {
            return content.Children(VariationContextAccessor, x => x.Key == uniqueId, culture).FirstOrDefault();
        }

        public static T FirstChild<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children<T>(VariationContextAccessor, culture).FirstOrDefault();
        }

        public static T FirstChild<T>(this IPublishedContent content, Func<T, bool> predicate, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Children<T>(VariationContextAccessor, culture).FirstOrDefault(predicate);
        }

        /// <summary>
        /// Gets the children of the content in a DataTable.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="services">A service context.</param>
        /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of the content.</returns>
        public static DataTable ChildrenAsTable(this IPublishedContent content, ServiceContext services, string contentTypeAliasFilter = "", string culture = null)
        {
            return GenerateDataTable(content, services, contentTypeAliasFilter, culture);
        }

        /// <summary>
        /// Gets the children of the content in a DataTable.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="services">A service context.</param>
        /// <param name="contentTypeAliasFilter">An optional content type alias.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The children of the content.</returns>
        private static DataTable GenerateDataTable(IPublishedContent content, ServiceContext services, string contentTypeAliasFilter = "", string culture = null)
        {
            var firstNode = contentTypeAliasFilter.IsNullOrWhiteSpace()
                                ? content.Children(VariationContextAccessor, culture).Any()
                                    ? content.Children(VariationContextAccessor, culture).ElementAt(0)
                                    : null
                                : content.Children(VariationContextAccessor, culture).FirstOrDefault(x => x.ContentType.Alias.InvariantEquals(contentTypeAliasFilter));
            if (firstNode == null)
                return new DataTable(); //no children found

            //use new utility class to create table so that we don't have to maintain code in many places, just one
            var dt = Core.DataTableExtensions.GenerateDataTable(
                //pass in the alias of the first child node since this is the node type we're rendering headers for
                firstNode.ContentType.Alias,
                //pass in the callback to extract the Dictionary<string, string> of all defined aliases to their names
                alias => GetPropertyAliasesAndNames(services, alias),
                //pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
                () =>
                {
                    //create all row data
                    var tableData = Core.DataTableExtensions.CreateTableData();
                    //loop through each child and create row data for it
                    foreach (var n in content.Children(VariationContextAccessor).OrderBy(x => x.SortOrder))
                    {
                        if (contentTypeAliasFilter.IsNullOrWhiteSpace() == false)
                        {
                            if (n.ContentType.Alias.InvariantEquals(contentTypeAliasFilter) == false)
                                continue; //skip this one, it doesn't match the filter
                        }

                        var standardVals = new Dictionary<string, object>
                            {
                                { "Id", n.Id },
                                { "NodeName", n.Name(VariationContextAccessor) },
                                { "NodeTypeAlias", n.ContentType.Alias },
                                { "CreateDate", n.CreateDate },
                                { "UpdateDate", n.UpdateDate },
                                { "CreatorId", n.CreatorId},
                                { "WriterId", n.WriterId },
                                { "Url", n.Url() }
                            };

                        var userVals = new Dictionary<string, object>();
                        foreach (var p in from IPublishedProperty p in n.Properties where p.GetSourceValue() != null select p)
                        {
                            // probably want the "object value" of the property here...
                            userVals[p.Alias] = p.GetValue();
                        }
                        //add the row data
                        Core.DataTableExtensions.AddRowData(tableData, standardVals, userVals);
                    }
                    return tableData;
                }
                );
            return dt;
        }

        #endregion

        #region Axes: siblings

        /// <summary>
        /// Gets the siblings of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> Siblings(this IPublishedContent content, string culture = null)
        {
            return content.Siblings(PublishedSnapshot, VariationContextAccessor, culture);
        }

        /// <summary>
        /// Gets the siblings of the content, of a given content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The siblings of the content, of the given content type.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<IPublishedContent> SiblingsOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.SiblingsOfType(PublishedSnapshot, VariationContextAccessor, contentTypeAlias, culture);
        }

        /// <summary>
        /// Gets the siblings of the content, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content, of the given content type.</returns>
        /// <remarks>
        ///   <para>Note that in V7 this method also return the content node self.</para>
        /// </remarks>
        public static IEnumerable<T> Siblings<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.Siblings<T>(PublishedSnapshot, VariationContextAccessor, culture);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content including the node itself.</returns>
        public static IEnumerable<IPublishedContent> SiblingsAndSelf(this IPublishedContent content, string culture = null)
        {
            return content.SiblingsAndSelf(PublishedSnapshot, VariationContextAccessor, culture);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position, of a given content type.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
        public static IEnumerable<IPublishedContent> SiblingsAndSelfOfType(this IPublishedContent content, string contentTypeAlias, string culture = null)
        {
            return content.SiblingsAndSelfOfType(PublishedSnapshot, VariationContextAccessor, contentTypeAlias, culture);
        }

        /// <summary>
        /// Gets the siblings of the content including the node itself to indicate the position, of a given content type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="culture">The specific culture to filter for. If null is used the current culture is used. (Default is null)</param>
        /// <returns>The siblings of the content including the node itself, of the given content type.</returns>
        public static IEnumerable<T> SiblingsAndSelf<T>(this IPublishedContent content, string culture = null)
            where T : class, IPublishedContent
        {
            return content.SiblingsAndSelf<T>(PublishedSnapshot, VariationContextAccessor, culture);
        }

        #endregion

        #region PropertyAliasesAndNames

        private static Func<ServiceContext, string, Dictionary<string, string>> _getPropertyAliasesAndNames;

        /// <summary>
        /// This is used only for unit tests to set the delegate to look up aliases/names dictionary of a content type
        /// </summary>
        internal static Func<ServiceContext, string, Dictionary<string, string>> GetPropertyAliasesAndNames
        {
            get => _getPropertyAliasesAndNames ?? GetAliasesAndNames;
            set => _getPropertyAliasesAndNames = value;
        }

        private static Dictionary<string, string> GetAliasesAndNames(ServiceContext services, string alias)
        {
            var type = services.ContentTypeService.Get(alias)
                ?? services.MediaTypeService.Get(alias)
                ?? (IContentTypeBase)services.MemberTypeService.Get(alias);
            var fields = GetAliasesAndNames(type);

            // ensure the standard fields are there
            var stdFields = new Dictionary<string, string>
            {
                {"Id", "Id"},
                {"NodeName", "NodeName"},
                {"NodeTypeAlias", "NodeTypeAlias"},
                {"CreateDate", "CreateDate"},
                {"UpdateDate", "UpdateDate"},
                {"CreatorName", "CreatorName"},
                {"WriterName", "WriterName"},
                {"Url", "Url"}
            };

            foreach (var field in stdFields.Where(x => fields.ContainsKey(x.Key) == false))
            {
                fields[field.Key] = field.Value;
            }

            return fields;
        }

        private static Dictionary<string, string> GetAliasesAndNames(IContentTypeBase contentType)
        {
            return contentType.PropertyTypes.ToDictionary(x => x.Alias, x => x.Name);
        }

        #endregion


        #region Url

        /// <summary>
             /// Gets the URL of the content item.
             /// </summary>
             /// <remarks>
             /// <para>If the content item is a document, then this method returns the URL of the
             /// document. If it is a media, then this methods return the media URL for the
             /// 'umbracoFile' property. Use the MediaUrl() method to get the media URL for other
             /// properties.</para>
             /// <para>The value of this property is contextual. It depends on the 'current' request uri,
             /// if any. In addition, when the content type is multi-lingual, this is the URL for the
             /// specified culture. Otherwise, it is the invariant URL.</para>
             /// </remarks>
             public static string Url(this IPublishedContent content, string culture = null, UrlMode mode = UrlMode.Default)
             {
                 return content.Url(Current.PublishedUrlProvider, culture, mode);
             }


        #endregion
    }
}
