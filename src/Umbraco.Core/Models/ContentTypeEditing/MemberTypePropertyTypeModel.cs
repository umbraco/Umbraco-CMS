namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class MemberTypePropertyTypeModel : PropertyTypeModelBase
{
    public bool IsSensitive { get; set; }

    public bool MemberCanView { get; set; }

    public bool MemberCanEdit { get; set; }
}
