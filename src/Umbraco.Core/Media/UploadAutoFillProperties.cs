using System.Drawing;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Media;

/// <summary>
/// Provides methods to manage auto-fill properties for upload fields.
/// </summary>
public class UploadAutoFillProperties
{
    private readonly IImageDimensionExtractor _imageDimensionExtractor;
    private readonly ILogger<UploadAutoFillProperties> _logger;
    private readonly MediaFileManager _mediaFileManager;

    public UploadAutoFillProperties(
        MediaFileManager mediaFileManager,
        ILogger<UploadAutoFillProperties> logger,
        IImageDimensionExtractor imageDimensionExtractor)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _imageDimensionExtractor = imageDimensionExtractor ?? throw new ArgumentNullException(nameof(imageDimensionExtractor));
    }

    /// <summary>
    /// Resets the auto-fill properties of a content item, for a specified auto-fill configuration.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="autoFillConfig">The auto-fill configuration.</param>
    /// <param name="culture">Variation language.</param>
    /// <param name="segment">Variation segment.</param>
    public void Reset(IContentBase content, ImagingAutoFillUploadField autoFillConfig, string? culture, string? segment)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(autoFillConfig);

        ResetProperties(content, autoFillConfig, culture, segment);
    }

    /// <summary>
    /// Populates the auto-fill properties of a content item, for a specified auto-fill configuration.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="autoFillConfig">The auto-fill configuration.</param>
    /// <param name="filepath">The filesystem path to the uploaded file.</param>
    /// <param name="culture">Variation language.</param>
    /// <param name="segment">Variation segment.</param>
    /// <remarks>
    /// The <paramref name="filepath" /> parameter is the path relative to the filesystem.
    /// </remarks>
    public void Populate(IContentBase content, ImagingAutoFillUploadField autoFillConfig, string filepath, string? culture, string? segment)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(autoFillConfig);

        // no file = reset, file = auto-fill
        if (filepath.IsNullOrWhiteSpace())
        {
            ResetProperties(content, autoFillConfig, culture, segment);
        }
        else
        {
            // it might not exist if the media item has been created programatically but doesn't have a file persisted yet.
            if (_mediaFileManager.FileSystem.FileExists(filepath))
            {
                // if anything goes wrong, just reset the properties
                try
                {
                    using (Stream filestream = _mediaFileManager.FileSystem.OpenFile(filepath))
                    {
                        SetProperties(content, autoFillConfig, filepath, filestream, culture, segment);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not populate upload auto-fill properties for file '{File}'.", filepath);
                    ResetProperties(content, autoFillConfig, culture, segment);
                }
            }
        }
    }

    /// <summary>
    /// Populates the auto-fill properties of a content item.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="autoFillConfig">The automatic fill configuration.</param>
    /// <param name="filepath">The filesystem-relative filepath, or null to clear properties.</param>
    /// <param name="filestream">The stream containing the file data.</param>
    /// <param name="culture">Variation language.</param>
    /// <param name="segment">Variation segment.</param>
    public void Populate(IContentBase content, ImagingAutoFillUploadField autoFillConfig, string filepath, Stream filestream, string culture, string segment)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(autoFillConfig);

        // no file = reset, file = auto-fill
        if (filepath.IsNullOrWhiteSpace() || filestream == null)
        {
            ResetProperties(content, autoFillConfig, culture, segment);
        }
        else
        {
            SetProperties(content, autoFillConfig, filepath, filestream, culture, segment);
        }
    }

    private static void SetProperties(IContentBase content, ImagingAutoFillUploadField autoFillConfig, Size? size, long? length, string extension, string? culture, string? segment)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(autoFillConfig);

        void SetProperty(string alias, object? value) => UploadAutoFillProperties.SetProperty(content, alias, value, culture, segment);

        SetProperty(autoFillConfig.WidthFieldAlias, size.HasValue ? size.Value.Width.ToInvariantString() : null);
        SetProperty(autoFillConfig.HeightFieldAlias, size.HasValue ? size.Value.Height.ToInvariantString() : null);
        SetProperty(autoFillConfig.LengthFieldAlias, length);
        SetProperty(autoFillConfig.ExtensionFieldAlias, extension);
    }

    private void SetProperties(IContentBase content, ImagingAutoFillUploadField autoFillConfig, string filepath, Stream filestream, string? culture, string? segment)
    {
        var extension = (Path.GetExtension(filepath) ?? string.Empty).TrimStart(Constants.CharArrays.Period);

        Size? size = _imageDimensionExtractor.IsSupportedImageFormat(extension)
            ? _imageDimensionExtractor.GetDimensions(filestream) ?? new Size(Constants.Conventions.Media.DefaultSize, Constants.Conventions.Media.DefaultSize)
            : null;

        SetProperties(content, autoFillConfig, size, filestream?.Length, extension, culture, segment);
    }

    private static void ResetProperties(IContentBase content, ImagingAutoFillUploadField autoFillConfig, string? culture, string? segment)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(autoFillConfig);

        void ResetProperty(string alias) => SetProperty(content, alias, null, culture, segment);

        ResetProperty(autoFillConfig.WidthFieldAlias);
        ResetProperty(autoFillConfig.HeightFieldAlias);
        ResetProperty(autoFillConfig.LengthFieldAlias);
        ResetProperty(autoFillConfig.ExtensionFieldAlias);
    }

    private static void SetProperty(IContentBase content, string alias, object? value, string? culture, string? segment)
    {
        if (!string.IsNullOrEmpty(alias) &&
            content.Properties.TryGetValue(alias, out var property))
        {
            property.SetValue(value, culture, segment);
        }
    }
}
