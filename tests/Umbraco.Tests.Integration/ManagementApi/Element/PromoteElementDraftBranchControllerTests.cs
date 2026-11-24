using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Element;

public class PromoteElementDraftBranchControllerTests : ManagementApiUserGroupTestBase<PromoteElementDraftBranchController>
{
    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementDraftBranchService ElementDraftBranchService => GetRequiredService<IElementDraftBranchService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _elementKey;

    private Guid _branchKey;

    [SetUp]
    public async Task Setup()
    {
        var elementType = new ContentTypeBuilder()
            .WithAlias(Guid.NewGuid().ToString())
            .WithName("Test Element")
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ElementCreateModel
        {
            ContentTypeKey = elementType.Key,
            ParentKey = null,
            Variants = [new VariantModel { Name = "Test Element" }],
        };
        var response = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success, $"Failed to create element: {response.Status}");
        _elementKey = response.Result!.Content!.Key;

        var branchAttempt = await ElementDraftBranchService.CreateAsync(_elementKey, "Black Friday", Constants.Security.SuperUserKey);
        _branchKey = branchAttempt.Result!.Key;
    }

    protected override Expression<Func<PromoteElementDraftBranchController, object>> MethodSelector =>
        x => x.Promote(CancellationToken.None, _elementKey, _branchKey, new PromoteElementDraftBranchRequestModel());

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
        await Client.PutAsync(Url, JsonContent.Create(new PromoteElementDraftBranchRequestModel()));

    [Test]
    public async Task As_Admin_Promoting_Copies_The_Branchs_Content_Onto_The_Default_Draft_Without_Publishing()
    {
        HttpResponseMessage response = await AuthorizedRequest(Constants.Security.AdminGroupKey, "Admin");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        IContentDraftBranch? branchAfter = ElementDraftBranchService.GetByKey(_elementKey, _branchKey);
        Assert.That(branchAfter, Is.Null);

        IElement? defaultDraft = GetRequiredService<IElementService>().GetById(_elementKey);
        Assert.That(defaultDraft!.Published, Is.False);
    }
}
