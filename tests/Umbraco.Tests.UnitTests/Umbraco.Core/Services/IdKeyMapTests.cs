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
    public async Task CanResolveContentRecycleBinIdFromKey()
    {
        var result = await GetSubject().GetIdForKeyAsync(Constants.System.RecycleBinContentKey, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContent, result.Result);
    }

    [Test]
    public async Task CanResolveMediaRecycleBinIdFromKey()
    {
        var result = await GetSubject().GetIdForKeyAsync(Constants.System.RecycleBinMediaKey, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMedia, result.Result);
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public async Task CanResolveElementRecycleBinIdFromKey(UmbracoObjectTypes objectType)
    {
        var result = await GetSubject().GetIdForKeyAsync(Constants.System.RecycleBinElementKey, objectType);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinElement, result.Result);
    }

    [Test]
    public async Task CanResolveContentRecycleBinKeyFromId()
    {
        var result = await GetSubject().GetKeyForIdAsync(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContentKey, result.Result);
    }

    [Test]
    public async Task CanResolveMediaRecycleBinKeyFromId()
    {
        var result = await GetSubject().GetKeyForIdAsync(Constants.System.RecycleBinMedia, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMediaKey, result.Result);
    }

    [TestCase(UmbracoObjectTypes.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer)]
    public async Task CanResolveElementRecycleBinKeyFromId(UmbracoObjectTypes objectType)
    {
        var result = await GetSubject().GetKeyForIdAsync(Constants.System.RecycleBinElement, objectType);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinElementKey, result.Result);
    }
}
