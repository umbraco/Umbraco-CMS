using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.ContentEditing;

[DataContract(Name = "contentType", Namespace = "")]
public class MemberTypeDisplay : ContentTypeCompositionDisplay<MemberPropertyTypeDisplay>
{
}
