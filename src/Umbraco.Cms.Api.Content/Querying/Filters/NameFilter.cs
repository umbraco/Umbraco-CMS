using System;
using Examine.Search;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

internal sealed class NameFilter : QueryOptionBase, IFilterHandler
{
    private const string NameSpecifier = "name:";

    public NameFilter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(NameSpecifier, StringComparison.OrdinalIgnoreCase);

    public IBooleanOperation? BuildFilterIndexQuery(IQuery query, string filterValueString)
    {
        var alias = filterValueString.Substring(NameSpecifier.Length);
        return query.Field("name", alias);
    }
}
