using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class DeleteDocumentBlueprintControllerTests : ManagementApiUserGroupTestBase<DeleteDocumentBlueprintController>
{
    private IContentBlueprintEditingService ContentBlueprintEditingService =>
        GetRequiredService<IContentBlueprintEditingService>();

    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _documentBlueprintKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // A new blueprint for each test, since deleting removes it.
        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
        };
        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the blueprint: {result.Status}");
        _documentBlueprintKey = result.Result.Content!.Key;
    }

    protected override Expression<Func<DeleteDocumentBlueprintController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _documentBlueprintKey);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);

    [Test]
    public async Task User_With_Start_Node_Can_Delete_A_Blueprint_Below_It()
    {
        var startNodeFolder = await CreateFolderAsync();
        _documentBlueprintKey = await CreateBlueprintAsync(startNodeFolder.Key);

        await AuthenticateAsUserScopedToFolderAsync(startNodeFolder.Id);

        var response = await ClientRequest();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task User_With_Start_Node_Cannot_Delete_A_Blueprint_Outside_It()
    {
        var startNodeFolder = await CreateFolderAsync();
        var otherFolder = await CreateFolderAsync();
        _documentBlueprintKey = await CreateBlueprintAsync(otherFolder.Key);

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

    private async Task<Guid> CreateBlueprintAsync(Guid parentKey)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var result = await ContentBlueprintEditingService.CreateAsync(
            new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = parentKey,
                Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create the blueprint: {result.Status}");
        return result.Result.Content!.Key;
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
