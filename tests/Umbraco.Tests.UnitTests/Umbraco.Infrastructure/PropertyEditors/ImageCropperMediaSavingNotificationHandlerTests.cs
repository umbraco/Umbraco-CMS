using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class ImageCropperMediaSavingNotificationHandlerTests : MediaSavingMetadataHandlerTestsBase
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.ImageCropper;

    protected override INotificationHandler<MediaSavingNotification> CreateHandler() =>
        new ImageCropperPropertyEditor(Mock.Of<IDataValueEditorFactory>(), NullLoggerFactory.Instance, FileManager, Settings,
            Mock.Of<IIOHelper>(), AutoFill, Mock.Of<IContentService>(), Serializer);
}
