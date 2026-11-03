using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class SvgFileUploadMediaSavingNotificationHandlerTests : MediaSavingMetadataHandlerTestsBase
{
    protected override string EditorAlias => Constants.PropertyEditors.Aliases.UploadField;

    protected override string FileExtension => "svg";

    protected override string MediaTypeAlias => Constants.Conventions.MediaTypes.VectorGraphicsAlias;

    protected override int PopulatedBytes => 30;

    protected override INotificationHandler<MediaSavingNotification> CreateHandler() =>
        new SvgFileUploadMediaSavingNotificationHandler(
            NullLogger<SvgFileUploadMediaSavingNotificationHandler>.Instance, SvgDimensions.Object, FileManager);

    protected override void VerifyDimensions(Times times)
    {
        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), times);
        Dimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never());
    }

    [Test]
    public void Skips_Non_Vector_Graphics_Media_Type()
    {
        IMedia media = CreateMedia(Constants.Conventions.MediaTypes.Image);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Width_Property_Missing()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            hasWidthProperty: false,
            hasHeightProperty: true);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Height_Property_Missing()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            hasWidthProperty: true,
            hasHeightProperty: false);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_No_Upload_Field_Property()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadFieldAlias: null);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_Upload_Value_Is_Empty()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: string.Empty);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
    }

    [Test]
    public void Skips_When_File_Does_Not_Exist()
    {
        IMedia media = CreateMedia(
            Constants.Conventions.MediaTypes.VectorGraphicsAlias,
            uploadValue: "/media/test.svg");

        FileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        FileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(false);

        CreateHandler().Handle(CreateNotification(media));

        SvgDimensions.Verify(x => x.GetDimensions(It.IsAny<Stream>()), Times.Never);
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

        FileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        FileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        FileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        SvgDimensions.Setup(x => x.GetDimensions(svgStream)).Returns(new Size(200, 100));

        CreateHandler().Handle(CreateNotification(media));

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

        FileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        FileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        FileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        SvgDimensions.Setup(x => x.GetDimensions(svgStream)).Returns((Size?)null);

        CreateHandler().Handle(CreateNotification(media));

        widthProperty.Verify(
            x => x.SetValue(It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Never);
        heightProperty.Verify(
            x => x.SetValue(It.IsAny<object?>(), It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Never);
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

        FileSystem.Setup(x => x.GetRelativePath(It.IsAny<string>())).Returns("media/test.svg");
        FileSystem.Setup(x => x.FileExists("media/test.svg")).Returns(true);
        FileSystem.Setup(x => x.OpenFile("media/test.svg")).Returns(svgStream);
        SvgDimensions.Setup(x => x.GetDimensions(svgStream)).Returns(new Size(200, 100));

        CreateHandler().Handle(CreateNotification(media));

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
