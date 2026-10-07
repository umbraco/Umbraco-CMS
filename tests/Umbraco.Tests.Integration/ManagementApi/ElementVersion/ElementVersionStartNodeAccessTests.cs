using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.ElementVersion;
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

namespace Umbraco.Cms.Tests.Integration.ManagementApi.ElementVersion;

/// <summary>
/// Asserts that the element version endpoints honour the element start node of the requesting user.
/// </summary>
/// <remarks>
/// <see cref="ManagementApiUserGroupTestBase{T}"/> only varies the built-in user groups, all of which have
/// root level library access, so it cannot express a start node restriction. These tests authenticate as a
/// user whose group is restricted to one container and assert that elements in the other container remain
/// inaccessible through every element version endpoint.
/// </remarks>
public class ElementVersionStartNodeAccessTests : ManagementApiTest<AllElementVersionController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementVersionService ElementVersionService => GetRequiredService<IElementVersionService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _accessibleElementKey;

    private Guid _inaccessibleElementKey;

    private Guid _accessibleVersionId;

    private Guid _inaccessibleVersionId;

    protected override Expression<Func<AllElementVersionController, object>> MethodSelector { get; set; }

    [SetUp]
    public new async Task Setup()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        EntityContainer deniedContainer = await CreateContainerAsync();
        EntityContainer allowedContainer = await CreateContainerAsync();

        _inaccessibleElementKey = await CreateElementAsync(elementType.Key, deniedContainer.Key);
        _accessibleElementKey = await CreateElementAsync(elementType.Key, allowedContainer.Key);

        _inaccessibleVersionId = await GetVersionIdAsync(_inaccessibleElementKey);
        _accessibleVersionId = await GetVersionIdAsync(_accessibleElementKey);

        // The user may only reach the branch below allowedContainer.
        await AuthenticateRestrictedUserAsync(allowedContainer.Id);
    }

    [Test]
    public async Task Cannot_Get_Version_History_Of_Element_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionHistoryUrl(_inaccessibleElementKey));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Get_Version_History_Of_Element_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionHistoryUrl(_accessibleElementKey));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Get_Version_Of_Element_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionUrl(_inaccessibleVersionId));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Get_Version_Of_Element_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(VersionUrl(_accessibleVersionId));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Cannot_Set_Prevent_Cleanup_On_Element_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.PutAsync(PreventCleanupUrl(_inaccessibleVersionId), null);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Set_Prevent_Cleanup_On_Element_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.PutAsync(PreventCleanupUrl(_accessibleVersionId), null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    private string VersionHistoryUrl(Guid elementId) =>
        GetManagementApiUrl<AllElementVersionController>(x => x.All(CancellationToken.None, elementId, null, 0, 100));

    private string VersionUrl(Guid versionId) =>
        GetManagementApiUrl<ByKeyElementVersionController>(x => x.ByKey(CancellationToken.None, versionId));

    private string PreventCleanupUrl(Guid versionId) =>
        GetManagementApiUrl<UpdatePreventCleanupElementVersionController>(x => x.Set(CancellationToken.None, versionId, true));

    private async Task<EntityContainer> CreateContainerAsync()
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> attempt = await ElementContainerService.CreateAsync(
            null,
            Guid.NewGuid().ToString(),
            null,
            Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the element container: {attempt.Status}");

        return attempt.Result!;
    }

    private async Task<Guid> CreateElementAsync(Guid elementTypeKey, Guid parentKey)
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };

        var attempt = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(attempt.Success, $"Could not create the element: {attempt.Status}");

        return attempt.Result!.Content!.Key;
    }

    private async Task<Guid> GetVersionIdAsync(Guid elementKey)
    {
        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await ElementVersionService.GetPagedContentVersionsAsync(elementKey, null, 0, 10);
        Assert.IsTrue(attempt.Success, $"Could not read the element versions: {attempt.Status}");

        // Version identifiers are surfaced to the API as the integer version id reinterpreted as a Guid.
        return attempt.Result!.Items.First().VersionId.ToGuid();
    }

    private async Task AuthenticateRestrictedUserAsync(int startElementId)
    {
        var alias = $"restricted{Guid.NewGuid():N}";

        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Library])
            .WithPermissions(new HashSet<string> { ActionElementBrowse.ActionLetter, ActionElementRollback.ActionLetter })
            .WithStartElementId(startElementId)
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
