using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Models.ImportExport;

/// <summary>
///     Represents the analysis result of an entity from XML import/export data.
/// </summary>
public class EntityXmlAnalysis
{
    /// <summary>
    ///     Gets or sets the type of the entity.
    /// </summary>
    public UmbracoEntityTypes EntityType { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the entity.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of the entity.
    /// </summary>
    public Guid? Key { get; set; }
}
