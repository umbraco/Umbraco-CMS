using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Implements a notification handler that processes file uploads when content is deleted, removing associated files.
/// </summary>
internal sealed class FileUploadContentDeletedNotificationHandler : FileUploadEntityDeletedNotificationHandlerBase, INotificationHandler<ContentDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadContentDeletedNotificationHandler"/> class.
    /// </summary>
    public FileUploadContentDeletedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentDeletedNotificationHandler> logger)
        : base(jsonSerializer, mediaFileManager, elementTypeCache, logger)
    {
    }

    /// <inheritdoc/>
    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);
}
