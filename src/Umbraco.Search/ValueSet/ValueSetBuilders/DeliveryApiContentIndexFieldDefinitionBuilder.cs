﻿using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Search;
using Umbraco.Extensions;

namespace Umbraco.Search.ValueSet.ValueSetBuilders;

internal sealed class DeliveryApiContentIndexFieldDefinitionBuilder : IDeliveryApiContentIndexFieldDefinitionBuilder
{
    private readonly ContentIndexHandlerCollection _indexHandlers;
    private readonly ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> _logger;

    public DeliveryApiContentIndexFieldDefinitionBuilder(
        ContentIndexHandlerCollection indexHandlers,
        ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> logger)
    {
        _indexHandlers = indexHandlers;
        _logger = logger;
    }

    public FieldDefinitionCollection Build()
    {
        var fieldDefinitions = new List<global::Examine.FieldDefinition>();

        AddRequiredFieldDefinitions(fieldDefinitions);
        AddContentIndexHandlerFieldDefinitions(fieldDefinitions);

        return new FieldDefinitionCollection(fieldDefinitions.ToArray());
    }

    // required field definitions go here
    // see also the field definitions in the Delivery API content index value set builder
    private void AddRequiredFieldDefinitions(ICollection<global::Examine.FieldDefinition> fieldDefinitions)
    {
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.DeliveryApiContentIndex.Id, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.DeliveryApiContentIndex.ContentTypeId, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.DeliveryApiContentIndex.Culture, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.DeliveryApiContentIndex.AncestorIds, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.PublishedFieldName, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.IndexPathFieldName, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoSearchFieldNames.NodeNameFieldName, FieldDefinitionTypes.Raw));
    }

    private void AddContentIndexHandlerFieldDefinitions(ICollection<global::Examine.FieldDefinition> fieldDefinitions)
    {
        // add index fields from index handlers (selectors, filters, sorts)
        foreach (IContentIndexHandler handler in _indexHandlers)
        {
            IndexField[] fields = handler.GetFields().ToArray();

            foreach (IndexField field in fields)
            {
                if (fieldDefinitions.Any(fieldDefinition => fieldDefinition.Name.InvariantEquals(field.FieldName)))
                {
                    _logger.LogWarning("Duplicate field definitions found for field name {FieldName} among the index handlers - first one wins.", field.FieldName);
                    continue;
                }

                global::Examine.FieldDefinition fieldDefinition = CreateFieldDefinition(field);
                fieldDefinitions.Add(fieldDefinition);
            }
        }
    }

    private static global::Examine.FieldDefinition CreateFieldDefinition(IndexField field)
    {
        var indexType = field.FieldType switch
        {
            FieldType.Date => FieldDefinitionTypes.DateTime,
            FieldType.Number => FieldDefinitionTypes.Integer,
            FieldType.StringRaw => FieldDefinitionTypes.Raw,
            FieldType.StringAnalyzed => FieldDefinitionTypes.FullText,
            FieldType.StringSortable => FieldDefinitionTypes.FullTextSortable,
            _ => throw new ArgumentOutOfRangeException(nameof(field.FieldType))
        };

        return new global::Examine.FieldDefinition(field.FieldName, indexType);
    }
}
