using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class MediaFileExtensionsTests
{
    [TestCase("/media/abc123/holiday-photo.jpg", "jpg")]
    [TestCase("/media/abc123/HOLIDAY-PHOTO.JPG", "jpg")]
    [TestCase("/media/abc123/archive.tar.gz", "gz")]
    [TestCase("/media/abc123/photo.jpg?width=100", "jpg")]
    [TestCase("/media/abc123/README", null)]
    [TestCase("", null)]
    [TestCase(null, null)]
    public void Entity_Reads_The_Extension_From_The_File_Path(string? mediaPath, string? expected)
        => Assert.AreEqual(expected, new MediaEntitySlim { MediaPath = mediaPath }.GetFileExtension());

    [Test]
    public void Entity_Reads_The_File_Path_Not_The_Item_Name()
    {
        var entity = new MediaEntitySlim { MediaPath = "/media/abc123/file.pdf", Name = "Version 2.0 mockup" };

        Assert.AreEqual("pdf", entity.GetFileExtension());
    }

    [TestCase("/media/abc123/holiday-photo.jpg", "jpg")]
    [TestCase("/media/abc123/HOLIDAY-PHOTO.JPG", "jpg")]
    [TestCase("/media/abc123/archive.tar.gz", "gz")]
    [TestCase("/media/abc123/photo.jpg?width=100", "jpg")]
    [TestCase("/media/abc123/README", null)]
    public void Media_Reads_The_Extension_From_The_Stored_File(string mediaPath, string? expected)
        => Assert.AreEqual(expected, CreateMedia(mediaPath).GetFileExtension(CreateMediaUrlGenerators()));

    [Test]
    public void Media_Reads_The_File_Path_Even_Without_An_Extension_Property()
    {
        // A custom media type can carry an upload field without the `umbracoExtension` property. Reading the file
        // keeps such an item labelled the same way everywhere, rather than only where the property happens to exist.
        IMedia media = CreateMedia("/media/abc123/report.pdf");

        Assert.AreEqual("pdf", media.GetFileExtension(CreateMediaUrlGenerators()));
    }

    [Test]
    public void Media_Has_No_Extension_When_It_Holds_No_File()
    {
        IMedia media = CreateMedia(mediaPath: null);

        Assert.IsNull(media.GetFileExtension(CreateMediaUrlGenerators()));
    }

    private static MediaUrlGeneratorCollection CreateMediaUrlGenerators()
    {
        var generators = new List<IMediaUrlGenerator> { new StubMediaUrlGenerator() };

        return new MediaUrlGeneratorCollection(() => generators);
    }

    private static IMedia CreateMedia(string? mediaPath)
    {
        var propertiesMock = new Mock<IPropertyCollection>();

        if (mediaPath is not null)
        {
            IProperty? outProperty = CreateUploadProperty(mediaPath);
            propertiesMock
                .Setup(p => p.TryGetValue(Constants.Conventions.Media.File, out outProperty))
                .Returns(true);
        }

        var mediaMock = new Mock<IMedia>();
        mediaMock.SetupGet(m => m.Properties).Returns(propertiesMock.Object);

        return mediaMock.Object;
    }

    private static IProperty CreateUploadProperty(string mediaPath)
    {
        var propertyTypeMock = new Mock<IPropertyType>();
        propertyTypeMock.SetupGet(pt => pt.PropertyEditorAlias).Returns(Constants.PropertyEditors.Aliases.UploadField);

        var propertyMock = new Mock<IProperty>();
        propertyMock.SetupGet(p => p.Alias).Returns(Constants.Conventions.Media.File);
        propertyMock.SetupGet(p => p.PropertyType).Returns(propertyTypeMock.Object);
        propertyMock.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>())).Returns(mediaPath);

        return propertyMock.Object;
    }

    private sealed class StubMediaUrlGenerator : IMediaUrlGenerator
    {
        public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                mediaPath = stringValue;
                return true;
            }

            mediaPath = null;
            return false;
        }
    }
}
