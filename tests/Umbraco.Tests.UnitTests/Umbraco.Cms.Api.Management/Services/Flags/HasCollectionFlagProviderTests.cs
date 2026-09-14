using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasCollectionFlagProviderTests
{
    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Document_Tree_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentTreeItemResponseModel>(), Is.True);
    }

    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Document_Collection_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentCollectionResponseModel>(), Is.True);
    }

    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Document_Item_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentItemResponseModel>(), Is.True);
    }

    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Media_Tree_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<MediaTreeItemResponseModel>(), Is.True);
    }

    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Media_Collection_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<MediaCollectionResponseModel>(), Is.True);
    }

    [Test]
    public void HasCollectionFlagProvider_Can_Provide_Media_Item_Flags()
    {
        var sut = new HasCollectionFlagProvider();
        Assert.That(sut.CanProvideFlags<MediaItemResponseModel>(), Is.True);
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Document_Tree_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Document_Collection_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeCollectionReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Document_Item_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<DocumentItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Media_Tree_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<MediaTreeItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Media_Collection_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<MediaCollectionResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeCollectionReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }

    [Test]
    public async Task HasCollectionFlagProvider_Should_Populate_Media_Item_Flags()
    {
        var sut = new HasCollectionFlagProvider();

        var viewModels = new List<MediaItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), MediaType = new MediaTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
            new() { Id = Guid.NewGuid() },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = viewModels[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.HasCollection"));
    }
}
