using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// this is PropertyTypeDto + the special property type fields for members
// it is used for querying everything needed for a property type, at once
internal sealed class PropertyTypeCommonDto : PropertyTypeDto
{
    /// <summary>
    /// Gets or sets a value indicating whether this property type can be edited by members in the CMS.
    /// </summary>
    [Column("memberCanEdit")]
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this property type should be visible on the profile.
    /// </summary>
    [Column("viewOnProfile")]
    public bool ViewOnProfile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property type is sensitive.
    /// </summary>
    [Column("isSensitive")]
    public bool IsSensitive { get; set; }
}
