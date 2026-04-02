namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Specifies the visibility options available for a property type within a member type.
/// </summary>
public class MemberTypePropertyTypeVisibility
{
    /// <summary>
    /// Gets or sets a value indicating whether a member can view this property type.
    /// </summary>
    public bool MemberCanView { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member can edit this property type.
    /// </summary>
    public bool MemberCanEdit { get; set; }
}
