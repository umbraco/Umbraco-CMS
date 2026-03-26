namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

/// <summary>
/// Provides configuration details in the response for temporary file operations.
/// </summary>
public class TemporaryFileConfigurationResponseModel
{
    /// <summary>Gets or sets the allowed image file types.</summary>
    public string[] ImageFileTypes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the list of disallowed uploaded file extensions.
    /// </summary>
    public string[] DisallowedUploadedFilesExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the allowed file extensions for uploaded temporary files.
    /// </summary>
    public string[] AllowedUploadedFileExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes.
    /// </summary>
    public long? MaxFileSize { get; set; }
}
