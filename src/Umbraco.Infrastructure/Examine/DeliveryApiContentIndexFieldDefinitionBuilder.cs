using Examine;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeliveryApiContentIndexFieldDefinitionBuilder : IDeliveryApiContentIndexFieldDefinitionBuilder
{
    private readonly ContentIndexHandlerCollection _contentIndexHandlerCollection;

    public DeliveryApiContentIndexFieldDefinitionBuilder(ContentIndexHandlerCollection contentIndexHandlerCollection)
        => _contentIndexHandlerCollection = contentIndexHandlerCollection;

    public FieldDefinitionCollection Build()
    {
        // mandatory field definitions go here
        // see also the field definitions in the Delivery API content index value set builder
        var fieldDefinitions = new List<FieldDefinition>
        {
            new("id", FieldDefinitionTypes.Integer),
            new(UmbracoExamineFieldNames.IndexPathFieldName, FieldDefinitionTypes.Raw),
            new(UmbracoExamineFieldNames.NodeNameFieldName, FieldDefinitionTypes.Raw)
        };

        // add custom fields from index handlers (selectors, filters, sorts)
        IndexField[] fields = _contentIndexHandlerCollection
            .SelectMany(handler => handler.GetFields())
            .Where(field => fieldDefinitions.Any(fieldDefinition => fieldDefinition.Name.InvariantEquals(field.FieldName)) is false)
            .DistinctBy(field => field.FieldName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        fieldDefinitions.AddRange(
            fields.Select(field =>
            {
                var type = field.FieldType switch
                {
                    FieldType.Date => FieldDefinitionTypes.DateTime,
                    FieldType.Number => FieldDefinitionTypes.Integer,
                    FieldType.String => FieldDefinitionTypes.FullText,
                    FieldType.StringSortable => FieldDefinitionTypes.FullTextSortable,
                    _ => throw new ArgumentOutOfRangeException(nameof(field.FieldType))
                };

                return new FieldDefinition(field.FieldName, type);
            }));

        return new FieldDefinitionCollection(fieldDefinitions.ToArray());
    }
}
