using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Document.Tree;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Integration.TestServerTest;

namespace Umbraco.Cms.Tests.Integration.NewBackoffice.Controllers.Document.Tree;

public class RootDocumentTreeControllerTests : UmbracoTestServerTestBase
{
    private RootDocumentTreeController Controller =>
        new RootDocumentTreeController(
            GetRequiredService<IEntityService>(),
            GetRequiredService<IUserStartNodeEntitiesService>(),
            GetRequiredService<IDataTypeService>(),
            GetRequiredService<IPublicAccessService>(),
            GetRequiredService<AppCaches>(),
            GetRequiredService<IBackOfficeSecurityAccessor>(),
            GetRequiredService<IContentTypeService>());

    private IContentService ContentService => GetRequiredService<IContentService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Zero(int itemsToCreate)
    {
        await CreateTestDocuments(itemsToCreate);

        var controllerResult = await Controller.Root(0, 0);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_Positive(int itemsToCreate)
    {
        await CreateTestDocuments(itemsToCreate);

        var controllerResult = await Controller.Root(0, 1);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    public async Task Total_Equals_CreateAmount_When_PageSize_GreaterThanTotal(int itemsToCreate)
    {
        await CreateTestDocuments(itemsToCreate);

        var controllerResult = await Controller.Root(0, 10000);
        var controllerValueResult =
            (controllerResult.Result as OkObjectResult)!.Value as PagedViewModel<DocumentTreeItemResponseModel>;

        Assert.AreEqual(itemsToCreate, controllerValueResult!.Total);
    }

    private async Task CreateTestDocuments(int amount)
    {
        var documentTypeAlias = "testDocumentType";

        await ContentTypeService.SaveAsync(
            new ContentType(
                ShortStringHelper,
                Constants.System.Root)
            {
                Name = documentTypeAlias,
                Alias = documentTypeAlias,
                AllowedAsRoot = true,
                AllowedContentTypes = new[] { new ContentTypeSort(Guid.NewGuid(), 0, documentTypeAlias) },
            },
            Constants.Security.SuperUserKey);

        for (var i = 0; i < amount; i++)
        {
            var document = ContentService.CreateAndSave("document" + i, Constants.System.Root, documentTypeAlias);
        }
    }
}
