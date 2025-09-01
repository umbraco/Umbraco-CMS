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

        var variant1 = new DocumentVariantItemResponseModel()
        {
            State = DocumentVariantState.PublishedPendingChanges, Culture = null, Name = "Test",
        };

        var variant2 = new DocumentVariantItemResponseModel()
        {
            State = DocumentVariantState.Published, Culture = null, Name = "Test",
        };

        await sut.PopulateSignsAsync(variant1);
        await sut.PopulateSignsAsync(variant2);

        Assert.AreEqual(variant1.Signs.Count(), 1);
        Assert.AreEqual(variant2.Signs.Count(), 0);

        var signModel = variant1.Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }

    [Test]
    public async Task HasPendingChangesSignProvider_Should_Populate_Variant_Signs()
    {
        var sut = new HasPendingChangesSignProvider();

        var variant1 = new DocumentVariantResponseModel()
        {
            State = DocumentVariantState.PublishedPendingChanges, Culture = null, Name = "Test",
        };

        var variant2 = new DocumentVariantResponseModel()
        {
            State = DocumentVariantState.Published, Culture = null, Name = "Test",
        };

        await sut.PopulateSignsAsync(variant1);
        await sut.PopulateSignsAsync(variant2);

        Assert.AreEqual(variant1.Signs.Count(), 1);
        Assert.AreEqual(variant2.Signs.Count(), 0);

        var signModel = variant1.Signs.First();
        Assert.AreEqual("Umb.PendingChanges", signModel.Alias);
    }
}
