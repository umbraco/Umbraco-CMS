using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class MoveDocumentBlueprintControllerTests : ManagementApiUserGroupTestBase<MoveDocumentBlueprintController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentBlueprintEditingService ContentBlueprintEditingService =>
        GetRequiredService<IContentBlueprintEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _documentBlueprintKey;

    private Guid _targetFolderKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var folder = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Test Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(folder.Success, $"Failed to create the folder: {folder.Status}");
        _targetFolderKey = folder.Result!.Key;

        // A new blueprint for each test, since moving it changes where the next move starts from.
        var blueprint = await ContentBlueprintEditingService.CreateAsync(
            new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = $"Blueprint {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(blueprint.Success, $"Failed to create the blueprint: {blueprint.Status}");
        _documentBlueprintKey = blueprint.Result.Content!.Key;
    }

    protected override Expression<Func<MoveDocumentBlueprintController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _documentBlueprintKey, null!);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Forbidden };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var moveModel = new MoveDocumentBlueprintRequestModel { Target = new ReferenceByIdModel(_targetFolderKey) };

        return await Client.PutAsync(Url, JsonContent.Create(moveModel));
    }
}
