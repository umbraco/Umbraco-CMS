using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class MemberTypeDisplay : ContentTypeCompositionDisplay<MemberPropertyTypeDisplay>
    {

    }
}