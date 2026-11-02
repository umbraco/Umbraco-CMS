using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadMediaSavingNotificationHandlerTests : MediaSavingMetadataHandlerTestsBase
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.UploadField;

    protected override INotificationHandler<MediaSavingNotification> CreateHandler(
        MediaFileManager manager,
        IOptionsMonitor<ContentSettings> settings,
        IJsonSerializer serializer,
        UploadAutoFillProperties autoFill,
        ISvgDimensionExtractor svgDimensions) =>
        new FileUploadMediaSavingNotificationHandler(serializer, manager, Mock.Of<IBlockEditorElementTypeCache>(), settings, autoFill);
}
