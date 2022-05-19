namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used to track the property types that are visible/editable on member profiles
/// </summary>
public class MemberTypePropertyProfileAccess
{
    public MemberTypePropertyProfileAccess(bool isVisible, bool isEditable, bool isSenstive)
    {
        IsVisible = isVisible;
        IsEditable = isEditable;
        IsSensitive = isSenstive;
    }

    public bool IsVisible { get; set; }

    public bool IsEditable { get; set; }

    public bool IsSensitive { get; set; }
}
