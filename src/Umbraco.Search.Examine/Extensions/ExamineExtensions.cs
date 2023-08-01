using System.Globalization;
using Examine;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Search;
using Umbraco.Search.Enums;

namespace Umbraco.Search.Examine.Extensions;

/// <summary>
///     Extension methods for Examine.
/// </summary>
public static class ExamineExtensions
{
    public static IEnumerable<IUmbracoSearchResult> ToUmbracoResults(this ISearchResults results)
    {
        var resultsTarget = results.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values));
        return resultsTarget;
    }

    public static FieldDefinitionCollection toExamineFieldDefinitionCollection(
        this UmbracoFieldDefinitionCollection collection)
    {
        return new FieldDefinitionCollection(collection
            .Select(x => new global::Examine.FieldDefinition(x.Name, x.Type.ToExamineType())).ToArray());
    }

    public static string ToExamineType(this UmbracoFieldType fieldType)
    {
        return fieldType switch
        {
            UmbracoFieldType.Integer => FieldDefinitionTypes.Integer,
            UmbracoFieldType.Raw => FieldDefinitionTypes.Raw,
            UmbracoFieldType.DateTime => FieldDefinitionTypes.DateTime,
            UmbracoFieldType.EmailAddress => FieldDefinitionTypes.EmailAddress,
            UmbracoFieldType.InvariantCultureIgnoreCase => FieldDefinitionTypes.InvariantCultureIgnoreCase,
            UmbracoFieldType.Double => FieldDefinitionTypes.Double,
            UmbracoFieldType.Float => FieldDefinitionTypes.Float,
            UmbracoFieldType.Long => FieldDefinitionTypes.Long,
            UmbracoFieldType.DateYear => FieldDefinitionTypes.DateYear,
            UmbracoFieldType.DateMonth => FieldDefinitionTypes.DateMonth,
            UmbracoFieldType.DateDay => FieldDefinitionTypes.DateDay,
            UmbracoFieldType.DateHour => FieldDefinitionTypes.DateDay,
            UmbracoFieldType.FullText => FieldDefinitionTypes.DateDay,
            UmbracoFieldType.FullTextSortable => FieldDefinitionTypes.DateDay,
            _ => FieldDefinitionTypes.Raw,
        };
    }

    /// <summary>
    ///     Creates an <see cref="IEnumerable{PublishedSearchResult}" /> containing all content from the
    ///     <paramref name="cache" />.
    /// </summary>
    /// <param name="results">The search results.</param>
    /// <param name="cache">The cache to fetch the content from.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{PublishedSearchResult}" /> containing all content.
    /// </returns>
    /// <exception cref="ArgumentNullException">cache</exception>
    /// <remarks>
    ///     Search results are skipped if it can't be fetched from the <paramref name="cache" /> by its integer id.
    /// </remarks>
    public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(
        this IEnumerable<ISearchResult> results,
        IPublishedCache? cache)
    {
        if (cache == null)
        {
            throw new ArgumentNullException(nameof(cache));
        }

        var publishedSearchResults = new List<PublishedSearchResult>();

        foreach (ISearchResult result in results)
        {
            if (int.TryParse(result.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var contentId))
            {
                IPublishedContent? content = cache.GetById(contentId);
                if (content is not null)
                {
                    publishedSearchResults.Add(new PublishedSearchResult(content, result.Score));
                }
            }
        }

        return publishedSearchResults;
    }

    /// <summary>
    ///     Creates an <see cref="IEnumerable{PublishedSearchResult}" /> containing all content, media or members from the
    ///     <paramref name="snapshot" />.
    /// </summary>
    /// <param name="results">The search results.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>
    ///     An <see cref="IEnumerable{PublishedSearchResult}" /> containing all content, media or members.
    /// </returns>
    /// <exception cref="ArgumentNullException">snapshot</exception>
    /// <remarks>
    ///     Search results are skipped if it can't be fetched from the respective cache by its integer id.
    /// </remarks>
    public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(
        this IEnumerable<ISearchResult> results,
        IPublishedSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        var publishedSearchResults = new List<PublishedSearchResult>();

        foreach (ISearchResult result in results)
        {
            if (int.TryParse(result.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var contentId) &&
                result.Values.TryGetValue(ExamineFieldNames.CategoryFieldName, out var indexType))
            {
                IPublishedContent? content;
                content = indexType switch
                {
                    IndexTypes.Content => snapshot.Content?.GetById(contentId),
                    IndexTypes.Media => snapshot.Media?.GetById(contentId),
                    IndexTypes.Member => throw new NotSupportedException(
                        "Cannot convert search results to member instances"),
                    _ => null
                };
                if (content == null)
                {
                    continue;
                }

                publishedSearchResults.Add(new PublishedSearchResult(content, result.Score));
            }
        }

        return publishedSearchResults;
    }
}
