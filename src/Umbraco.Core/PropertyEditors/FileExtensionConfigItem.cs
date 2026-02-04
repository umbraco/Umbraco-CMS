namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a file extension configuration item used by file upload property editors.
/// </summary>
public class FileExtensionConfigItem : IFileExtensionConfigItem
{
    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public string? Value { get; set; }
}
