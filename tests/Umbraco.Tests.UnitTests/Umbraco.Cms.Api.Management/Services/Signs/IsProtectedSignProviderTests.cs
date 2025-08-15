using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class IsProtectedSignProviderTests
{
    [Test]
    public async Task IsProtectedSignProvider_Can_Provide_Tree_Signs()
    {
        var sut = new IsProtectedSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public async Task IsProtectedSignProvider_Can_Provide_Collection_Signs()
    {
        var sut = new IsProtectedSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public async Task IsProtectedSignProvider_Can_Provide_Plain_Signs()
    {
        var sut = new IsProtectedSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public async Task IsProtectedSignProvider_Should_Populate_Tree_Signs()
    {
        var sut = new IsProtectedSignProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.IsProtected", signModel.Alias);
    }

    [Test]
    public async Task IsProtectedSignProvider_Should_Populate_Collection_Signs()
    {
        var sut = new IsProtectedSignProvider();

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.IsProtected", signModel.Alias);
    }

    [Test]
    public async Task IsProtectedSignProvider_Should_Populate_Plain_Signs()
    {
        var sut = new IsProtectedSignProvider();

        var viewModels = new List<DocumentItemResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.IsProtected", signModel.Alias);
    }
}
