using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class IdKeyMapTests
{
    private IdKeyMap GetSubject()
        => new IdKeyMap(Mock.Of<ICoreScopeProvider>(), Mock.Of<IIdKeyMapRepository>());

    [Test]
    public void CanResolveContentRecycleBinIdFromKey()
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinContentKey, UmbracoObjectTypes.Document);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinContent));
    }

    [Test]
    public void CanResolveMediaRecycleBinIdFromKey()
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinMediaKey, UmbracoObjectTypes.Media);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinMedia));
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public void CanResolveElementRecycleBinIdFromKey(UmbracoObjectTypes objectType)
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinElementKey, objectType);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinElement));
    }

    [Test]
    public void CanResolveContentRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinContentKey));
    }

    [Test]
    public void CanResolveMediaRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinMedia, UmbracoObjectTypes.Media);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinMediaKey));
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public void CanResolveElementRecycleBinKeyFromId(UmbracoObjectTypes objectType)
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinElement, objectType);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(Constants.System.RecycleBinElementKey));
    }
}
