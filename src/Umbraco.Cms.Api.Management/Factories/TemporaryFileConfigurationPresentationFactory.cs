using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public class TemporaryFileConfigurationPresentationFactory : ITemporaryFileConfigurationPresentationFactory
{
    private readonly RuntimeSettings _runtimeSettings;
    private readonly ContentSettings _contentSettings;
    private readonly ContentImagingSettings _imagingSettings;

    public TemporaryFileConfigurationPresentationFactory(
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<RuntimeSettings> runtimeSettings,
        IOptionsSnapshot<ContentImagingSettings> imagingSettings)
    {
        _runtimeSettings = runtimeSettings.Value;
        _contentSettings = contentSettings.Value;
        _imagingSettings = imagingSettings.Value;
    }

    public TemporaryFileConfigurationResponseModel Create() =>
        new()
        {
            ImageFileTypes = _imagingSettings.ImageFileTypes.ToArray(),
            DisallowedUploadedFilesExtensions = _contentSettings.DisallowedUploadedFileExtensions.ToArray(),
            AllowedUploadedFileExtensions = _contentSettings.AllowedUploadedFileExtensions.ToArray(),
            MaxFileSize = _runtimeSettings.MaxRequestLength,
        };
}
