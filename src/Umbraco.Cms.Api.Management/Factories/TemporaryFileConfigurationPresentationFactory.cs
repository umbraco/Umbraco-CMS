using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for temporary file configurations.
/// </summary>
public class TemporaryFileConfigurationPresentationFactory : ITemporaryFileConfigurationPresentationFactory
{
    private readonly RuntimeSettings _runtimeSettings;
    private readonly ContentSettings _contentSettings;
    private readonly ContentImagingSettings _imagingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFileConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="contentSettings">An options snapshot containing the current content settings configuration.</param>
    /// <param name="runtimeSettings">An options snapshot containing the current runtime settings configuration.</param>
    /// <param name="imageUrlGenerator">The service used to generate image URLs.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFileConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="contentSettings">An options snapshot containing the content settings.</param>
    /// <param name="runtimeSettings">An options snapshot containing the runtime settings.</param>
    /// <param name="imagingSettings">An options snapshot containing the content imaging settings.</param>
    /// <param name="imageUrlGenerator">The service used to generate image URLs.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFileConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="contentSettings">An options snapshot containing the content settings.</param>
    /// <param name="runtimeSettings">An options snapshot containing the runtime settings.</param>
    /// <param name="imagingSettings">An options snapshot containing the content imaging settings.</param>
    public TemporaryFileConfigurationPresentationFactory(
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<RuntimeSettings> runtimeSettings,
        IOptionsSnapshot<ContentImagingSettings> imagingSettings)
    {
        _runtimeSettings = runtimeSettings.Value;
        _contentSettings = contentSettings.Value;
        _imagingSettings = imagingSettings.Value;
    }

    /// <summary>
    /// Creates and returns a <see cref="TemporaryFileConfigurationResponseModel"/> populated with the current temporary file configuration settings, including allowed and disallowed file extensions, image file types, and maximum file size.
    /// </summary>
    /// <returns>A <see cref="TemporaryFileConfigurationResponseModel"/> containing the current temporary file configuration settings.</returns>
    public TemporaryFileConfigurationResponseModel Create() =>
        new()
        {
            ImageFileTypes = _imagingSettings.ImageFileTypes.ToArray(),
            DisallowedUploadedFilesExtensions = _contentSettings.DisallowedUploadedFileExtensions.ToArray(),
            AllowedUploadedFileExtensions = _contentSettings.AllowedUploadedFileExtensions.ToArray(),
            MaxFileSize = _runtimeSettings.MaxRequestLength,
        };
}
