namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

public class MemberTypeCreateModel : MemberTypeModelBase
{
    public Guid? Key { get; set; }

    public Guid? ContainerKey { get; set; }
}
