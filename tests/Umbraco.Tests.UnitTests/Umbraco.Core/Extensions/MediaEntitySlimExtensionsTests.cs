using NUnit.Framework;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class MediaEntitySlimExtensionsTests
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
}
