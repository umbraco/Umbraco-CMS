using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class SvgFileUploadMediaSavingNotificationHandlerTests
{
    private Mock<ISvgDimensionExtractor> _svgDimensionExtractor = null!;
    private Mock<IFileSystem> _fileSystem = null!;
    private SvgFileUploadMediaSavingNotificationHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _svgDimensionExtractor = new Mock<ISvgDimensionExtractor>();
        _fileSystem = new Mock<IFileSystem>();

        var mediaFileManager = new MediaFileManager(
            _fileSystem.Object,
            Mock.Of<IMediaPathScheme>(),
            NullLogger<MediaFileManager>.Instance,
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IServiceProvider>(),
            new Lazy<ICoreScopeProvider>(() => Mock.Of<ICoreScopeProvider>()));

        _handler = new SvgFileUploadMediaSavingNotificationHandler(
            NullLogger<SvgFileUploadMediaSavingNotificationHandler>.Instance,
            _svgDimensionExtractor.Object,
            mediaFileManager);
    }

    [Test]
    public void Skips_Non_Vector_Graphics_Media_Type()
    {
        IMedia media = CreateMedia(Constants.Conventions.MediaTypes.Image);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Width_Property_Missing()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            hasWidthProperty: false,
            hasHeightProperty: true);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Height_Property_Missing()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            hasWidthProperty: true,
            hasHeightProperty: false);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_No_Upload_Field_Property()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadFieldAlias: null);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Upload_Value_Is_Empty()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: string.Empty);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_File_Does_Not_Exist()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: "/media/test.svg");

        _fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        _fileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(false);

        _handler.Handle(CreateNotification(media));

        _svgDimensionExtractor.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Sets_Width_And_Height_When_Dimensions_Extracted()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: "/media/test.svg",
            widthProperty: out var widthProperty,
            heightProperty: out var heightProperty);

        var svgStream = new MemoryStream(Encoding.UTF8.GetBytes("<svg/>"));

        _fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        _fileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        _fileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        _svgDimensionExtractor.Setup(x => x.GetDimensions(svgStream)).Returns(new Size(200, 100));

        _handler.Handle(CreateNotification(media));

        widthProperty.Verify(x => x.SetValue(200, null, null), Times.Once);
        heightProperty.Verify(x => x.SetValue(100, null, null), Times.Once);
    }

    [Test]
    public void Does_Not_Set_Properties_When_Dimensions_Not_Extracted()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: "/media/test.svg",
            widthProperty: out var widthProperty,
            heightProperty: out var heightProperty);

        var svgStream = new MemoryStream(Encoding.UTF8.GetBytes("<svg/>"));

        _fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        _fileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        _fileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        _svgDimensionExtractor.Setup(x => x.GetDimensions(svgStream)).Returns((Size?)null);

        _handler.Handle(CreateNotification(media));

        widthProperty.Verify(x => x.SetValue(It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        heightProperty.Verify(x => x.SetValue(It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public void Passes_Culture_And_Segment_To_SetValue()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: "/media/test.svg",
            culture: "en-US",
            segment: "seg1",
            widthProperty: out var widthProperty,
            heightProperty: out var heightProperty);

        var svgStream = new MemoryStream(Encoding.UTF8.GetBytes("<svg/>"));

        _fileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        _fileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        _fileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        _svgDimensionExtractor.Setup(x => x.GetDimensions(svgStream)).Returns(new Size(200, 100));

        _handler.Handle(CreateNotification(media));

        widthProperty.Verify(x => x.SetValue(200, "en-US", "seg1"), Times.Once);
        heightProperty.Verify(x => x.SetValue(100, "en-US", "seg1"), Times.Once);
    }

    private static MediaSavingNotification CreateNotification(IMedia media)
        => new(media, new EventMessages());

    private static IMedia CreateMedia(
        string contentTypeAlias,
        bool hasWidthProperty = true,
        bool hasHeightProperty = true,
        string? uploadFieldAlias = Constants.PropertyEditors.Aliases.UploadField,
        string? uploadValue = null,
        string? culture = null,
        string? segment = null) =>
        CreateMedia(
            contentTypeAlias,
            hasWidthProperty,
            hasHeightProperty,
            uploadFieldAlias,
            uploadValue,
            culture,
            segment,
            out _,
            out _);

    private static IMedia CreateMedia(
        string contentTypeAlias,
        string? uploadValue,
        out Mock<IProperty> widthProperty,
        out Mock<IProperty> heightProperty,
        string? culture = null,
        string? segment = null) =>
        CreateMedia(
            contentTypeAlias,
            hasWidthProperty: true,
            hasHeightProperty: true,
            uploadFieldAlias: Constants.PropertyEditors.Aliases.UploadField,
            uploadValue: uploadValue,
            culture: culture,
            segment: segment,
            widthProperty: out widthProperty,
            heightProperty: out heightProperty);

    private static IMedia CreateMedia(
        string contentTypeAlias,
        bool hasWidthProperty,
        bool hasHeightProperty,
        string? uploadFieldAlias,
        string? uploadValue,
        string? culture,
        string? segment,
        out Mock<IProperty> widthProperty,
        out Mock<IProperty> heightProperty)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.Setup(x => x.Alias).Returns(contentTypeAlias);

        var properties = new Mock<IPropertyCollection>();

        // Width property.
        widthProperty = new Mock<IProperty>();
        IProperty? widthOut = widthProperty.Object;
        properties
            .Setup(x => x.TryGetValue(Constants.Conventions.Media.Width, out widthOut))
            .Returns(hasWidthProperty);

        // Height property.
        heightProperty = new Mock<IProperty>();
        IProperty? heightOut = heightProperty.Object;
        properties
            .Setup(x => x.TryGetValue(Constants.Conventions.Media.Height, out heightOut))
            .Returns(hasHeightProperty);

        // Upload field property.
        if (uploadFieldAlias is not null)
        {
            var uploadPropertyType = new Mock<IPropertyType>();
            uploadPropertyType.Setup(x => x.PropertyEditorAlias).Returns(uploadFieldAlias);

            var uploadProperty = new Mock<IProperty>();
            uploadProperty.Setup(x => x.PropertyType).Returns(uploadPropertyType.Object);
            uploadProperty.Setup(x => x.GetValue(culture, segment)).Returns(uploadValue);
            uploadProperty.Setup(x => x.Values).Returns(new List<IPropertyValue>
            {
                CreatePropertyValue(culture, segment),
            });

            properties
                .Setup(x => x.GetEnumerator())
                .Returns(() => new List<IProperty> { uploadProperty.Object }.GetEnumerator());
        }
        else
        {
            properties
                .Setup(x => x.GetEnumerator())
                .Returns(() => new List<IProperty>().GetEnumerator());
        }

        var media = new Mock<IMedia>();
        media.Setup(x => x.ContentType).Returns(contentType.Object);
        media.Setup(x => x.Properties).Returns(properties.Object);

        return media.Object;
    }

    private static IPropertyValue CreatePropertyValue(string? culture, string? segment)
    {
        var propertyValue = new Mock<IPropertyValue>();
        propertyValue.Setup(x => x.Culture).Returns(culture);
        propertyValue.Setup(x => x.Segment).Returns(segment);
        return propertyValue.Object;
    }
}
