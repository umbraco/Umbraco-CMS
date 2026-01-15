using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.IO.MediaPathSchemes;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO.MediaPathSchemes;

[TestFixture]
public class UniqueMediaPathSchemeTests
{
    private static MediaFileManager MediaFileManager => new(
        Mock.Of<IFileSystem>(),
        Mock.Of<IMediaPathScheme>(),
        Mock.Of<ILogger<MediaFileManager>>(),
        Mock.Of<IShortStringHelper>(),
        Mock.Of<IServiceProvider>(),
        Mock.Of<Lazy<ICoreScopeProvider>>());

    [Test]
    public void GetFilePath_Creates_ExpectedPath()
    {
        var scheme = new UniqueMediaPathScheme();
        var itemGuid = new Guid("00000000-0000-4000-0000-000000000001");
        var propertyGuid = new Guid("00000000-0000-4000-0000-000000000002");
        var filename = "test.txt";
        string actualPath = scheme.GetFilePath(MediaFileManager, itemGuid, propertyGuid, filename);
        Assert.AreEqual("aaaaaaaa/test.txt", actualPath);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void GetFilePath_ShouldThrow_WhenUsingVersion7Guids(bool userVersion7ForItemGuid, bool userVersion7ForPropertyGuid)
    {
        var scheme = new UniqueMediaPathScheme();
        var itemGuid = new Guid($"00000000-0000-{(userVersion7ForItemGuid ? "7" : "4")}000-0000-000000000001");
        var propertyGuid = new Guid($"00000000-0000-{(userVersion7ForPropertyGuid ? "7" : "4")}000-0000-000000000002");
        var filename = "test.txt";

        Assert.Throws<ArgumentException>(() => scheme.GetFilePath(MediaFileManager, itemGuid, propertyGuid, filename));
    }
}
