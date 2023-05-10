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

        var filterOption = new FilterOption
        {
            FieldName = NameSortIndexer.FieldName,
            Value = string.Empty
        };

        // Support negation
        if (value.StartsWith('!'))
        {
            filterOption.Value = value.Substring(1);
            filterOption.Operator = FilterOperation.IsNot;
        }
        else
        {
            filterOption.Value = value;
            filterOption.Operator = FilterOperation.Is;
        }

        return filterOption;
    }
}
