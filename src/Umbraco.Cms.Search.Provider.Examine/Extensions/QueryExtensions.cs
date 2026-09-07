using Examine.Search;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.Provider.Examine.Extensions;

/// <summary>
/// Provides Examine query-building helpers for translating the search abstractions' range/exact filters into Lucene queries.
/// </summary>
internal static class QueryExtensions
{
    /// <summary>
    /// Adds a range query for one or more ranges to the given field(s), with an inclusive lower bound and an
    /// exclusive upper bound on each range.
    /// </summary>
    /// <typeparam name="T">The type of the range bounds.</typeparam>
    /// <param name="query">The query to add the range filter to.</param>
    /// <param name="fieldName">The invariant/culture field name to query.</param>
    /// <param name="segmentFieldName">An additional segment-specific field name to query, or null if not segmented.</param>
    /// <param name="negate">If true, matches documents whose field value falls outside all of <paramref name="ranges"/>.</param>
    /// <param name="ranges">The ranges to match. A document matches if its field value falls within any one of these.</param>
    public static void AddRangeFilter<T>(this IBooleanOperation query, string fieldName, string? segmentFieldName, bool negate, IEnumerable<FilterRange<T>> ranges)
        where T : struct
    {
        FilterRange<T>[] rangesAsArray = ranges as FilterRange<T>[] ?? ranges.ToArray();

        if (rangesAsArray.Length == 0)
        {
            return;
        }

        string[] fieldNames = segmentFieldName is not null ? [fieldName, segmentFieldName] : [fieldName];

        if (negate)
        {
            foreach (FilterRange<T> range in rangesAsArray)
            {
                query.Not().RangeQuery<T>(fieldNames, range.MinValue, range.MaxValue, true, false);
            }
        }
        else
        {
            query.And().Group(nestedQuery =>
            {
                INestedBooleanOperation rangeQuery = nestedQuery.RangeQuery<T>(fieldNames, rangesAsArray[0].MinValue, rangesAsArray[0].MaxValue, true, false);
                for (var i = 1; i < rangesAsArray.Length; i++)
                {
                    rangeQuery.Or().RangeQuery<T>(fieldNames, rangesAsArray[i].MinValue, rangesAsArray[i].MaxValue, true, false);
                }

                return rangeQuery;
            });
        }
    }

    /// <summary>
    /// Adds an exact-match query for one or more values to the given field(s).
    /// </summary>
    /// <typeparam name="T">The type of the filtered values.</typeparam>
    /// <param name="query">The query to add the exact filter to.</param>
    /// <param name="fieldName">The invariant/culture field name to query.</param>
    /// <param name="segmentFieldName">An additional segment-specific field name to query, or null if not segmented.</param>
    /// <param name="filter">The exact filter describing the values to match and whether to negate the match.</param>
    public static void AddExactFilter<T>(this IBooleanOperation query, string fieldName, string? segmentFieldName, ExactFilter<T> filter) where T : struct
    {
        if (filter.Values.Length == 0)
        {
            return;
        }

        if (filter.Negate)
        {
            foreach (T filterValue in filter.Values)
            {
                if (segmentFieldName is not null)
                {
                    query.Not().Group(nestedQuery => nestedQuery.Field(fieldName, filterValue).Or().Field(segmentFieldName, filterValue));
                }
                else
                {
                    query.Not().Group(nestedQuery => nestedQuery.Field(fieldName, filterValue));
                }
            }
        }
        else
        {
            query.And().Group(nestedQuery =>
            {
                INestedBooleanOperation nestedBooleanOperation;

                if (segmentFieldName is not null)
                {
                    nestedBooleanOperation = nestedQuery.Field(fieldName, filter.Values[0]).Or().Field(segmentFieldName, filter.Values[0]);
                    for (var i = 1; i < filter.Values.Length; i++)
                    {
                        nestedBooleanOperation.Or().Field(fieldName, filter.Values[i]).Or().Field(segmentFieldName, filter.Values[i]);
                    }
                }
                else
                {
                    nestedBooleanOperation = nestedQuery.Field(fieldName, filter.Values[0]);
                    for (var i = 1; i < filter.Values.Length; i++)
                    {
                        nestedBooleanOperation.Or().Field(fieldName, filter.Values[i]);
                    }
                }

                return nestedBooleanOperation;
            });
        }
    }
}
