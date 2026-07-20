using Examine.Search;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.Provider.Examine.Extensions;

internal static class QueryExtensions
{
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
