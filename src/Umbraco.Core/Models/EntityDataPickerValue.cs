namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a value selected from an entity data picker.
/// </summary>
public sealed class EntityDataPickerValue
{
    /// <summary>
    ///     Gets or sets the collection of selected entity identifiers.
    /// </summary>
    public required IEnumerable<string> Ids { get; set; }

    /// <summary>
    ///     Gets or sets the data source from which the entities were selected.
    /// </summary>
    public required string DataSource { get; set; }
}
