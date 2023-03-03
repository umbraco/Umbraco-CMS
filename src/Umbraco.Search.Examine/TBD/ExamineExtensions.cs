using System.Globalization;
using Examine;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Search;
using Umbraco.Search.Enums;

namespace Umbraco.Search.Examine.TBD;

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
        switch (fieldType)
        {
            case UmbracoFieldType.Integer:
                return FieldDefinitionTypes.Integer;
            case UmbracoFieldType.Raw:
                return FieldDefinitionTypes.Raw;
            case UmbracoFieldType.DateTime:
                return FieldDefinitionTypes.DateTime;
            case UmbracoFieldType.EmailAddress:
                return FieldDefinitionTypes.EmailAddress;
            case UmbracoFieldType.InvariantCultureIgnoreCase:
                return FieldDefinitionTypes.InvariantCultureIgnoreCase;
            case UmbracoFieldType.Double:
                return FieldDefinitionTypes.Double;
            case UmbracoFieldType.Float:
                return FieldDefinitionTypes.Float;
            case UmbracoFieldType.Long:
                return FieldDefinitionTypes.Long;
            case UmbracoFieldType.DateYear:
                return FieldDefinitionTypes.DateYear;
            case UmbracoFieldType.DateMonth:
                return FieldDefinitionTypes.DateMonth;
            case UmbracoFieldType.DateDay:
                return FieldDefinitionTypes.DateDay;
            case UmbracoFieldType.DateHour:
                return FieldDefinitionTypes.DateDay;
            case UmbracoFieldType.FullText :
                return FieldDefinitionTypes.DateDay;
            case UmbracoFieldType.FullTextSortable:
                return FieldDefinitionTypes.DateDay;
        }

        //if type unknown return RAW
        return FieldDefinitionTypes.Raw;
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
                switch (indexType)
                {
                    case IndexTypes.Content:
                        content = snapshot.Content?.GetById(contentId);
                        break;
                    case IndexTypes.Media:
                        content = snapshot.Media?.GetById(contentId);
                        break;
                    case IndexTypes.Member:
                        throw new NotSupportedException("Cannot convert search results to member instances");
                    default:
                        continue;
                }

                if (content != null)
                {
                    publishedSearchResults.Add(new PublishedSearchResult(content, result.Score));
                }
            }
        }

        return publishedSearchResults;
    }
}
