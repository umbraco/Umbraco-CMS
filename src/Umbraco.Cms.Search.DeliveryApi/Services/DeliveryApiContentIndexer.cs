using Microsoft.Extensions.Logging;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using IndexField = Umbraco.Cms.Search.Core.Models.Indexing.IndexField;

namespace Umbraco.Cms.Search.DeliveryApi.Services;

/// <summary>
/// Indexes Delivery API selector, filter and sorter field values for content, using the registered Delivery API content index handlers.
/// </summary>
internal sealed class DeliveryApiContentIndexer : IContentIndexer
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly ILogger<DeliveryApiContentIndexer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryApiContentIndexer"/> class.
    /// </summary>
    /// <param name="contentIndexHandlerCollection">The collection of Delivery API index handlers to run for each piece of content.</param>
    /// <param name="dateTimeOffsetConverter">The converter used to normalize indexed date values to <see cref="DateTimeOffset"/>.</param>
    /// <param name="logger">The logger used to record duplicate or unresolved field values from the index handlers.</param>
    public DeliveryApiContentIndexer(
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IDateTimeOffsetConverter dateTimeOffsetConverter,
        ILogger<DeliveryApiContentIndexer> logger)
    {
        _contentIndexHandlerCollection = contentIndexHandlerCollection;
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _logger = logger;
    }

    /// <summary>
    /// Builds the index fields for Delivery API selectors, filters and sorters for the given content, one set per requested culture.
    /// </summary>
    /// <param name="content">The content to index. Only <see cref="IContent"/> is supported; other types yield no fields.</param>
    /// <param name="cultures">The cultures to build fields for.</param>
    /// <param name="published">Whether the published or draft version of the content is being indexed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Delivery API index fields for the content.</returns>
    public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
    {
        if (content is not IContent concreteContent)
        {
            return Task.FromResult(Enumerable.Empty<IndexField>());
        }

        var indexFieldsByIdentifier = new Dictionary<string, IndexField>();

        foreach (IContentIndexHandler handler in _contentIndexHandlerCollection)
        {
            // ignore the core handlers, as they've covered by the system fields
            if (handler.GetType().Namespace?.StartsWith("Umbraco.Cms.Api.Delivery") is true)
            {
                continue;
            }

            foreach (var culture in cultures)
            {
                Cms.Core.DeliveryApi.IndexField[] fields = handler.GetFields().ToArray();
                IndexFieldValue[] fieldValues = handler.GetFieldValues(concreteContent, culture).ToArray();

                foreach (IndexFieldValue fieldValue in fieldValues)
                {
                    var identifier = $"{fieldValue.FieldName}|{culture}";
                    if (indexFieldsByIdentifier.ContainsKey(identifier))
                    {
                        _logger.LogWarning(
                            "Duplicate field value found for field name {fieldName} (culture: {culture}) among the index handlers - first one wins.",
                            fieldValue.FieldName,
                            culture ?? "[null]");
                        continue;
                    }

                    Cms.Core.DeliveryApi.IndexField? field = fields.FirstOrDefault(f => f.FieldName == fieldValue.FieldName);
                    if (field is null)
                    {
                        _logger.LogWarning("Field name {fieldName}  did not have a corresponding field definition from the index handler {indexHandler}", fieldValue.FieldName, handler.GetType().FullName);
                        continue;
                    }

                    IndexValue? indexValue = null;
                    switch (field.FieldType)
                    {
                        case FieldType.StringRaw:
                            var keywords = fieldValue.Values.OfType<string>().ToArray();
                            if (keywords.Length > 0)
                            {
                                indexValue = new()
                                {
                                    Keywords = keywords
                                };
                            }
                            break;
                        case FieldType.StringAnalyzed:
                        case FieldType.StringSortable:
                            var texts = fieldValue.Values.OfType<string>().ToArray();
                            if (texts.Length > 0)
                            {
                                indexValue = new()
                                {
                                    Texts = texts
                                };
                            }
                            break;
                        case FieldType.Number:
                            var decimals = fieldValue.Values.OfType<decimal>()
                                .Union(fieldValue.Values.OfType<int>().Select(i => (decimal)i))
                                .ToArray();
                            if (decimals.Length > 0)
                            {
                                indexValue = new()
                                {
                                    Decimals = decimals
                                };
                            }
                            break;
                        case FieldType.Date:
                            DateTimeOffset[] dateTimeOffsets = fieldValue.Values
                                .OfType<DateTime>()
                                .Select(_dateTimeOffsetConverter.ToDateTimeOffset)
                                .ToArray();
                            if (dateTimeOffsets.Length > 0)
                            {
                                indexValue = new()
                                {
                                    DateTimeOffsets = dateTimeOffsets
                                };
                            }
                            break;
                    }

                    if (indexValue is null)
                    {
                        continue;
                    }

                    indexFieldsByIdentifier[identifier] = new IndexField(fieldValue.FieldName, indexValue, culture, null);
                }
            }
        }

        return Task.FromResult(indexFieldsByIdentifier.Values.AsEnumerable());
    }
}
