using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiFieldDefinitionCollection : FieldDefinitionCollection
{
    public static readonly FieldDefinition[] ContentDeliveryAPIIndexFieldDefinitions =
    {
        new("id", FieldDefinitionTypes.FullText),
        new("parentKey", FieldDefinitionTypes.FullText),
        new("ancestorKeys", FieldDefinitionTypes.FullText),
        new("name", FieldDefinitionTypes.FullTextSortable),
        new("level", FieldDefinitionTypes.Integer),
        new("path", FieldDefinitionTypes.Raw), // TODO: should be sortable
        new("sortOrder", FieldDefinitionTypes.Integer)
    };

    public ContentApiFieldDefinitionCollection()
        : base(ContentDeliveryAPIIndexFieldDefinitions)
    {
    }
}
