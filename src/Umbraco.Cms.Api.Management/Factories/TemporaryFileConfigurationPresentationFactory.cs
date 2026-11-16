using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for temporary file configurations.
/// </summary>
public class TemporaryFileConfigurationPresentationFactory : ITemporaryFileConfigurationPresentationFactory
{
    private readonly RuntimeSettings _runtimeSettings;
    private readonly ContentSettings _contentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryFileConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="contentSettings">An options snapshot containing the content settings.</param>
    /// <param name="runtimeSettings">An options snapshot containing the runtime settings.</param>
    public TemporaryFileConfigurationPresentationFactory(
        IOptionsSnapshot<ContentSettings> contentSettings,
        IOptionsSnapshot<RuntimeSettings> runtimeSettings)
    {
        _runtimeSettings = runtimeSettings.Value;
        _contentSettings = contentSettings.Value;
    }

    /// <summary>
    /// Creates and returns a <see cref="TemporaryFileConfigurationResponseModel"/> populated with the current temporary file configuration settings, including allowed and disallowed file extensions, image file types, and maximum file size.
    /// </summary>
    /// <returns>A <see cref="TemporaryFileConfigurationResponseModel"/> containing the current temporary file configuration settings.</returns>
    public TemporaryFileConfigurationResponseModel Create() =>
        new()
        {
            ImageFileTypes = _contentSettings.Imaging.ImageFileTypes.ToArray(),
            DisallowedUploadedFilesExtensions = _contentSettings.DisallowedUploadedFileExtensions.ToArray(),
            AllowedUploadedFileExtensions = _contentSettings.AllowedUploadedFileExtensions.ToArray(),
            MaxFileSize = _runtimeSettings.MaxRequestLength,
        };
}
