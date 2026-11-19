using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

/// <summary>
/// Asserts that the preview URL endpoint honours the content start node of the requesting user.
/// </summary>
/// <remarks>
/// <see cref="ManagementApiUserGroupTestBase{T}"/> only varies the built-in user groups, all of which have
/// root level content access, so it cannot express a start node restriction.
/// </remarks>
public class DocumentPreviewUrlStartNodeAccessTests : ManagementApiTest<DocumentPreviewUrlController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IContent _accessibleDocument;

    private IContent _inaccessibleDocument;

    protected override Expression<Func<DocumentPreviewUrlController, object>> MethodSelector { get; set; }

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

        await AuthenticateRestrictedUserAsync(allowedRoot.Id);
    }

    [Test]
    public async Task Cannot_Get_Preview_Url_Of_Document_Outside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(PreviewUrl(_inaccessibleDocument.Key));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Can_Get_Preview_Url_Of_Document_Inside_Start_Node()
    {
        HttpResponseMessage response = await Client.GetAsync(PreviewUrl(_accessibleDocument.Key));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    // GetManagementApiUrl requires the action to take a CancellationToken, which this one doesn't.
    private static string PreviewUrl(Guid documentId) =>
        $"/umbraco/management/api/v1/document/{documentId}/preview-url?providerAlias={Constants.UrlProviders.Content}";

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

    private async Task AuthenticateRestrictedUserAsync(int startContentId)
    {
        var alias = $"restricted{Guid.NewGuid():N}";

        IUserGroup userGroup = new UserGroupBuilder()
            .WithAlias(alias)
            .WithName(alias)
            .WithAllowedSections([Constants.Applications.Content])
            .WithPermissions(new HashSet<string> { ActionBrowse.ActionLetter })
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
