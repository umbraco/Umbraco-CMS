using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Implements a notification handler that processes file uploads when media is deleted, removing associated files.
/// </summary>
internal sealed class FileUploadMediaDeletedNotificationHandler : FileUploadEntityDeletedNotificationHandlerBase, INotificationHandler<MediaDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadMediaDeletedNotificationHandler"/> class.
    /// </summary>
    public FileUploadMediaDeletedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentDeletedNotificationHandler> logger)
        : base(jsonSerializer, mediaFileManager, elementTypeCache, logger)
    {
    }

    /// <inheritdoc/>
    public void Handle(MediaDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);
}
