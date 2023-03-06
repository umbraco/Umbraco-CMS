using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiFieldDefinitionCollection : FieldDefinitionCollection
{
    public static readonly FieldDefinition[] ContentDeliveryAPIIndexFieldDefinitions =
    {
        new("id", FieldDefinitionTypes.FullText),
        new("parentKey", FieldDefinitionTypes.FullText),
        new("ancestorKeys", FieldDefinitionTypes.FullText)
    };

    public ContentApiFieldDefinitionCollection()
        : base(ContentDeliveryAPIIndexFieldDefinitions)
    {
    }
}
