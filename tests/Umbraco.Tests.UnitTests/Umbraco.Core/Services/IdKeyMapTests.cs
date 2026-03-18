using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Contains unit tests for the <see cref="IdKeyMap"/> service in <c>Umbraco.Core.Services</c>.
/// These tests verify the correct mapping between IDs and keys.
/// </summary>
[TestFixture]
public class IdKeyMapTests
{
    private IdKeyMap GetSubject()
        => new IdKeyMap(Mock.Of<ICoreScopeProvider>(), Mock.Of<IIdKeyMapRepository>());

    /// <summary>
    /// Tests that the content recycle bin ID can be resolved correctly from its key.
    /// </summary>
    [Test]
    public void CanResolveContentRecycleBinIdFromKey()
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinContentKey, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContent, result.Result);
    }

    /// <summary>
    /// Tests that the media recycle bin ID can be resolved from its key.
    /// </summary>
    [Test]
    public void CanResolveMediaRecycleBinIdFromKey()
    {
        var result = GetSubject().GetIdForKey(Constants.System.RecycleBinMediaKey, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMedia, result.Result);
    }

    /// <summary>
    /// Tests that the content recycle bin key can be resolved from its ID.
    /// </summary>
    [Test]
    public void CanResolveContentRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinContent, UmbracoObjectTypes.Document);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinContentKey, result.Result);
    }

    /// <summary>
    /// Tests that the media recycle bin key can be resolved from its ID.
    /// </summary>
    [Test]
    public void CanResolveMediaRecycleBinKeyFromId()
    {
        var result = GetSubject().GetKeyForId(Constants.System.RecycleBinMedia, UmbracoObjectTypes.Media);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(Constants.System.RecycleBinMediaKey, result.Result);
    }
}
