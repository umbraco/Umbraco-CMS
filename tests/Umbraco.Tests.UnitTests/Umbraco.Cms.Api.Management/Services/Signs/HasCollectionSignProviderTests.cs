using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class HasCollectionSignProviderTests
{
    [Test]
    public void HasCollectionSignProvider_Can_Provide_Document_Tree_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public void HasCollectionSignProvider_Can_Provide_Document_Collection_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentCollectionResponseModel>());
    }

    [Test]
    public void HasCollectionSignProvider_Can_Provide_Document_Item_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentItemResponseModel>());
    }

    [Test]
    public void HasCollectionSignProvider_Can_Provide_Media_Tree_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<MediaTreeItemResponseModel>());
    }

    [Test]
    public void HasCollectionSignProvider_Can_Provide_Media_Collection_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<MediaCollectionResponseModel>());
    }

    [Test]
    public void HasCollectionSignProvider_Can_Provide_Media_Item_Signs()
    {
        var sut = new HasCollectionSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<MediaItemResponseModel>());
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Document_Tree_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Document_Collection_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeCollectionReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Document_Item_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<DocumentItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Media_Tree_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<MediaTreeItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Media_Collection_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<MediaCollectionResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeCollectionReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Media_Item_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<MediaItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);
        Assert.AreEqual(viewModels[1].Signs.Count(), 0);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }
}
