using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    [Test]
    public async Task Can_Purge_Empty_Containers_From_Recycle_Bin()
    {
        for (var i = 0; i < 5; i++)
        {
            var key = Guid.NewGuid();
            await ElementContainerService.CreateAsync(key, $"Root Container {i}", null, Constants.Security.SuperUserKey);
            await ElementContainerService.MoveToRecycleBinAsync(key, Constants.Security.SuperUserKey);
        }

        Assert.AreEqual(5, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());

        var emptyResult = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.IsTrue(emptyResult);
        Assert.AreEqual(0, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());
    }

    [Test]
    [LongRunning]
    public async Task Can_Purge_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Recycle_Bin()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);
        await ElementContainerService.MoveToRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.AreNotEqual(0, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());

        var emptyResult = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.IsTrue(emptyResult);
        Assert.AreEqual(0, EntityService.GetDescendants(Constants.System.RecycleBinElement).Count());
    }
}
