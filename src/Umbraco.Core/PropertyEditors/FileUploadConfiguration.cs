namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the file upload address value editor.
/// </summary>
public class FileUploadConfiguration
{
    [ConfigurationField("fileExtensions")]
    public IEnumerable<string> FileExtensions { get; set; } = Enumerable.Empty<string>();
}
