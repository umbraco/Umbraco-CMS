using System.Drawing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadMediaSavingNotificationHandlerTests
{
    [TestCase(true, Constants.PropertyEditors.Aliases.UploadField)]
    [TestCase(false, Constants.PropertyEditors.Aliases.UploadField)]
    [TestCase(true, Constants.PropertyEditors.Aliases.ImageCropper)]
    [TestCase(false, Constants.PropertyEditors.Aliases.ImageCropper)]
    public void Only_Ordinary_Saves_Read_File_Metadata(bool isSorting, string editorAlias)
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        var mediaType = new MediaType(helper, -1);
        mediaType.AddPropertyType(new PropertyType(helper, editorAlias, ValueStorageType.Ntext, Constants.Conventions.Media.File));
        foreach (var alias in new[] { Constants.Conventions.Media.Width, Constants.Conventions.Media.Height, Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension })
        {
            mediaType.AddPropertyType(new PropertyType(helper, Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar, alias));
        }

        var media = new global::Umbraco.Cms.Core.Models.Media("Document", -1, mediaType) { Id = 123 };
        media.SetValue(Constants.Conventions.Media.File, "/media/document.pdf");
        media.SetValue(Constants.Conventions.Media.Width, "10");
        media.SetValue(Constants.Conventions.Media.Height, "20");
        media.SetValue(Constants.Conventions.Media.Bytes, "30");
        media.SetValue(Constants.Conventions.Media.Extension, "pdf");
        media.ResetDirtyProperties();

        var fileSystem = new Mock<IFileSystem>();
        fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("document.pdf");
        fileSystem.Setup(x => x.FileExists("document.pdf")).Returns(true);
        fileSystem.Setup(x => x.OpenFile("document.pdf")).Returns(() => new MemoryStream(new byte[50]));
        var dimensions = new Mock<IImageDimensionExtractor>();
        dimensions.SetupGet(x => x.SupportedImageFileTypes).Returns(new[] { "pdf" });
        dimensions.Setup(x => x.GetDimensions(It.IsAny<Stream>())).Returns(new Size(100, 200));
        var manager = new MediaFileManager(fileSystem.Object, Mock.Of<IMediaPathScheme>(), NullLogger<MediaFileManager>.Instance,
            helper, Mock.Of<IServiceProvider>(), new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));
        var options = new Mock<IOptionsMonitor<ContentSettings>>();
        options.SetupGet(x => x.CurrentValue).Returns(new ContentSettings());
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var autoFill = new UploadAutoFillProperties(manager, NullLogger<UploadAutoFillProperties>.Instance, dimensions.Object);
        INotificationHandler<MediaSavingNotification> handler = editorAlias == Constants.PropertyEditors.Aliases.UploadField
            ? new FileUploadMediaSavingNotificationHandler(serializer, manager, Mock.Of<IBlockEditorElementTypeCache>(), options.Object, autoFill)
            : new ImageCropperPropertyEditor(Mock.Of<IDataValueEditorFactory>(), NullLoggerFactory.Instance, manager, options.Object,
                Mock.Of<IIOHelper>(), autoFill, Mock.Of<IContentService>(), serializer);
        handler.Handle(new MediaSavingNotification(media, new EventMessages()) { IsSorting = isSorting });

        fileSystem.Verify(x => x.OpenFile(It.IsAny<string>()), isSorting ? Times.Never() : Times.Once());
        dimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), isSorting ? Times.Never() : Times.Once());
        Assert.Multiple(() =>
        {
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Width), Is.EqualTo(isSorting ? 10 : 100));
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Height), Is.EqualTo(isSorting ? 20 : 200));
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Bytes), Is.EqualTo(isSorting ? 30 : 50));
            Assert.That(media.GetValue<string>(Constants.Conventions.Media.Extension), Is.EqualTo("pdf"));
        });
    }
}
