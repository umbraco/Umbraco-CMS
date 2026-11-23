using NUnit.Framework;
using Umbraco.Cms.Infrastructure.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadSourceTests
{
    [TestCase("/media/abc123/image.jpg")]
    [TestCase("/media/abc123/my image.jpg")]
    [TestCase("/media/abc123/image.jpg?v=1")]
    [TestCase("/media/abc123/bild-æøå.jpg")]
    [TestCase("https://cdn.example.com/media/abc123/image.jpg")]
    [TestCase("http://cdn.example.com/media/abc123/image.jpg")]
    public void IsWellFormed_Accepts_Root_Relative_Paths_And_Web_Urls(string source)
        => Assert.IsTrue(FileUploadSource.IsWellFormed(source));

    [TestCase("@attacker.example.com/media/abc123/image.jpg")]
    [TestCase("//attacker.example.com/media/abc123/image.jpg")]
    [TestCase("/\\attacker.example.com/media/abc123/image.jpg")]
    [TestCase("media/abc123/image.jpg")]
    [TestCase("javascript:alert(1)")]
    [TestCase("data:image/svg+xml,<svg onload=alert(1)>")]
    [TestCase("file:///etc/passwd")]
    [TestCase("/media/abc123/image.jpg\" ><script src=https://attacker.example.com/x.js></script>.svg")]
    [TestCase("/media/abc123/<img>.jpg")]
    [TestCase("/media/abc123/image.jpg\n")]
    [TestCase("/media/../umbraco/image.jpg")]
    [TestCase("/media/%2e%2e/umbraco/image.jpg")]
    [TestCase("https://cdn.example.com/media/../image.jpg")]
    [TestCase("")]
    [TestCase(" ")]
    public void IsWellFormed_Rejects_Anything_Else(string source)
        => Assert.IsFalse(FileUploadSource.IsWellFormed(source));

    [Test]
    public void IsAcceptable_Accepts_An_Unchanged_Source_Even_When_Not_Well_Formed()
    {
        const string legacySource = "media/1234/legacy image.jpg";

        Assert.IsTrue(FileUploadSource.IsAcceptable(legacySource, legacySource));
    }

    [Test]
    public void IsAcceptable_Rejects_A_Changed_Source_That_Is_Not_Well_Formed()
        => Assert.IsFalse(FileUploadSource.IsAcceptable("javascript:alert(1)", "/media/abc123/image.jpg"));

    [Test]
    public void IsAcceptable_Accepts_A_Changed_Source_That_Is_Well_Formed()
        => Assert.IsTrue(FileUploadSource.IsAcceptable("/media/def456/image.jpg", null));
}
