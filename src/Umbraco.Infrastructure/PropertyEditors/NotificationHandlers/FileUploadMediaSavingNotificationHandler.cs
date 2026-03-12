using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Implements a notification handler that processes file uploads media is saved, completing properties on the media item.
/// </summary>
public class FileUploadMediaSavingNotificationHandler : FileUploadNotificationHandlerBase, INotificationHandler<MediaSavingNotification>
{
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly UploadAutoFillProperties _uploadAutoFillProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadMediaSavingNotificationHandler"/> class.
    /// </summary>
    public FileUploadMediaSavingNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        IOptionsMonitor<ContentSettings> contentSettings,
        UploadAutoFillProperties uploadAutoFillProperties)
        : base(jsonSerializer, mediaFileManager, elementTypeCache)
    {
        _contentSettings = contentSettings;
        _uploadAutoFillProperties = uploadAutoFillProperties;
    }

    /// <inheritdoc/>
    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            AutoFillProperties(entity);
        }
    }

    private void AutoFillProperties(IContentBase model)
    {
        IEnumerable<IProperty> properties = model.Properties.Where(x => IsUploadFieldPropertyType(x.PropertyType));

        foreach (IProperty property in properties)
        {
            ImagingAutoFillUploadField? autoFillConfig = _contentSettings.CurrentValue.GetConfig(property.Alias);
            if (autoFillConfig == null)
            {
                continue;
            }

            foreach (IPropertyValue pvalue in property.Values)
            {
                var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                if (string.IsNullOrWhiteSpace(svalue))
                {
                    _uploadAutoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                }
                else
                {
                    _uploadAutoFillProperties.Populate(
                        model,
                        autoFillConfig,
                        MediaFileManager.FileSystem.GetRelativePath(svalue),
                        pvalue.Culture,
                        pvalue.Segment);
                }
            }
        }
    }
}
