using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.RelationType;

public class RelationTypeBaseModel
{
    /// <summary>
    ///     Gets or sets the name of the model.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
    /// </summary>
    public bool IsBidirectional { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the RelationType should be returned in "Used by"-queries.
    /// </summary>
    public bool IsDependency { get; set; }
}
