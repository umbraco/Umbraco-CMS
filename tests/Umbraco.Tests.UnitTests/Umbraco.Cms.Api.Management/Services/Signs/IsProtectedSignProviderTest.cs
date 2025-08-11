using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services.Signs;

[TestFixture]
internal class IsProtectedSignProviderTest
{
    [Test]
    public async Task IsProtectedSignProvider_Should_Populate_Signs()
    {
        var entities = new List<EntitySlim>
        {
            new() { Name = "Item 1" },
            new() { Name = "Item 2" },
        };

        var sut = new IsProtectedSignProvider();

        Assert.IsTrue(sut.CanProvideTreeSigns<DocumentTreeItemResponseModel>());

        var viewModels = new List<DocumentTreeItemResponseModel>
        {
            new(),
            new() { IsProtected = true },
        };

        await sut.PopulateTreeSignsAsync(viewModels.ToArray(), entities);

        Assert.AreEqual(viewModels[0].Signs.Count(), 0);
        Assert.AreEqual(viewModels[1].Signs.Count(), 1);

        var signModel = viewModels[1].Signs.First();
        Assert.AreEqual("Umb.IsProtected", signModel.Alias);
    }
}
