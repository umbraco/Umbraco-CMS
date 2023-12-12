using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice.Controllers.Dictionary.Tree;

public class ChildrenDictionaryTreeControllerTests : UmbracoTestServerTestBase
{
    private ChildrenDictionaryTreeController Controller =>
        new ChildrenDictionaryTreeController(GetRequiredService<IEntityService>(),
            GetRequiredService<IDictionaryItemService>());

    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Zero(int itemsToCreate)
    {
        var parentKey = await CreateTestDictionaryItems(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 0);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<EntityTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Positive(int itemsToCreate)
    {
        var parentKey = await CreateTestDictionaryItems(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 1);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<EntityTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_GreaterThanTotal(int itemsToCreate)
    {
        var parentKey = await CreateTestDictionaryItems(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 10000);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<EntityTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    private async Task<Guid> CreateTestDictionaryItems(int amount)
    {
        var parentDictionaryKey = Guid.NewGuid();

        var parentAttempt = await DictionaryItemService.CreateAsync(
            new DictionaryItem("parentItem") { Key = parentDictionaryKey },
            Constants.Security.SuperUserKey);
        if (parentAttempt.Success is false)
        {
            throw new Exception("Setup failed unexpectedly => Assert might be compromised");
        }

        for (var i = 0; i < amount; i++)
        {
            var attempt = await DictionaryItemService.CreateAsync(
                new DictionaryItem(parentDictionaryKey, "testItem" + i),
                Constants.Security.SuperUserKey);
            if (attempt.Success is false)
            {
                throw new Exception("Setup failed unexpectedly => Assert might be compromised");
            }
        }

        return parentDictionaryKey;
    }
}
