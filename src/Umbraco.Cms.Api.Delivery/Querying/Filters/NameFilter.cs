using Umbraco.Cms.Api.Delivery.Indexing.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public sealed class NameFilter : IFilterHandler
{
    private const string NameSpecifier = "name:";

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(NameSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public FilterOption BuildFilterOption(string filter)
    {
        var value = filter.Substring(NameSpecifier.Length);
        var negate = value.StartsWith('!');
        var values = value.TrimStart('!').Split(Constants.CharArrays.Comma);

        return new FilterOption
        {
            FieldName = NameFilterIndexer.FieldName,
            Values = values,
            Operator = negate
                ? FilterOperation.DoesNotContain
                : FilterOperation.Contains
        };
    }
}
