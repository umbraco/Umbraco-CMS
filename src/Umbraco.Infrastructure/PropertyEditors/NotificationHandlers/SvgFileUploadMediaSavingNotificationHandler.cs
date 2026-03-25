using System.Drawing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Notification handler to set width / height of uploaded SVG media
/// </summary>
internal sealed class SvgFileUploadMediaSavingNotificationHandler : INotificationHandler<MediaSavingNotification>
{
    private readonly ISvgDimensionExtractor _svgDimensionExtractor;
    private readonly MediaFileManager _mediaFileManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SvgFileUploadMediaSavingNotificationHandler"/> class.
    /// </summary>
    /// <param name="svgDimensionExtractor"></param>
    /// <param name="mediaFileManager"></param>
    public SvgFileUploadMediaSavingNotificationHandler(
        ISvgDimensionExtractor svgDimensionExtractor,
        MediaFileManager mediaFileManager)
    {
        _svgDimensionExtractor = svgDimensionExtractor;
        _mediaFileManager = mediaFileManager;
    }

    /// <summary>
    /// Implementation that sets width / height for uploaded SVGs.
    /// </summary>
    /// <param name="notification"></param>
    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            if (!entity.ContentType.Alias.Equals(Core.Constants.Conventions.MediaTypes.VectorGraphicsAlias))
            {
                continue;
            }

            AutoFillSvgWidthHeight(entity);
        }
    }

    private void AutoFillSvgWidthHeight(IContentBase model)
    {
        IProperty? property = model.Properties
            .FirstOrDefault(x => x.PropertyType.PropertyEditorAlias == Core.Constants.PropertyEditors.Aliases.UploadField);

        if (property is null)
        {
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

            var extension = (Path.GetExtension(filepath) ?? string.Empty).TrimStart(Core.Constants.CharArrays.Period);
            if (!_svgDimensionExtractor.SupportedImageFileTypes.Contains(extension))
            {
                continue;
            }

            if (!_mediaFileManager.FileSystem.FileExists(filepath))
            {
                continue;
            }

            using (Stream filestream = _mediaFileManager.FileSystem.OpenFile(filepath))
            {
                SetWidthAndHeight(model, filestream);
            }

        }

    }

    private void SetWidthAndHeight(IContentBase model, Stream filestream)
    {
        Size? size = _svgDimensionExtractor.GetDimensions(filestream);
        if (size.HasValue)
        {
            SetProperty(model, Core.Constants.Conventions.Media.Width,size.Value.Width);
            SetProperty(model, Core.Constants.Conventions.Media.Height,size.Value.Height);
        }
    }

    private static void SetProperty(IContentBase content, string alias, object? value)
    {
        if (!string.IsNullOrEmpty(alias) &&
            content.Properties.TryGetValue(alias, out IProperty? property))
        {
            property.SetValue(value);
        }
    }

}
