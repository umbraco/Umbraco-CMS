using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Implements a notification handler that processes file uploads when a member is deleted, removing associated files.
/// </summary>
internal sealed class FileUploadMemberDeletedNotificationHandler : FileUploadEntityDeletedNotificationHandlerBase, INotificationHandler<MemberDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadMemberDeletedNotificationHandler"/> class.
    /// </summary>
    public FileUploadMemberDeletedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentDeletedNotificationHandler> logger)
        : base(jsonSerializer, mediaFileManager, elementTypeCache, logger)
    {
    }

    /// <inheritdoc/>
    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);
}
