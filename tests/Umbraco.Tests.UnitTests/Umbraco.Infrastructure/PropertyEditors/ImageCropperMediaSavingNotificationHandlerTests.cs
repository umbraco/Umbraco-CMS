using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class ImageCropperMediaSavingNotificationHandlerTests : MediaSavingMetadataHandlerTestsBase
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.ImageCropper;

    protected override INotificationHandler<MediaSavingNotification> CreateHandler(
        MediaFileManager manager,
        IOptionsMonitor<ContentSettings> settings,
        IJsonSerializer serializer,
        UploadAutoFillProperties autoFill,
        ISvgDimensionExtractor svgDimensions) =>
        new ImageCropperPropertyEditor(Mock.Of<IDataValueEditorFactory>(), NullLoggerFactory.Instance, manager, settings,
            Mock.Of<IIOHelper>(), autoFill, Mock.Of<IContentService>(), serializer);
}
