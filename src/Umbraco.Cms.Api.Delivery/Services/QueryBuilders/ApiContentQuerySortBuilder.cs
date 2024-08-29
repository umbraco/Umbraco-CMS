using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services.QueryBuilders;

internal sealed class ApiContentQuerySortBuilder
{
    private readonly Dictionary<string, FieldType> _fieldTypes;
    private readonly ILogger _logger;

    public ApiContentQuerySortBuilder(Dictionary<string, FieldType> fieldTypes, ILogger logger)
    {
        _fieldTypes = fieldTypes;
        _logger = logger;
    }

    public void Append(IList<SortOption> sortOptions, IOrdering queryOperation)
    {
        foreach (SortOption sort in sortOptions)
        {
            if (_fieldTypes.TryGetValue(sort.FieldName, out FieldType fieldType) is false)
            {
                _logger.LogWarning(
                    "Sort implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.",
                    sort.FieldName);
                continue;
            }

            SortType sortType = fieldType switch
            {
                FieldType.Number => SortType.Int,
                FieldType.Date => SortType.Long,
                FieldType.StringRaw => SortType.String,
                FieldType.StringAnalyzed => SortType.String,
                FieldType.StringSortable => SortType.String,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldType))
            };

            queryOperation = sort.Direction switch
            {
                Direction.Ascending => queryOperation.OrderBy(new SortableField(sort.FieldName, sortType)),
                Direction.Descending => queryOperation.OrderByDescending(new SortableField(sort.FieldName, sortType)),
                _ => queryOperation
            };
        }
    }
}
