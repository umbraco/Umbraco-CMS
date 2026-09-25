using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class CreateDocumentBlueprintControllerTests : ManagementApiUserGroupTestBase<CreateDocumentBlueprintController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _contentTypeKey;

    private Guid? _parentKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        _contentTypeKey = contentType.Key;
        _parentKey = null;
    }

    protected override Expression<Func<CreateDocumentBlueprintController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Created };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var createModel = new CreateDocumentBlueprintRequestModel
        {
            DocumentType = new ReferenceByIdModel(_contentTypeKey),
            Parent = _parentKey.HasValue ? new ReferenceByIdModel(_parentKey.Value) : null,
            Id = Guid.NewGuid(),
            Values = [],
            Variants = [new DocumentVariantRequestModel { Culture = null, Segment = null, Name = $"Blueprint {Guid.NewGuid()}" }],
        };

        return await Client.PostAsync(Url, JsonContent.Create(createModel));
    }

    [Test]
    public async Task User_With_Start_Node_Can_Create_A_Blueprint_Below_It()
    {
        var startNodeFolder = await CreateFolderAsync();
        _parentKey = startNodeFolder.Key;

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task User_With_Start_Node_Cannot_Create_A_Blueprint_At_The_Root()
    {
        var startNodeFolder = await CreateFolderAsync();
        _parentKey = null;

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task<EntityContainer> CreateFolderAsync()
    {
        var result = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the folder: {result.Status}");
        return result.Result!;
    }

    private async Task AuthenticateAsUserScopedToFolderAsync(int folderId)
    {
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Group With Document Blueprint Start Node {Guid.NewGuid()}")
            .WithAllowedSections(["library"])
            .WithStartDocumentBlueprintId(folderId)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"blueprintstartnode{Guid.NewGuid():N}@umbraco.com",
            UserPassword,
            userGroup.Key);
    }
}
