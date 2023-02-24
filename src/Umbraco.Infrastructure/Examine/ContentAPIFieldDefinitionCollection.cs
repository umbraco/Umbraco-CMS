using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentApiFieldDefinitionCollection : FieldDefinitionCollection
{
    public static readonly FieldDefinition[] ContentDeliveryAPIIndexFieldDefinitions =
{
        new("id", FieldDefinitionTypes.Raw),
        new("parentKey", FieldDefinitionTypes.Raw),
        new("ancestorKeys", FieldDefinitionTypes.Raw)
    };

    public ContentApiFieldDefinitionCollection()
    : base(ContentDeliveryAPIIndexFieldDefinitions)
    {
    }
}
