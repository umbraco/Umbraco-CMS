using System.Web;
using Examine;
using Examine.Search;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions;

public static class PublishedContentExtensions
{
    #region Variations

    /// <summary>
    ///     Gets the culture assigned to a document by domains, in the context of a current Uri.
    /// </summary>
    /// <param name="content">The document.</param>
    /// <param name="umbracoContextAccessor"></param>
    /// <param name="siteDomainHelper"></param>
    /// <param name="current">An optional current Uri.</param>
    /// <returns>The culture assigned to the document by domains.</returns>
    /// <remarks>
    ///     <para>
    ///         In 1:1 multilingual setup, a document contains several cultures (there is not
    ///         one document per culture), and domains, withing the context of a current Uri, assign
    ///         a culture to that document.
    ///     </para>
    /// </remarks>
    public static string? GetCultureFromDomains(
        this IPublishedContent content,
        IUmbracoContextAccessor umbracoContextAccessor,
        ISiteDomainMapper siteDomainHelper,
        Uri? current = null)
    {
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        return DomainUtilities.GetCultureFromDomains(content.Id, content.Path, current, umbracoContext, siteDomainHelper);
    }

    #endregion

    #region Creator/Writer Names

    /// <summary>
    ///     Gets the name of the content item creator.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService"></param>
    public static string? CreatorName(this IPublishedContent content, IUserService userService) =>
        userService.GetProfileById(content.CreatorId)?.Name;

    /// <summary>
    ///     Gets the name of the content item writer.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="userService"></param>
    public static string? WriterName(this IPublishedContent content, IUserService userService) =>
        userService.GetProfileById(content.WriterId)?.Name;

    #endregion

    #region Search

    public static IEnumerable<PublishedSearchResult> SearchDescendants(
        this IPublishedContent content,
        IExamineManager examineManager,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term,
        string? indexName = null)
    {
        indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
        if (!examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found with name " + indexName);
        }

        // var t = term.Escape().Value;
        // var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;
        IBooleanOperation? query = index.Searcher.CreateQuery()
            .Field(UmbracoExamineFieldNames.IndexPathFieldName, (content.Path + ",").MultipleCharacterWildcard())
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    public static IEnumerable<PublishedSearchResult> SearchChildren(
        this IPublishedContent content,
        IExamineManager examineManager,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term,
        string? indexName = null)
    {
        indexName = string.IsNullOrEmpty(indexName) ? Constants.UmbracoIndexes.ExternalIndexName : indexName;
        if (!examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found with name " + indexName);
        }

        // var t = term.Escape().Value;
        // var luceneQuery = "+parentID:" + content.Id + " +" + t;
        IBooleanOperation? query = index.Searcher.CreateQuery()
            .Field("parentID", content.Id)
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    #endregion

    #region IsSomething: equality

    /// <summary>
    ///     If the specified <paramref name="content" /> is equal to <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue) =>
        content.IsEqual(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is equal to <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsEqual(
        this IPublishedContent content,
        IPublishedContent other,
        string valueIfTrue,
        string valueIfFalse) =>
        new HtmlString(HttpUtility.HtmlEncode(content.IsEqual(other) ? valueIfTrue : valueIfFalse));

    /// <summary>
    ///     If the specified <paramref name="content" /> is not equal to <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent
        IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue) =>
        content.IsNotEqual(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is not equal to <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsNotEqual(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse) =>
        new HtmlString(HttpUtility.HtmlEncode(content.IsNotEqual(other) ? valueIfTrue : valueIfFalse));

    #endregion

    #region IsSomething: ancestors and descendants

    /// <summary>
    ///     If the specified <paramref name="content" /> is a decendant of <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <see cref="string.Empty" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent
        IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue) =>
        content.IsDescendant(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is a decendant of <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsDescendant(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse) =>
        new HtmlString(HttpUtility.HtmlEncode(content.IsDescendant(other) ? valueIfTrue : valueIfFalse));

    public static IHtmlContent IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue) => content.IsDescendantOrSelf(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is a decendant of <paramref name="other" /> or are the same, the HTML
    ///     encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsDescendantOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse) =>
        new HtmlString(HttpUtility.HtmlEncode(content.IsDescendantOrSelf(other) ? valueIfTrue : valueIfFalse));

    public static IHtmlContent
        IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue) =>
        content.IsAncestor(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is an ancestor of <paramref name="other" />, the HTML encoded
    ///     <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsAncestor(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse) =>
        new HtmlString(HttpUtility.HtmlEncode(content.IsAncestor(other) ? valueIfTrue : valueIfFalse));

    public static IHtmlContent IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue)
        => content.IsAncestorOrSelf(other, valueIfTrue, string.Empty);

    /// <summary>
    ///     If the specified <paramref name="content" /> is an ancestor of <paramref name="other" /> or are the same, the HTML
    ///     encoded <paramref name="valueIfTrue" /> will be returned; otherwise, <paramref name="valueIfFalse" />.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="other">The other content.</param>
    /// <param name="valueIfTrue">The value if <c>true</c>.</param>
    /// <param name="valueIfFalse">The value if <c>false</c>.</param>
    /// <returns>
    ///     The HTML encoded value.
    /// </returns>
    public static IHtmlContent IsAncestorOrSelf(this IPublishedContent content, IPublishedContent other, string valueIfTrue, string valueIfFalse)
        => new HtmlString(HttpUtility.HtmlEncode(content.IsAncestorOrSelf(other) ? valueIfTrue : valueIfFalse));

    #endregion
}
