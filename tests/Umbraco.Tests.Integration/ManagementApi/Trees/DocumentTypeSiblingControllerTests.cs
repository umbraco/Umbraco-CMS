using System.Linq.Expressions;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Trees;

[TestFixture]
internal sealed class DocumentTypeSiblingControllerTests : ManagementApiTest<SiblingsDocumentTypeTreeController>
{
    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private ContentTypeService ContentTypeService => (ContentTypeService)GetRequiredService<IContentTypeService>();

    protected override Expression<Func<SiblingsDocumentTypeTreeController, object>> MethodSelector =>
        x => x.Siblings(CancellationToken.None, Guid.Empty, 0, 0, false);

    [Test]
    public async Task Document_Type_Siblings_Under_Folder_Have_Correct_Parent()
    {
        // create folder
        Attempt<EntityContainer, EntityContainerOperationStatus> folderResult =
            await ContentTypeContainerService.CreateAsync(null, "Root Container", null, Constants.Security.SuperUserKey);

        // create contentTypeOne
        IContentType contentTypeOne = ContentTypeBuilder.CreateBasicContentType();
        contentTypeOne.Alias = "contentTypeOne";
        contentTypeOne.ParentId = folderResult.Result.Id;
        contentTypeOne.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentTypeOne);

        // create contentTypeTwo
        IContentType contentTypeTwo = ContentTypeBuilder.CreateBasicContentType();
        contentTypeTwo.Alias = "contentTypeTwo";
        contentTypeTwo.ParentId = folderResult.Result.Id;
        contentTypeTwo.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentTypeTwo);

        // get siblings of doctype one
        await AuthenticateClientAsync(Client, "test@test.test", "test@test.test", true);
        var siblingsResponse = await GetManagementApiResponseAsync(contentTypeOne.Key);
        var responseModel = await siblingsResponse.Content.ReadFromJsonAsync<SubsetViewModel<DocumentTreeItemResponseModel>>();

        Assert.IsTrue(responseModel.Items.All(i => i.Parent!.Id == folderResult.Result.Key));
    }

    private async Task<HttpResponseMessage> GetManagementApiResponseAsync(Guid target)
    {
        var url = GetManagementApiUrl<SiblingsDocumentTypeTreeController>(x =>
            x.Siblings(CancellationToken.None, target, 10, 10, false));
        return await Client.GetAsync(url);
    }
}
