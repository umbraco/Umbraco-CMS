using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;

/// <summary>
/// Indexes property values for a content item, delegating each property to the matching <see cref="IPropertyValueHandler"/>.
/// Sensitive member properties are excluded.
/// </summary>
internal sealed class PropertyValueFieldsContentIndexer : IContentIndexer
{
    private readonly PropertyValueHandlerCollection _propertyValueHandlerCollection;
    private readonly IMemberTypeService _memberTypeService;
    private readonly ILogger<PropertyValueFieldsContentIndexer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyValueFieldsContentIndexer"/> class.
    /// </summary>
    /// <param name="propertyValueHandlerCollection">The registered property value handlers, one per supported property editor.</param>
    /// <param name="memberTypeService">The service used to determine whether a member property is sensitive and should be excluded.</param>
    /// <param name="logger">The logger used to record property editors with no matching value handler.</param>
    public PropertyValueFieldsContentIndexer(
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        IMemberTypeService memberTypeService,
        ILogger<PropertyValueFieldsContentIndexer> logger)
    {
        _propertyValueHandlerCollection = propertyValueHandlerCollection;
        _logger = logger;
        _memberTypeService = memberTypeService;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
        => await CollectPropertyValueFields(content, cultures, published);

    private async Task<IEnumerable<IndexField>> CollectPropertyValueFields(IContentBase content, string?[] cultures, bool published)
    {
        var fields = new List<IndexField>();

        IMemberType? memberType = content is IMember ? await _memberTypeService.GetAsync(content.ContentType.Key) : null;

        foreach (IProperty property in content.Properties)
        {
            if (memberType?.IsSensitiveProperty(property.Alias) is true)
            {
                // explicitly filter out sensitive properties
                continue;
            }

            IPropertyValueHandler? handler = _propertyValueHandlerCollection.GetPropertyValueHandler(property.PropertyType);
            if (handler is null)
            {
                _logger.LogDebug(
                    "No property value handler found for property editor alias {propertyEditorAlias} - cannot index property value.",
                    property.PropertyType.PropertyEditorAlias);
                continue;
            }

            var propertyCultures = property.PropertyType.VariesByCulture()
                ? cultures
                : [null];

            var propertySegments = property.PropertyType.VariesBySegment()
                ? property.Values.Select(value => value.Segment).Distinct().ToArray()
                : [null];

            foreach (var culture in propertyCultures)
            {
                foreach (var segment in propertySegments)
                {
                    IEnumerable<IndexField>? indexFields = handler.GetIndexFields(property, culture, segment, published, content);
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (indexFields is null)
                    {
                        continue;
                    }
                    fields.AddRange(indexFields);
                }
            }
        }

        return fields;
    }
}
