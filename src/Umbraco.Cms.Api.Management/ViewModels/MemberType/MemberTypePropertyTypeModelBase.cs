using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

    /// <summary>
    /// Represents the base model for defining property types on a member type.
    /// </summary>
public abstract class MemberTypePropertyTypeModelBase : PropertyTypeModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether this property type contains sensitive information that requires special handling.
    /// </summary>
    public bool IsSensitive { get; set; }

    /// <summary>
    /// Gets or sets the visibility state (such as public or private) of the member type property.
    /// </summary>
    public MemberTypePropertyTypeVisibility Visibility { get; set; } = new();
}
