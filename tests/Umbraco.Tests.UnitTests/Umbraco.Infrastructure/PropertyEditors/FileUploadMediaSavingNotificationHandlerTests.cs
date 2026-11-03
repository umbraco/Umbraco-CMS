using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadMediaSavingNotificationHandlerTests : MediaSavingMetadataHandlerTestsBase
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.UploadField;

    protected override INotificationHandler<MediaSavingNotification> CreateHandler() =>
        new FileUploadMediaSavingNotificationHandler(
            Serializer,
            FileManager,
            Mock.Of<IBlockEditorElementTypeCache>(),
            Settings,
            AutoFill);
}
