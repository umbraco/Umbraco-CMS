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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContent, result.Result);
    }

    [Test]
    public void CanResolveMediaRecycleBinIdFromKey()
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinMediaKey, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMedia, result.Result);
    }

    [Test]
    public void CanResolveContentRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContentKey, result.Result);
    }

    [Test]
    public void CanResolveMediaRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinMedia, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMediaKey, result.Result);
    }
}
