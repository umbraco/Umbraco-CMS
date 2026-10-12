using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DynamicRoot;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DynamicRoot;

public class GetRootsControllerTests : ManagementApiTest<GetRootsController>
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    protected override Expression<Func<GetRootsController, object>> MethodSelector { get; set; } =
        x => x.GetRoots(CancellationToken.None, null!);

    [Test]
    public async Task Content_Root_Origin_Returns_No_Roots()
    {
        DynamicRootResponseModel result = await QueryAsync("ContentRoot", null, Guid.Empty, null);

        Assert.IsEmpty(result.Roots);
    }

    [Test]
    public async Task By_Key_Origin_Of_The_Content_Root_Returns_No_Roots()
    {
        DynamicRootResponseModel result = await QueryAsync("ByKey", Constants.System.RootSystemKey, Guid.Empty, null);

        Assert.IsEmpty(result.Roots);
    }

    [Test]
    public async Task Content_Root_Origin_With_Query_Step_Returns_Matching_Root_Level_Documents()
    {
        (IContentType contentType, IContent root, _) = await CreateContentAsync();

        DynamicRootResponseModel result = await QueryAsync(
            "ContentRoot",
            null,
            Guid.Empty,
            null,
            new DynamicRootQueryStepRequestModel { Alias = "NearestDescendantOrSelf", DocumentTypeIds = [contentType.Key] });

        CollectionAssert.AreEqual(new[] { root.Key }, result.Roots);
    }

    [Test]
    public async Task Site_Origin_Returns_The_Root_Level_Document()
    {
        (_, IContent root, IContent child) = await CreateContentAsync();

        DynamicRootResponseModel result = await QueryAsync("Site", null, root.Key, child.Key);

        CollectionAssert.AreEqual(new[] { root.Key }, result.Roots);
    }

    private async Task<(IContentType ContentType, IContent Root, IContent Child)> CreateContentAsync()
    {
        var alias = $"dynamicRoot{Guid.NewGuid():N}";
        ITemplate template = TemplateBuilder.CreateTextPageTemplate(alias, alias);
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        IContentType contentType = ContentTypeBuilder.CreateSimpleContentType(alias, alias, defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent root = ContentBuilder.CreateSimpleContent(contentType, "Root");
        ContentService.Save(root);

        IContent child = ContentBuilder.CreateSimpleContent(contentType, "Child", root);
        ContentService.Save(child);

        return (contentType, root, child);
    }

    private async Task<DynamicRootResponseModel> QueryAsync(
        string originAlias,
        Guid? originId,
        Guid parentId,
        Guid? currentId,
        params DynamicRootQueryStepRequestModel[] steps)
    {
        await AuthenticateClientAsync(Client, "admin@umbraco.com", UserPassword, isAdmin: true);

        var requestModel = new DynamicRootRequestModel
        {
            Context = new DynamicRootContextRequestModel { Id = currentId, Parent = new ReferenceByIdModel(parentId) },
            Query = new DynamicRootQueryRequestModel
            {
                Origin = new DynamicRootQueryOriginRequestModel { Alias = originAlias, Id = originId },
                Steps = steps,
            },
        };

        HttpResponseMessage response = await Client.PostAsync(Url, JsonContent.Create(requestModel));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());

        DynamicRootResponseModel? result = await response.Content.ReadFromJsonAsync<DynamicRootResponseModel>();
        Assert.IsNotNull(result);
        return result!;
    }
}
