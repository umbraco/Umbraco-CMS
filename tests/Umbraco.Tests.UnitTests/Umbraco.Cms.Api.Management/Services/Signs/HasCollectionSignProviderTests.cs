using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Tree;



namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class HasCollectionSignProviderTests
{
    [Test]
    public async Task HasCollectionSignProvider_Can_Provide_Tree_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentTreeItemResponseModel>());
    }

    [Test]
    public async Task HasCollectionSignProvider_Should_Populate_Tree_Signs()
    {
        var sut = new HasCollectionSignProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new()
            {
                Id = Guid.NewGuid(), DocumentType = new DocumentTypeReferenceResponseModel() { Collection = new ReferenceByIdModel(Guid.NewGuid()) },
            },
        };

        await sut.PopulateSignsAsync(viewModels);

        Assert.AreEqual(viewModels[0].Signs.Count(), 1);

        var signModel = viewModels[0].Signs.First();
        Assert.AreEqual("Umb.HasCollection", signModel.Alias);
    }
}
