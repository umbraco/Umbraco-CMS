using NUnit.Framework;
using Umbraco.Cms.Core.IO.MediaPathSchemes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.IO.MediaPathSchemes;

[TestFixture]
public class UniqueMediaPathSchemeTests
{
    [Test]
    public void GetFilePath_Creates_ExpectedPath()
    {
        var scheme = new UniqueMediaPathScheme();
        var itemGuid = new Guid("00000000-0000-4000-0000-000000000001");
        var propertyGuid = new Guid("00000000-0000-4000-0000-000000000002");
        var filename = "test.txt";
        string actualPath = scheme.GetFilePath(null, itemGuid, propertyGuid, filename);
        Assert.AreEqual("aaaaaaaa/test.txt", actualPath);
    }

    [Test]
    public void GetFilePath_ShouldThrow_WhenUsingVersion7Guids()
    {
        var scheme = new UniqueMediaPathScheme();
        var itemGuid = new Guid("00000000-0000-7000-0000-000000000001");
        var propertyGuid = new Guid("00000000-0000-4000-0000-000000000002");
        var filename = "test.txt";

        Assert.Throws<InvalidOperationException>(() => scheme.GetFilePath(null, itemGuid, propertyGuid, filename));
    }
}
