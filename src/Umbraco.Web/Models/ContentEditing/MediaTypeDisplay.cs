using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class MediaTypeDisplay : ContentTypeCompositionDisplay<PropertyTypeDisplay>
    {

    }
}