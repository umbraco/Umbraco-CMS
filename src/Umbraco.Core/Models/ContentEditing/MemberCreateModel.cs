namespace Umbraco.Cms.Core.Models.ContentEditing;

public class MemberCreateModel : MemberEditingModelBase
{
    public string Password { get; set; } = string.Empty;

    public Guid? Key { get; set; }

    public Guid ContentTypeKey { get; set; } = Guid.Empty;
}

