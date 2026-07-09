using System.Drawing;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Notification handler to set width / height of uploaded SVG media.
/// </summary>
internal sealed class SvgFileUploadMediaSavingNotificationHandler : INotificationHandler<MediaSavingNotification>
{
    private readonly ILogger<SvgFileUploadMediaSavingNotificationHandler> _logger;
    private readonly ISvgDimensionExtractor _svgDimensionExtractor;
    private readonly MediaFileManager _mediaFileManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SvgFileUploadMediaSavingNotificationHandler"/> class.
    /// </summary>
    public SvgFileUploadMediaSavingNotificationHandler(
        ILogger<SvgFileUploadMediaSavingNotificationHandler> logger,
        ISvgDimensionExtractor svgDimensionExtractor,
        MediaFileManager mediaFileManager)
    {
        _logger = logger;
        _svgDimensionExtractor = svgDimensionExtractor;
        _mediaFileManager = mediaFileManager;
    }

    /// <inheritdoc/>
    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            if (entity.ContentType.Alias.Equals(Core.Constants.Conventions.MediaTypes.VectorGraphicsAlias) is false)
            {
                continue;
            }

            AutoFillSvgWidthHeight(entity);
        }
    }

    private void AutoFillSvgWidthHeight(IContentBase model)
    {
        if (model.Properties.TryGetValue(Core.Constants.Conventions.Media.Width, out _) is false
            || model.Properties.TryGetValue(Core.Constants.Conventions.Media.Height, out _) is false)
        {
            _logger.LogDebug("Skipping SVG dimension extraction for media {MediaId}: width/height properties not found on content type.", model.Id);
            return;
        }

        IProperty? property = model.Properties
            .FirstOrDefault(x => x.PropertyType.PropertyEditorAlias == Core.Constants.PropertyEditors.Aliases.UploadField);

        if (property is null)
        {
            _logger.LogDebug("Skipping SVG dimension extraction for media {MediaId}: no upload field property found.", model.Id);
            return;
        }

        foreach (IPropertyValue pvalue in property.Values)
        {
            var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                continue;
            }

            string filepath = _mediaFileManager.FileSystem.GetRelativePath(svalue);

            if (_mediaFileManager.FileSystem.FileExists(filepath) is false)
            {
                _logger.LogWarning("SVG file not found at path {FilePath} for media {MediaId}.", filepath, model.Id);
                continue;
            }

            using Stream filestream = _mediaFileManager.FileSystem.OpenFile(filepath);

            SetWidthAndHeight(model, filestream, pvalue.Culture, pvalue.Segment);
        }
    }

    private void SetWidthAndHeight(IContentBase model, Stream filestream, string? culture, string? segment)
    {
        Size? size = _svgDimensionExtractor.GetDimensions(filestream);
        if (size.HasValue)
        {
            _logger.LogDebug("Extracted SVG dimensions {Width}x{Height} for media {MediaId}.", size.Value.Width, size.Value.Height, model.Id);
            SetProperty(model, Core.Constants.Conventions.Media.Width, size.Value.Width, culture, segment);
            SetProperty(model, Core.Constants.Conventions.Media.Height, size.Value.Height, culture, segment);
        }
        else
        {
            _logger.LogDebug("Could not extract dimensions from SVG for media {MediaId}.", model.Id);
        }
    }

    private static void SetProperty(IContentBase content, string alias, object? value, string? culture, string? segment)
    {
        if (string.IsNullOrEmpty(alias) is false &&
            content.Properties.TryGetValue(alias, out IProperty? property))
        {
            property.SetValue(value, culture, segment);
        }
    }
}
