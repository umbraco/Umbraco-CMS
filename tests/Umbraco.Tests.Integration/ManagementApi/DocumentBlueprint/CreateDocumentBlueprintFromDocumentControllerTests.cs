using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentBlueprint;

public class CreateDocumentBlueprintFromDocumentControllerTests
    : ManagementApiUserGroupTestBase<CreateDocumentBlueprintFromDocumentController>
{
    private IContentBlueprintContainerService ContentBlueprintContainerService =>
        GetRequiredService<IContentBlueprintContainerService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected override Expression<Func<CreateDocumentBlueprintFromDocumentController, object>> MethodSelector =>
        x => x.CreateFromDocument(CancellationToken.None, null!);

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

    // The matrix above exercises placing a blueprint at the root.
    protected override async Task<HttpResponseMessage> ClientRequest() => await PostCreateFromDocument(null);

    private Guid _documentKey;

    private Guid _scopedFolderKey;

    private int _scopedFolderId;

    private Guid _otherFolderKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Document Type {Guid.NewGuid()}")
            .WithAllowAsRoot(true)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var documentResult = await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = $"Document {Guid.NewGuid()}" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(documentResult.Success, $"Failed to create the document: {documentResult.Status}");
        _documentKey = documentResult.Result.Content!.Key;

        var scopedFolder = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Scoped Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(scopedFolder.Success, $"Failed to create the scoped folder: {scopedFolder.Status}");
        _scopedFolderKey = scopedFolder.Result!.Key;
        _scopedFolderId = scopedFolder.Result.Id;

        var otherFolder = await ContentBlueprintContainerService.CreateAsync(
            null,
            $"Other Folder {Guid.NewGuid()}",
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(otherFolder.Success, $"Failed to create the other folder: {otherFolder.Status}");
        _otherFolderKey = otherFolder.Result!.Key;
    }

    [Test]
    public async Task Can_Create_In_The_Folder_The_User_Is_Scoped_To()
    {
        await AuthenticateAsScopedUser();

        var response = await PostCreateFromDocument(_scopedFolderKey);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Create_In_A_Folder_The_User_Is_Not_Scoped_To()
    {
        await AuthenticateAsScopedUser();

        var response = await PostCreateFromDocument(_otherFolderKey);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Create_At_The_Root_When_Scoped_To_A_Folder()
    {
        await AuthenticateAsScopedUser();

        var response = await PostCreateFromDocument(null);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private async Task AuthenticateAsScopedUser()
    {
        var userGroup = new UserGroupBuilder()
            .WithAlias(Guid.NewGuid().ToString("N"))
            .WithName($"Test Group Scoped To A Blueprint Folder {Guid.NewGuid()}")
            .WithAllowedSections([Constants.Applications.Content, Constants.Applications.Library])
            .WithPermissions(new HashSet<string> { ActionCreateBlueprintFromContent.ActionLetter })
            .WithStartDocumentBlueprintId(_scopedFolderId)
            .Build();
        await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);

        await AuthenticateClientAsync(
            Client,
            $"blueprintdestination{Guid.NewGuid():N}@umbraco.com",
            "1234567890",
            userGroup.Key);
    }

    private async Task<HttpResponseMessage> PostCreateFromDocument(Guid? parentKey) =>
        await Client.PostAsync(
            Url,
            JsonContent.Create(new CreateDocumentBlueprintFromDocumentRequestModel
            {
                Document = new ReferenceByIdModel(_documentKey),
                Name = $"Blueprint {Guid.NewGuid()}",
                Parent = parentKey is null ? null : new ReferenceByIdModel(parentKey.Value),
            }));
}
