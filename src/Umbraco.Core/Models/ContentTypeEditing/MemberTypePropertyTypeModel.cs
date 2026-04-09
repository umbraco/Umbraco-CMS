namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
///     Represents a property type model for member types with additional member-specific settings.
/// </summary>
public class MemberTypePropertyTypeModel : PropertyTypeModelBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the property contains sensitive data.
    /// </summary>
    public bool IsSensitive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether members can view this property on their own profile.
    /// </summary>
    public bool MemberCanView { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether members can edit this property on their own profile.
    /// </summary>
    public bool MemberCanEdit { get; set; }
}
