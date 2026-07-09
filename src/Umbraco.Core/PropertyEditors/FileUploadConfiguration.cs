namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the file upload address value editor.
/// </summary>
public class FileUploadConfiguration
{
    /// <summary>
    /// Gets or sets the allowed file extensions for uploads.
    /// </summary>
    [ConfigurationField("fileExtensions")]
    public IEnumerable<string> FileExtensions { get; set; } = Enumerable.Empty<string>();
}
