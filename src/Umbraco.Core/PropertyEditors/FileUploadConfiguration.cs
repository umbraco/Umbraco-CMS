namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the file upload address value editor.
/// </summary>
public class FileUploadConfiguration : IFileExtensionsConfig
{
    [ConfigurationField("fileExtensions", "Accepted file extensions", "multivalues")]
    public List<FileExtensionConfigItem> FileExtensions { get; set; } = new();
}
