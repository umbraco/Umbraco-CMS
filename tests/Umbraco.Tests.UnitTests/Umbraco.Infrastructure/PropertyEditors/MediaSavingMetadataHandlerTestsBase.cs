using System.Drawing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

public abstract class MediaSavingMetadataHandlerTestsBase
{
    protected abstract string EditorAlias { get; }

    protected virtual bool IsSvg => false;

    protected abstract INotificationHandler<MediaSavingNotification> CreateHandler(
        MediaFileManager manager,
        IOptionsMonitor<ContentSettings> settings,
        IJsonSerializer serializer,
        UploadAutoFillProperties autoFill,
        ISvgDimensionExtractor svgDimensions);

    [TestCase(true)]
    [TestCase(false)]
    public void Only_Ordinary_Saves_Read_File_Metadata(bool isSorting)
    {
        var extension = IsSvg ? "svg" : "pdf";
        var fileName = $"document.{extension}";
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        var mediaType = new MediaType(helper, -1)
        {
            Alias = IsSvg ? Constants.Conventions.MediaTypes.VectorGraphicsAlias : "document",
        };
        mediaType.AddPropertyType(new PropertyType(helper, EditorAlias, ValueStorageType.Ntext, Constants.Conventions.Media.File));
        foreach (var alias in new[] { Constants.Conventions.Media.Width, Constants.Conventions.Media.Height, Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension })
        {
            mediaType.AddPropertyType(new PropertyType(helper, Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar, alias));
        }

        var media = new global::Umbraco.Cms.Core.Models.Media("Document", -1, mediaType) { Id = 123 };
        media.SetValue(Constants.Conventions.Media.File, $"/media/{fileName}");
        media.SetValue(Constants.Conventions.Media.Width, "10");
        media.SetValue(Constants.Conventions.Media.Height, "20");
        media.SetValue(Constants.Conventions.Media.Bytes, "30");
        media.SetValue(Constants.Conventions.Media.Extension, extension);
        media.ResetDirtyProperties();

        var fileSystem = new Mock<IFileSystem>();
        fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns(fileName);
        fileSystem.Setup(x => x.FileExists(fileName)).Returns(true);
        fileSystem.Setup(x => x.OpenFile(fileName)).Returns(() => new MemoryStream(new byte[50]));
        var dimensions = new Mock<IImageDimensionExtractor>();
        dimensions.SetupGet(x => x.SupportedImageFileTypes).Returns(new[] { extension });
        dimensions.Setup(x => x.GetDimensions(It.IsAny<Stream>())).Returns(new Size(100, 200));
        var svgDimensions = new Mock<ISvgDimensionExtractor>();
        svgDimensions.Setup(x => x.GetDimensions(It.IsAny<Stream>())).Returns(new Size(100, 200));
        var manager = new MediaFileManager(fileSystem.Object, Mock.Of<IMediaPathScheme>(), NullLogger<MediaFileManager>.Instance,
            helper, Mock.Of<IServiceProvider>(), new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));
        var options = new Mock<IOptionsMonitor<ContentSettings>>();
        options.SetupGet(x => x.CurrentValue).Returns(new ContentSettings());
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var autoFill = new UploadAutoFillProperties(manager, NullLogger<UploadAutoFillProperties>.Instance, dimensions.Object);
        var handler = CreateHandler(manager, options.Object, serializer, autoFill, svgDimensions.Object);

        handler.Handle(new MediaSavingNotification(media, new EventMessages()) { IsSorting = isSorting });

        fileSystem.Verify(x => x.OpenFile(It.IsAny<string>()), isSorting ? Times.Never() : Times.Once());
        dimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), isSorting || IsSvg ? Times.Never() : Times.Once());
        svgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), !isSorting && IsSvg ? Times.Once() : Times.Never());
        Assert.Multiple(() =>
        {
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Width), Is.EqualTo(isSorting ? 10 : 100));
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Height), Is.EqualTo(isSorting ? 20 : 200));
            Assert.That(media.GetValue<int>(Constants.Conventions.Media.Bytes), Is.EqualTo(isSorting || IsSvg ? 30 : 50));
            Assert.That(media.GetValue<string>(Constants.Conventions.Media.Extension), Is.EqualTo(extension));
        });
    }
}
