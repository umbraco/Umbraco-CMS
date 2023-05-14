using Umbraco.Cms.Api.Delivery.Indexing.Sorts;
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

        return new FilterOption
        {
            FieldName = NameSortIndexer.FieldName,
            Values = new[] { value.TrimStart('!') },
            Operator = value.StartsWith('!')
                ? FilterOperation.IsNot
                : FilterOperation.Is
        };
    }
}
