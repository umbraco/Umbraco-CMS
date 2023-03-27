using System;
using Examine.Search;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Content.Querying.Filters;

internal sealed class ContentTypeFilter : QueryOptionBase, IFilterHandler
{
    private const string ContentTypeSpecifier = "contentType:";

    public ContentTypeFilter(IPublishedSnapshotAccessor publishedSnapshotAccessor, IRequestRoutingService requestRoutingService)
        : base(publishedSnapshotAccessor, requestRoutingService)
    {
    }

    /// <inheritdoc />
    public bool CanHandle(string queryString)
        => queryString.StartsWith(ContentTypeSpecifier, StringComparison.OrdinalIgnoreCase);

    public IBooleanOperation? BuildFilterIndexQuery(IQuery query, string filterValueString)
    {
        var alias = filterValueString.Substring(ContentTypeSpecifier.Length);
        return query.Field("__NodeTypeAlias", alias);
    }
}
