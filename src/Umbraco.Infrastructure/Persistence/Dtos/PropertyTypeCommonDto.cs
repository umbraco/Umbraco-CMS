using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// this is PropertyTypeDto + the special property type fields for members
// it is used for querying everything needed for a property type, at once
internal class PropertyTypeCommonDto : PropertyTypeDto
{
    [Column("memberCanEdit")]
    public bool CanEdit { get; set; }

    [Column("viewOnProfile")]
    public bool ViewOnProfile { get; set; }

    [Column("isSensitive")]
    public bool IsSensitive { get; set; }
}
