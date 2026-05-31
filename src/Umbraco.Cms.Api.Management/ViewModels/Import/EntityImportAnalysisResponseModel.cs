namespace Umbraco.Cms.Api.Management.ViewModels.Import;

/// <summary>
/// Represents the response returned after analyzing an entity import.
/// </summary>
public class EntityImportAnalysisResponseModel
{
    /// <summary>
    /// Gets or sets the type of the entity being imported.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the entity.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier key of the entity.
    /// </summary>
    public Guid? Key { get; set; }
}
