using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Indexing.Sorts;

public sealed class PathSortIndexer : IContentIndexHandler
{
    private readonly IEntityService _entityService;

    public PathSortIndexer(IEntityService entityService)
        => _entityService = entityService;

    internal const string FieldName = "pathOrder";

    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content)
    {
        string? pathOrder = null;
        var ancestorIds = content.GetAncestorIds()?.ToArray();

        if (ancestorIds is not null && ancestorIds.Any())
        {
            var sortOrders = _entityService
                .GetAll(UmbracoObjectTypes.Document, ancestorIds)
                .OrderBy(e => e.Level)
                .Select(e => e.SortOrder)
                .ToArray();

            pathOrder = string.Join(",", sortOrders);
        }

        // Start with the sort order of the current node as initial value
        var initialValue = content.SortOrder.ToString();
        pathOrder = !string.IsNullOrEmpty(pathOrder) ? $"{pathOrder},{initialValue}" : initialValue;

        yield return new IndexFieldValue { FieldName = FieldName, Value = pathOrder };
    }

    public IEnumerable<IndexField> GetFields()
        => new[] { new IndexField { FieldName = FieldName, FieldType = FieldType.StringSortable } };
}
