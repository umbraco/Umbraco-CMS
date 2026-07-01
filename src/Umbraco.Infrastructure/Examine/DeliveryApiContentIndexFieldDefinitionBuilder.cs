using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexFieldDefinitionBuilder : IDeliveryApiContentIndexFieldDefinitionBuilder
{
    private readonly ContentIndexHandlerCollection _indexHandlers;
    private readonly ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Examine.DeliveryApiContentIndexFieldDefinitionBuilder"/> class,
    /// which is responsible for building field definitions for the Delivery API content index.
    /// </summary>
    /// <param name="indexHandlers">A collection of handlers used to define how content fields are indexed for the Delivery API.</param>
    /// <param name="logger">The logger used for logging diagnostic and operational information.</param>
    public DeliveryApiContentIndexFieldDefinitionBuilder(
        ContentIndexHandlerCollection indexHandlers,
        ILogger<DeliveryApiContentIndexFieldDefinitionBuilder> logger)
    {
        _indexHandlers = indexHandlers;
        _logger = logger;
    }

    /// <summary>
    /// Builds a collection of field definitions used for the delivery API content index.
    /// </summary>
    /// <returns>
    /// A <see cref="Umbraco.Cms.Infrastructure.Examine.FieldDefinitionCollection"/> containing the generated field definitions.
    /// </returns>
    public FieldDefinitionCollection Build()
    {
        var fieldDefinitions = new List<FieldDefinition>();

        AddRequiredFieldDefinitions(fieldDefinitions);
        AddContentIndexHandlerFieldDefinitions(fieldDefinitions);

        return new FieldDefinitionCollection(fieldDefinitions.ToArray());
    }

    // required field definitions go here
    // see also the field definitions in the Delivery API content index value set builder
    private void AddRequiredFieldDefinitions(ICollection<FieldDefinition> fieldDefinitions)
    {
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.Id, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.ContentTypeId, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.Published, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.DeliveryApiContentIndex.ProtectedAccess, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.IndexPathFieldName, FieldDefinitionTypes.Raw));
        fieldDefinitions.Add(new(UmbracoExamineFieldNames.NodeNameFieldName, FieldDefinitionTypes.Raw));
    }

    private void AddContentIndexHandlerFieldDefinitions(ICollection<FieldDefinition> fieldDefinitions)
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

                FieldDefinition fieldDefinition = CreateFieldDefinition(field);
                fieldDefinitions.Add(fieldDefinition);
            }
        }
    }

    private static FieldDefinition CreateFieldDefinition(IndexField field)
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

        return new FieldDefinition(field.FieldName, indexType);
    }
}
