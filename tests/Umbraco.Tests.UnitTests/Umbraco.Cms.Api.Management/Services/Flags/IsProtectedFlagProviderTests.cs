using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class IsProtectedFlagProviderTests
{
    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Tree_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentTreeItemResponseModel>(), Is.True);
    }

    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Collection_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentCollectionResponseModel>(), Is.True);
    }

    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Plain_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.That(sut.CanProvideFlags<DocumentItemResponseModel>(), Is.True);
    }

    [Test]
    public async Task IsProtectedFlagProvider_Should_Populate_Tree_Flags()
    {
        var sut = new IsProtectedFlagProvider();

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(0));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(1));

        var flagModel = viewModels[1].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.IsProtected"));
    }

    [Test]
    public async Task IsProtectedFlagProvider_Should_Populate_Collection_Flags()
    {
        var sut = new IsProtectedFlagProvider();

        var viewModels = new List<DocumentCollectionResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(0));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(1));

        var flagModel = viewModels[1].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.IsProtected"));
    }

    [Test]
    public async Task IsProtectedFlagProvider_Should_Populate_Plain_Flags()
    {
        var sut = new IsProtectedFlagProvider();

        var viewModels = new List<DocumentItemResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateFlagsAsync(viewModels);

        Assert.That(viewModels[0].Flags.Count(), Is.EqualTo(0));
        Assert.That(viewModels[1].Flags.Count(), Is.EqualTo(1));

        var flagModel = viewModels[1].Flags.First();
        Assert.That(flagModel.Alias, Is.EqualTo("Umb.IsProtected"));
    }
}
