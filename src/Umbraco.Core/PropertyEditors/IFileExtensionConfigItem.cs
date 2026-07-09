namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a file extension configuration item used by file upload property editors.
/// </summary>
public interface IFileExtensionConfigItem
{
    /// <summary>
    ///     Gets or sets the unique identifier for this file extension configuration item.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    ///     Gets or sets the file extension value (e.g., "jpg", "pdf").
    /// </summary>
    string? Value { get; set; }
}
