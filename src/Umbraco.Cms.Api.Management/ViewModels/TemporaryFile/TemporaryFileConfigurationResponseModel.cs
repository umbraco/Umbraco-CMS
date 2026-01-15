namespace Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

public class TemporaryFileConfigurationResponseModel
{
    public string[] ImageFileTypes { get; set; } = Array.Empty<string>();

    public string[] DisallowedUploadedFilesExtensions { get; set; } = Array.Empty<string>();

    public string[] AllowedUploadedFileExtensions { get; set; } = Array.Empty<string>();

    public long? MaxFileSize { get; set; }
}
