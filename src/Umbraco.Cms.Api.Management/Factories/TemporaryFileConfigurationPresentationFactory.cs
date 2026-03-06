using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Api.Management.Factories;

public class TemporaryFileConfigurationPresentationFactory : ITemporaryFileConfigurationPresentationFactory
{
    private readonly RuntimeSettings _runtimeSettings;
    private readonly ContentSettings _contentSettings;
    private readonly ContentImagingSettings _imagingSettings;

    [Obsolete("Use the constructor that accepts IOptionsSnapshot<ContentImagingSettings> and does not accept IImageUrlGenerator instead. Scheduled for removal in Umbraco 19.")]
    public TemporaryFileConfigurationPresentationFactory(
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<RuntimeSettings> runtimeSettings,
        IImageUrlGenerator imageUrlGenerator)
        : this(
            contentSettings,
            runtimeSettings,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsSnapshot<ContentImagingSettings>>())
    {
        // IImageUrlGenerator parameter is ignored - kept for DI compatibility with existing registrations, but not used in the factory. This is to avoid breaking changes when adding the new constructor that accepts IOptionsSnapshot<ContentImagingSettings>.
    }

    [Obsolete("Use the constructor that accepts IOptionsSnapshot<ContentImagingSettings> and does not accept IImageUrlGenerator instead. Scheduled for removal in Umbraco 19.")]
    public TemporaryFileConfigurationPresentationFactory(
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<RuntimeSettings> runtimeSettings,
        IOptionsSnapshot<ContentImagingSettings> imagingSettings,
        IImageUrlGenerator imageUrlGenerator)
        : this(
            contentSettings,
            runtimeSettings,
            imagingSettings)
    {
        // IImageUrlGenerator parameter is ignored - kept for DI compatibility with existing registrations, but not used in the factory. This is to avoid breaking changes when adding the new constructor that accepts IOptionsSnapshot<ContentImagingSettings>.
    }

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
