using System.Text.RegularExpressions;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public abstract class ContainsFilterBase : IFilterHandler
{
    /// <summary>
    /// The regex to parse the filter query. Must supply two match groups named "operator" and "value", where "operator"
    /// contains the filter operator (i.e. ">") and "value" contains the value to filter on.
    /// </summary>
    /// <remarks>
    /// Supported operators:
    /// <list type="bullet">
    /// <item>":" = Is (the filter value equals the index field value)</item>
    /// <item>":!" = IsNot (the filter value does not equal the index field value)</item>
    /// <item>"&gt;" = GreaterThan (the filter value is greater than the index field value)</item>
    /// <item>"&gt;:" = GreaterThanOrEqual (the filter value is greater than or equal to the index field value)</item>
    /// <item>"&lt;" = LessThan (the filter value is less than the index field value)</item>
    /// <item>"&lt;:" = LessThanOrEqual (the filter value is less than or equal to the index field value)</item>
    /// </list>
    /// Range operators (greater than, less than) only work with numeric and date type filters.
    /// </remarks>
    protected abstract Regex QueryParserRegex { get; }

    /// <summary>
    /// The index field name to filter on.
    /// </summary>
    protected abstract string FieldName { get; }

    /// <inheritdoc />
    public bool CanHandle(string query)
        => QueryParserRegex.IsMatch(query);

    /// <inheritdoc/>
    public FilterOption BuildFilterOption(string filter)
    {
        GroupCollection groups = QueryParserRegex.Match(filter).Groups;

        if (groups.Count != 3 || groups.ContainsKey("operator") is false || groups.ContainsKey("value") is false)
        {
            return DefaultFilterOption();
        }

        FilterOperation? filterOperation = ParseFilterOperation(groups["operator"].Value);
        if (filterOperation.HasValue is false)
        {
            return DefaultFilterOption();
        }

        return new FilterOption
        {
            FieldName = FieldName,
            Values = new[] { groups["value"].Value },
            Operator = filterOperation.Value
        };

        FilterOption DefaultFilterOption()
            => new FilterOption
            {
                FieldName = FieldName,
                Values = new[] { Guid.NewGuid().ToString() },
                Operator = FilterOperation.Is
            };
    }

    private FilterOperation? ParseFilterOperation(string filterOperation)
        => filterOperation switch
        {
            ":" => FilterOperation.Is,
            ":!" => FilterOperation.IsNot,
            ">" => FilterOperation.GreaterThan,
            ">:" => FilterOperation.GreaterThanOrEqual,
            "<" => FilterOperation.LessThan,
            "<:" => FilterOperation.LessThanOrEqual,
            _ => null
        };
}
