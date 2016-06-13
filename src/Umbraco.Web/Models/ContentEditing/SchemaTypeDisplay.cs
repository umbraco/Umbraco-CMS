using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "schemaType", Namespace = "")]
    public class SchemaTypeDisplay : ContentTypeCompositionDisplay<PropertyTypeDisplay>
    {

    }
}