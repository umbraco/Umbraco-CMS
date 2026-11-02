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
    private Mock<IFileSystem> _fileSystem = null!;
    private IMedia _media = null!;

    protected abstract string EditorAlias { get; }

    protected virtual string FileExtension => "pdf";

    protected virtual string MediaTypeAlias => "document";

    protected virtual int PopulatedBytes => 50;

    protected MediaFileManager FileManager { get; private set; } = null!;

    protected IOptionsMonitor<ContentSettings> Settings { get; private set; } = null!;

    protected IJsonSerializer Serializer { get; private set; } = null!;

    protected UploadAutoFillProperties AutoFill { get; private set; } = null!;

    protected Mock<IImageDimensionExtractor> Dimensions { get; private set; } = null!;

    protected Mock<ISvgDimensionExtractor> SvgDimensions { get; private set; } = null!;

    private string FileName => $"document.{FileExtension}";

    protected abstract INotificationHandler<MediaSavingNotification> CreateHandler();

    [SetUp]
    public void SetUpMetadataTest()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        _media = CreateMedia(helper);
        _fileSystem = new Mock<IFileSystem>();
        _fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns(FileName);
        _fileSystem.Setup(x => x.FileExists(FileName)).Returns(true);
        _fileSystem.Setup(x => x.OpenFile(FileName)).Returns(() => new MemoryStream(new byte[50]));
        Dimensions = new Mock<IImageDimensionExtractor>();
        Dimensions.SetupGet(x => x.SupportedImageFileTypes).Returns(new[] { FileExtension });
        Dimensions.Setup(x => x.GetDimensions(It.IsAny<Stream>())).Returns(new Size(100, 200));
        SvgDimensions = new Mock<ISvgDimensionExtractor>();
        SvgDimensions.Setup(x => x.GetDimensions(It.IsAny<Stream>())).Returns(new Size(100, 200));
        FileManager = new MediaFileManager(_fileSystem.Object, Mock.Of<IMediaPathScheme>(), NullLogger<MediaFileManager>.Instance,
            helper, Mock.Of<IServiceProvider>(), new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));
        var options = new Mock<IOptionsMonitor<ContentSettings>>();
        options.SetupGet(x => x.CurrentValue).Returns(new ContentSettings());
        Settings = options.Object;
        Serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        AutoFill = new UploadAutoFillProperties(FileManager, NullLogger<UploadAutoFillProperties>.Instance, Dimensions.Object);
    }

    [Test]
    public void Sorting_Does_Not_Read_File_Metadata()
    {
        CreateHandler().Handle(new MediaSavingNotification(_media, new EventMessages()) { IsSorting = true });

        _fileSystem.Verify(x => x.OpenFile(It.IsAny<string>()), Times.Never());
        Dimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never());
        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never());
        AssertMetadata(10, 20, 30);
    }

    [Test]
    public void Ordinary_Save_Reads_File_Metadata()
    {
        CreateHandler().Handle(new MediaSavingNotification(_media, new EventMessages()));

        _fileSystem.Verify(x => x.OpenFile(It.IsAny<string>()), Times.Once());
        VerifyDimensions(Times.Once());
        AssertMetadata(100, 200, PopulatedBytes);
    }

    protected virtual void VerifyDimensions(Times times)
    {
        Dimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), times);
        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never());
    }

    private IMedia CreateMedia(IShortStringHelper helper)
    {
        var mediaType = new MediaType(helper, -1) { Alias = MediaTypeAlias };
        mediaType.AddPropertyType(new PropertyType(helper, EditorAlias, ValueStorageType.Ntext, Constants.Conventions.Media.File));
        foreach (var alias in new[] { Constants.Conventions.Media.Width, Constants.Conventions.Media.Height, Constants.Conventions.Media.Bytes, Constants.Conventions.Media.Extension })
        {
            mediaType.AddPropertyType(new PropertyType(helper, Constants.PropertyEditors.Aliases.Label, ValueStorageType.Nvarchar, alias));
        }

        var media = new global::Umbraco.Cms.Core.Models.Media("Document", -1, mediaType) { Id = 123 };
        media.SetValue(Constants.Conventions.Media.File, $"/media/{FileName}");
        media.SetValue(Constants.Conventions.Media.Width, "10");
        media.SetValue(Constants.Conventions.Media.Height, "20");
        media.SetValue(Constants.Conventions.Media.Bytes, "30");
        media.SetValue(Constants.Conventions.Media.Extension, FileExtension);
        media.ResetDirtyProperties();
        return media;
    }

    private void AssertMetadata(int width, int height, int bytes)
    {
        Assert.Multiple(() =>
        {
            Assert.That(_media.GetValue<int>(Constants.Conventions.Media.Width), Is.EqualTo(width));
            Assert.That(_media.GetValue<int>(Constants.Conventions.Media.Height), Is.EqualTo(height));
            Assert.That(_media.GetValue<int>(Constants.Conventions.Media.Bytes), Is.EqualTo(bytes));
            Assert.That(_media.GetValue<string>(Constants.Conventions.Media.Extension), Is.EqualTo(FileExtension));
        });
    }
}
