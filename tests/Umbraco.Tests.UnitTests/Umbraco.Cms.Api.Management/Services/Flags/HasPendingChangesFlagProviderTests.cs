using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasPendingChangesFlagProviderTests
{
    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.IsTrue(sut.CanProvideFlags<DocumentVariantItemResponseModel>());
    }

    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.IsTrue(sut.CanProvideFlags<DocumentVariantResponseModel>());
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

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

        await sut.PopulateFlagsAsync(variants);

        Assert.AreEqual(variants[0].Flags.Count(), 1);
        Assert.AreEqual(variants[1].Flags.Count(), 0);

        var flagModel = variants[0].Flags.First();
        Assert.AreEqual("Umb.PendingChanges", flagModel.Alias);
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

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

        await sut.PopulateFlagsAsync(variants);

        Assert.AreEqual(variants[0].Flags.Count(), 1);
        Assert.AreEqual(variants[1].Flags.Count(), 0);

        var flagModel = variants[0].Flags.First();
        Assert.AreEqual("Umb.PendingChanges", flagModel.Alias);
    }
}
