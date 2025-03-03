using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Api.Management.Factories;

public class TemporaryFileConfigurationPresentationFactory : ITemporaryFileConfigurationPresentationFactory
{
    private readonly RuntimeSettings _runtimeSettings;
    private readonly IImageUrlGenerator _imageUrlGenerator;
    private readonly ContentSettings _contentSettings;

    public TemporaryFileConfigurationPresentationFactory(IOptionsSnapshot<ContentSettings> contentSettings, IOptionsSnapshot<RuntimeSettings> runtimeSettings, IImageUrlGenerator imageUrlGenerator)
    {
        _runtimeSettings = runtimeSettings.Value;
        _imageUrlGenerator = imageUrlGenerator;
        _contentSettings = contentSettings.Value;
    }

    public TemporaryFileConfigurationResponseModel Create() =>
        new()
        {
            ImageFileTypes = _imageUrlGenerator.SupportedImageFileTypes.ToArray(),
            DisallowedUploadedFilesExtensions = _contentSettings.DisallowedUploadedFileExtensions.ToArray(),
            AllowedUploadedFileExtensions = _contentSettings.AllowedUploadedFileExtensions.ToArray(),
            MaxFileSize = _runtimeSettings.MaxRequestLength,
        };
}
