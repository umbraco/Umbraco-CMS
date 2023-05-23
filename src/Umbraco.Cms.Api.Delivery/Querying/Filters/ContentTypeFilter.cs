using Umbraco.Cms.Api.Delivery.Indexing.Filters;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying.Filters;

public sealed class ContentTypeFilter : IFilterHandler
{
    private const string ContentTypeSpecifier = "contentType:";

    /// <inheritdoc />
    public bool CanHandle(string query)
        => query.StartsWith(ContentTypeSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public FilterOption BuildFilterOption(string filter)
    {
        var alias = filter.Substring(ContentTypeSpecifier.Length);

        return new FilterOption
        {
            FieldName = ContentTypeFilterIndexer.FieldName,
            Values = alias.IsNullOrWhiteSpace() == false
                ? new[] { alias.TrimStart('!') }
                : Array.Empty<string>(),
            Operator = alias.StartsWith('!')
                ? FilterOperation.IsNot
                : FilterOperation.Is
        };
    }
}
