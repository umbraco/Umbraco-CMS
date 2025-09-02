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
    public void HasPendingChangesSignProvider_Can_Provide_Variant_Item_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentVariantItemResponseModel>());
    }

    [Test]
    public void HasPendingChangesSignProvider_Can_Provide_Variant_Signs()
    {
        var sut = new HasPendingChangesSignProvider();
        Assert.IsTrue(sut.CanProvideSigns<DocumentVariantResponseModel>());
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Variant_Item_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var variants = new List<DocumentVariantItemResponseModel>
        {
            new()
            {
                State = DocumentVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = DocumentVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateSignsAsync(variants);

        Assert.AreEqual(variants[0].Signs.Count(), 1);
        Assert.AreEqual(variants[1].Signs.Count(), 0);

        var signModel = variants[0].Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Variant_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var variants = new List<DocumentVariantResponseModel>
        {
            new()
            {
                State = DocumentVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = DocumentVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateSignsAsync(variants);

        Assert.AreEqual(variants[0].Signs.Count(), 1);
        Assert.AreEqual(variants[1].Signs.Count(), 0);

        var signModel = variants[0].Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }
}
