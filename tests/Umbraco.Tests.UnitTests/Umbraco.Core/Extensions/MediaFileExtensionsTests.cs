using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
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
    public void GetFileExtension_Reads_The_Extension_From_The_File_Path(string? mediaPath, string? expected)
        => Assert.AreEqual(expected, new MediaEntitySlim { MediaPath = mediaPath }.GetFileExtension());

    [Test]
    public void GetFileExtension_Comes_From_The_File_Path_Not_The_Item_Name()
    {
        var entity = new MediaEntitySlim { MediaPath = "/media/abc123/file.pdf", Name = "Version 2.0 mockup" };

        Assert.AreEqual("pdf", entity.GetFileExtension());
    }

    [TestCase("jpg", "jpg")]
    [TestCase("JPG", "jpg")]
    [TestCase(".pdf", "pdf")]
    [TestCase("", null)]
    [TestCase(null, null)]
    public void GetFileExtension_Normalizes_The_Media_Items_Own_Extension_Property(string? stored, string? expected)
    {
        var media = new Mock<IMedia>();
        media.Setup(x => x.GetValue<string>(Constants.Conventions.Media.Extension, null, null, false)).Returns(stored);

        Assert.AreEqual(expected, media.Object.GetFileExtension());
    }
}
