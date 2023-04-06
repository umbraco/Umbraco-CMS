using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

internal sealed class NameFilter : IFilterHandler
{
    private const string NameSpecifier = "name:";

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(NameSpecifier, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public FilterOption BuildFilterOption(string filterValueString)
    {
        var value = filterValueString.Substring(NameSpecifier.Length);

        var filter = new FilterOption
        {
            FieldName = "name",
            Value = string.Empty
        };

        // TODO: do we support negation?
        if (value.StartsWith('!'))
        {
            filter.Value = value.Substring(1);
            filter.Operator = FilterOperation.IsNot;
        }
        else
        {
            filter.Value = value;
            filter.Operator = FilterOperation.Is;
        }

        return filter;
    }
}
