using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class HasPendingChangesFlagProviderTests
{
    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Document_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentVariantItemResponseModel>(), Is.True);
    }

    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Document_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentVariantResponseModel>(), Is.True);
    }

    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Element_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.That(sut.CanProvideFlags<ElementVariantItemResponseModel>(), Is.True);
    }

    [Test]
    public void HasPendingChangesFlagProvider_Can_Provide_Element_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();
        Assert.That(sut.CanProvideFlags<ElementVariantResponseModel>(), Is.True);
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Document_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

        var variants = new List<DocumentVariantItemResponseModel>
        {
            new()
            {
                State = PublishableVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = PublishableVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateFlagsAsync(variants);

        Assert.That(variants[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(variants[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = variants[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.PendingChanges"));
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Document_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

        var variants = new List<DocumentVariantResponseModel>
        {
            new()
            {
                State = PublishableVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = PublishableVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateFlagsAsync(variants);

        Assert.That(variants[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(variants[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = variants[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.PendingChanges"));
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Element_Variant_Item_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

        var variants = new List<ElementVariantItemResponseModel>
        {
            new()
            {
                State = PublishableVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = PublishableVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateFlagsAsync(variants);

        Assert.That(variants[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(variants[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = variants[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.PendingChanges"));
    }

    [Test]
    public async Task HasPendingChangesFlagProvider_Should_Populate_Element_Variant_Flags()
    {
        var sut = new HasPendingChangesFlagProvider();

        var variants = new List<ElementVariantResponseModel>
        {
            new()
            {
                State = PublishableVariantState.PublishedPendingChanges,
                Culture = null,
                Name = "Test",
            },
            new()
            {
                State = PublishableVariantState.Published,
                Culture = null,
                Name = "Test2",
            },
        };

        await sut.PopulateFlagsAsync(variants);

        Assert.That(variants[0].Flags.Count(), Is.EqualTo(1));
        Assert.That(variants[1].Flags.Count(), Is.EqualTo(0));

        var flagModel = variants[0].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.PendingChanges"));
    }
}
