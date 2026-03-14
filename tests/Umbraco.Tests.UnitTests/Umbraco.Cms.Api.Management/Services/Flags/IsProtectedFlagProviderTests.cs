using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Flags;

[TestFixture]
internal class IsProtectedFlagProviderTests
{
    /// <summary>
    /// Tests that the IsProtectedFlagProvider can provide tree flags for DocumentTreeItemResponseModel.
    /// </summary>
    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Tree_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.IsTrue(sut.CanProvideFlags<DocumentTreeItemResponseModel>());
    }

    /// <summary>
    /// Tests that the IsProtectedFlagProvider can provide flags for a collection.
    /// </summary>
    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Collection_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.IsTrue(sut.CanProvideFlags<DocumentCollectionResponseModel>());
    }

    /// <summary>
    /// Tests that the IsProtectedFlagProvider can provide plain flags for DocumentItemResponseModel.
    /// </summary>
    [Test]
    public void IsProtectedFlagProvider_Can_Provide_Plain_Flags()
    {
        var sut = new IsProtectedFlagProvider();
        Assert.IsTrue(sut.CanProvideFlags<DocumentItemResponseModel>());
    }

    /// <summary>
    /// Verifies that <see cref="IsProtectedFlagProvider"/> correctly populates the "IsProtected" flag on <see cref="DocumentTreeItemResponseModel"/> instances within a tree structure.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

        Assert.AreEqual(viewModels[0].Flags.Count(), 0);
        Assert.AreEqual(viewModels[1].Flags.Count(), 1);

        var flagModel = viewModels[1].Flags.First();
        Assert.AreEqual("Umb.IsProtected", flagModel.Alias);
    }

    /// <summary>
    /// Tests that the IsProtectedFlagProvider correctly populates the Flags collection
    /// on DocumentCollectionResponseModel instances based on their IsProtected property.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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

        Assert.AreEqual(viewModels[0].Flags.Count(), 0);
        Assert.AreEqual(viewModels[1].Flags.Count(), 1);

        var flagModel = viewModels[1].Flags.First();
        Assert.AreEqual("Umb.IsProtected", flagModel.Alias);
    }

    /// <summary>
    /// Verifies that <see cref="IsProtectedFlagProvider"/> correctly populates the "Umb.IsProtected" flag
    /// only for document items that are marked as protected.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

        Assert.AreEqual(viewModels[0].Flags.Count(), 0);
        Assert.AreEqual(viewModels[1].Flags.Count(), 1);

        var flagModel = viewModels[1].Flags.First();
        Assert.AreEqual("Umb.IsProtected", flagModel.Alias);
    }
}
