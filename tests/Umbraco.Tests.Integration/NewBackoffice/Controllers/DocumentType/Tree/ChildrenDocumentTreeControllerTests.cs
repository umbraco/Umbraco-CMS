using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Document.Tree;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice.Controllers.DocumentType.Tree;

public class ChildrenDocumentTypeTreeControllerTests : UmbracoTestServerTestBase
{
    private ChildrenDocumentTypeTreeController Controller =>
        new ChildrenDocumentTypeTreeController(
            GetRequiredService<IEntityService>(),
            GetRequiredService<IContentTypeService>());

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentTypeContainerService ContentTypeContainerService =>
        GetRequiredService<IContentTypeContainerService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Zero(int itemsToCreate)
    {
        var parentKey = await CreateTestDocumentTypes(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 0);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Positive(int itemsToCreate)
    {
        var parentKey = await CreateTestDocumentTypes(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 1);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_GreaterThanTotal(int itemsToCreate)
    {
        var parentKey = await CreateTestDocumentTypes(itemsToCreate);

        var controllerResult = await Controller.Children(parentKey, 0, 10000);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    private async Task<Guid> CreateTestDocumentTypes(int amount)
    {
        var parentContainerKey = Guid.NewGuid();
        var documentTypeAlias = "testDocumentType";

        var folderResult = await ContentTypeContainerService.CreateAsync(
            parentContainerKey,
            "TestFolder",
            null,
            Constants.Security.SuperUserKey);
        if (folderResult.Success is false)
        {
            throw new Exception("Setup failed unexpectedly => Assert might be compromised");
        }

        for (var i = 0; i < amount; i++)
        {
            await ContentTypeService.SaveAsync(
                new ContentType(
                    ShortStringHelper,
                    Constants.System.Root)
                {
                    Name = documentTypeAlias + i,
                    Alias = documentTypeAlias + i,
                    AllowedAsRoot = true,
                    AllowedContentTypes = new[] { new ContentTypeSort(Guid.NewGuid(), 0, documentTypeAlias) },
                    ParentId = folderResult.Result.Id
                },
                Constants.Security.SuperUserKey);
        }

        return parentContainerKey;
    }
}
