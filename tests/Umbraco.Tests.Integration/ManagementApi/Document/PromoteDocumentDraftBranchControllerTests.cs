using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class PromoteDocumentDraftBranchControllerTests : ManagementApiUserGroupTestBase<PromoteDocumentDraftBranchController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentDraftBranchService ContentDraftBranchService => GetRequiredService<IContentDraftBranchService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private Guid _documentKey;

    private Guid _branchKey;

    [SetUp]
    public new async Task Setup()
    {
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel> { new() { Name = Guid.NewGuid().ToString() } },
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _documentKey = response.Result.Content.Key;

        var branchAttempt = await ContentDraftBranchService.CreateAsync(_documentKey, "Black Friday", Constants.Security.SuperUserKey);
        _branchKey = branchAttempt.Result!.Key;
    }

    protected override Expression<Func<PromoteDocumentDraftBranchController, object>> MethodSelector =>
        x => x.Promote(CancellationToken.None, _documentKey, _branchKey, new PromoteDocumentDraftBranchRequestModel());

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest() =>
        await Client.PutAsync(Url, JsonContent.Create(new PromoteDocumentDraftBranchRequestModel()));

    [Test]
    public async Task As_Admin_Promoting_Copies_The_Branchs_Content_Onto_The_Default_Draft_Without_Publishing()
    {
        HttpResponseMessage response = await AuthorizedRequest(Constants.Security.AdminGroupKey, "Admin");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        IContentDraftBranch? branchAfter = ContentDraftBranchService.GetByKey(_documentKey, _branchKey);
        Assert.That(branchAfter, Is.Null);

        IContent? defaultDraft = GetRequiredService<IContentService>().GetById(_documentKey);
        Assert.That(defaultDraft!.Published, Is.False);
    }

    [Test]
    public async Task Promote_Returns_Forbidden_When_User_Lacks_Permission_For_Requested_Culture()
    {
        await LanguageService.CreateAsync(new global::Umbraco.Cms.Core.Models.Language("da-DK", "Danish") { FallbackIsoCode = "en-US" }, Constants.Security.SuperUserKey);

        var userGroup = UserGroupBuilder.CreateUserGroup(
            alias: "restrictedCultureGroup",
            permissions: new[] { ActionPublish.ActionLetter, ActionUpdate.ActionLetter, ActionBrowse.ActionLetter }.ToHashSet());
        userGroup.HasAccessToAllLanguages = false;
        ILanguage danish = (await LanguageService.GetAsync("da-DK"))!;
        userGroup.AddAllowedLanguage(danish.Id);
        Attempt<IUserGroup, UserGroupOperationStatus> groupCreated =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.That(groupCreated.Success, Is.True);

        await AuthenticateClientAsync(Client, "restrictedcultureuser@test.com", UserPassword, groupCreated.Result.Key);

        // the user's only allowed language is da-DK - requesting en-US (a real, valid culture the user just
        // isn't permitted to touch) must be rejected at the authorization layer, before the service is ever
        // called - regardless of the fact that this particular document is invariant.
        HttpResponseMessage response = await Client.PutAsync(
            Url,
            JsonContent.Create(new PromoteDocumentDraftBranchRequestModel { Cultures = new HashSet<string> { "en-US" } }));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
}
