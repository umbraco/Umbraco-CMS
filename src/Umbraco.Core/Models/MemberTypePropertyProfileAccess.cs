namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used to track the property types that are visible/editable on member profiles
/// </summary>
public class MemberTypePropertyProfileAccess
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberTypePropertyProfileAccess" /> class.
    /// </summary>
    /// <param name="isVisible">Whether the property is visible on the member profile.</param>
    /// <param name="isEditable">Whether the property can be edited by the member.</param>
    /// <param name="isSenstive">Whether the property contains sensitive data.</param>
    public MemberTypePropertyProfileAccess(bool isVisible, bool isEditable, bool isSenstive)
    {
        IsVisible = isVisible;
        IsEditable = isEditable;
        IsSensitive = isSenstive;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the property is visible on the member profile.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the property can be edited by the member.
    /// </summary>
    public bool IsEditable { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the property contains sensitive data.
    /// </summary>
    public bool IsSensitive { get; set; }
}
