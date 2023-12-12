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

public class RootDocumentTypeTreeControllerTests : UmbracoTestServerTestBase
{
    private RootDocumentTypeTreeController Controller =>
        new RootDocumentTypeTreeController(
            GetRequiredService<IEntityService>(),
            GetRequiredService<IContentTypeService>());

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(10, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(10, 1)]
    [TestCase(0, 10)]
    [TestCase(1, 10)]
    [TestCase(10, 10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Zero(int itemsToCreate, int foldersToCreate)
    {
        await CreateTestDocumentTypes(itemsToCreate);
        await CreateTestFolders(foldersToCreate);

        var controllerResult = await Controller.Root(0, 0);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        var controllerFolderOnlyResult = await Controller.Root(0, 0, true);
        var controllerFolderOnlyValueResult =
            (controllerFolderOnlyResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(
                itemsToCreate + foldersToCreate,
                controllerValueResult!.Total);
            Assert.AreEqual(foldersToCreate, controllerFolderOnlyValueResult!.Total);
        });
    }

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(10, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(10, 1)]
    [TestCase(0, 10)]
    [TestCase(1, 10)]
    [TestCase(10, 10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Positive(int itemsToCreate, int foldersToCreate)
    {
        await CreateTestDocumentTypes(itemsToCreate);
        await CreateTestFolders(foldersToCreate);

        var controllerResult = await Controller.Root(0, 1);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        var controllerFolderOnlyResult = await Controller.Root(0, 1, true);
        var controllerFolderOnlyValueResult =
            (controllerFolderOnlyResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(
                itemsToCreate + foldersToCreate,
                controllerValueResult!.Total);
            Assert.AreEqual(foldersToCreate, controllerFolderOnlyValueResult!.Total);
        });
    }

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(10, 0)]
    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(10, 1)]
    [TestCase(0, 10)]
    [TestCase(1, 10)]
    [TestCase(10, 10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_GreaterThanTotal(int itemsToCreate, int foldersToCreate)
    {
        await CreateTestDocumentTypes(itemsToCreate);
        await CreateTestFolders(foldersToCreate);

        var controllerResult = await Controller.Root(0, 10000);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        var controllerFolderOnlyResult = await Controller.Root(0, 10000, true);
        var controllerFolderOnlyValueResult =
            (controllerFolderOnlyResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTypeTreeItemResponseModel>;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(
                itemsToCreate + foldersToCreate,
                controllerValueResult!.Total);
            Assert.AreEqual(foldersToCreate, controllerFolderOnlyValueResult!.Total);
        });
    }

    private async Task CreateTestDocumentTypes(int amount)
    {
        var documentTypeAlias = "testDocumentType";

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
                },
                Constants.Security.SuperUserKey);
        }
    }

    private async Task CreateTestFolders(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            var folderResult = await ContentTypeContainerService.CreateAsync(
                null,
                "TestFolder" + i,
                null,
                Constants.Security.SuperUserKey);
            if (folderResult.Success is false)
            {
                throw new Exception("Setup failed unexpectedly => Assert might be compromised");
            }
        }
    }
}
