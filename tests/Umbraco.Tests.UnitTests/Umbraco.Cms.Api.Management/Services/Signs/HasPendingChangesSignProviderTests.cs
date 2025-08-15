using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;



namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class HasPendingChangesSignProviderTests
{
    [Test]
    public async Task HasPendingChangesSignProvider_Can_Provide_Tree_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Can_Provide_Collection_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Can_Provide_Plain_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Tree_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new() { Id = Guid.NewGuid() },
            new()
            {
                Id = Guid.NewGuid(), Variants = new List<DocumentVariantItemResponseModel>
                {
                    new()
                    {
                        State = DocumentVariantState.PublishedPendingChanges,
                        Culture = null,
                        Name = "Test",
                    },
                },
            },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Collection_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new() { Id = Guid.NewGuid() },
            new()
            {
                Id = Guid.NewGuid(), Variants = new List<DocumentVariantResponseModel>
                {
                    new()
                    {
                        State = DocumentVariantState.PublishedPendingChanges,
                        Culture = null,
                        Name = "Test",
                    },
                },
            },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Plain_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var viewModels = new List<DocumentItemResponseModel>
        {
            new() { Id = Guid.NewGuid() },
            new()
            {
                Id = Guid.NewGuid(), Variants = new List<DocumentVariantItemResponseModel>
                {
                    new()
                    {
                        State = DocumentVariantState.PublishedPendingChanges,
                        Culture = null,
                        Name = "Test",
                    },
                },
            },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }
}
