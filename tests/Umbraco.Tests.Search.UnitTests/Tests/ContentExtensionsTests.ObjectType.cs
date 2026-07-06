using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;

namespace Umbraco.Tests.Search.UnitTests.Tests;

public partial class ContentExtensionsTests
{
    [Test]
    public void ObjectType_ReturnsDocument_ForContent()
    {
        UmbracoObjectTypes objectType = Mock.Of<IContent>().ObjectType();
        Assert.That(objectType, Is.EqualTo(UmbracoObjectTypes.Document));
    }

    [Test]
    public void ObjectType_ReturnsMedia_ForMedia()
    {
        UmbracoObjectTypes objectType = Mock.Of<IMedia>().ObjectType();
        Assert.That(objectType, Is.EqualTo(UmbracoObjectTypes.Media));
    }

    [Test]
    public void ObjectType_ReturnsMedia_ForMembers()
    {
        UmbracoObjectTypes objectType = Mock.Of<IMember>().ObjectType();
        Assert.That(objectType, Is.EqualTo(UmbracoObjectTypes.Member));
    }

    [Test]
    public void ObjectType_Throws_ForOtherTypes()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Mock.Of<IMyContent>().ObjectType());

    public interface IMyContent : IContentBase
    {
    }
}
