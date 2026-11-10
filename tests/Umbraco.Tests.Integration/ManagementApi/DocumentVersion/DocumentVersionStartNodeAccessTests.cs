using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentVersion;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentVersion;

/// <summary>
/// Asserts that the document version endpoints honour the content start node of the requesting user.
/// </summary>
/// <remarks>
/// <see cref="ManagementApiUserGroupTestBase{T}"/> only varies the built-in user groups, all of which have
/// root level content access, so it cannot express a start node restriction. These tests authenticate as a
/// user whose group is restricted to one branch and assert that documents in the other branch remain
/// inaccessible through every document version endpoint.
/// </remarks>
public class DocumentVersionStartNodeAccessTests : ManagementApiTest<AllDocumentVersionController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentVersionService ContentVersionService => GetRequiredService<IContentVersionService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IContent _accessibleDocument;

    private IContent _inaccessibleDocument;

    private Guid _accessibleVersionId;

    private Guid _inaccessibleVersionId;

    protected override Expression<Func<AllDocumentVersionController, object>> MethodSelector { get; set; }

    [SetUp]
    public new async Task Setup()
    {
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContent deniedRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);
        IContent allowedRoot = await CreateDocumentAsync(contentType.Key, Constants.System.RootKey);

        _inaccessibleDocument = await CreateDocumentAsync(contentType.Key, deniedRoot.Key);
        _accessibleDocument = await CreateDocumentAsync(contentType.Key, allowedRoot.Key);

        _inaccessibleVersionId = await GetVersionIdAsync(_inaccessibleDocument.Key);
        _accessibleVersionId = await GetVersionIdAsync(_accessibleDocument.Key);

        // The user may only reach the branch below allowedRoot.
        await AuthenticateRestrictedUserAsync(allowedRoot.Id);
    }

    [Test]
    public async Task Cannot_Get_Version_History_Of_Document_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionHistoryUrl(_inaccessibleDocument.Key));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Get_Version_History_Of_Document_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionHistoryUrl(_accessibleDocument.Key));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Get_Version_Of_Document_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionUrl(_inaccessibleVersionId));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Get_Version_Of_Document_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionUrl(_accessibleVersionId));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Set_Prevent_Cleanup_On_Document_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.PutAsync(PreventCleanupUrl(_inaccessibleVersionId), null);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Set_Prevent_Cleanup_On_Document_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.PutAsync(PreventCleanupUrl(_accessibleVersionId), null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string VersionHistoryUrl(Guid documentId) =>
        GetManagementApiUrl<AllDocumentVersionController>(x => x.All(CancellationToken.None, documentId, null, 0, 100));

    private string VersionUrl(Guid versionId) =>
        GetManagementApiUrl<ByKeyDocumentVersionController>(x => x.ByKey(CancellationToken.None, versionId));

    private string PreventCleanupUrl(Guid versionId) =>
        GetManagementApiUrl<UpdatePreventCleanupDocumentVersionController>(x => x.Set(CancellationToken.None, versionId, true));

    private async Task<IContent> CreateDocumentAsync(Guid contentTypeKey, Guid? parentKey)
    {
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        Attempt<ContentCreateResult, ContentEditingOperationStatus> attempt =
            await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the document: {attempt.Status}");

        return attempt.Result.Content;
    }

    private async Task<Guid> GetVersionIdAsync(Guid documentKey)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await ContentVersionService.GetPagedContentVersionsAsync(documentKey, null, 0, 10);
        Assert.IsTrue(attempt.Success, $"Could not read the document versions: {attempt.Status}");

        ContentVersionMeta version = attempt.Result!.Items.First();

        // Version identifiers are surfaced to the API as the integer version id reinterpreted as a Guid.
        return version.VersionId.ToGuid();
    }

    private async Task AuthenticateRestrictedUserAsync(int startContentId)
    {
        var alias = $"restricted{Guid.NewGuid():N}";

        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Content])
            .WithPermissions(new HashSet<string> { ActionBrowse.ActionLetter, ActionRollback.ActionLetter })
            .WithStartContentId(startContentId)
            .Build();
        userGroup.HasAccessToAllLanguages = true;

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupAttempt =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(userGroupAttempt.Success, $"Could not create the user group: {userGroupAttempt.Status}");

        var email = $"{alias}@umbraco.com";

        await AuthenticateClientAsync(
            Client,
            async userService =>
            {
                IUser user = (await userService.CreateAsync(
                    Constants.Security.SuperUserKey,
                    new UserCreateModel
                    {
                        Email = email,
                        Name = alias,
                        UserName = email,
                        UserGroupKeys = new HashSet<Guid> { userGroupAttempt.Result.Key },
                    },
                    true)).Result.CreatedUser;

                return (user, UserPassword);
            },
            $"{email}:{alias}");
    }
}
