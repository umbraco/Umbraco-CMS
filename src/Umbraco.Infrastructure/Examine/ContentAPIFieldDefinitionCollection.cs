using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public class ContentAPIFieldDefinitionCollection : FieldDefinitionCollection
{
    public static readonly FieldDefinition[] ContentDeliveryAPIIndexFieldDefinitions =
{
        new("id", FieldDefinitionTypes.Raw),
        new("parentKey", FieldDefinitionTypes.Raw),
        new("ancestorKeys", FieldDefinitionTypes.Raw)
    };

    public ContentAPIFieldDefinitionCollection()
    : base(ContentDeliveryAPIIndexFieldDefinitions)
    {
    }
}
