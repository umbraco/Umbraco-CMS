namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class MemberEditingModelBase : ContentEditingModelBase
{
    public bool IsApproved { get; set; }

    public IEnumerable<Guid>? Roles { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
