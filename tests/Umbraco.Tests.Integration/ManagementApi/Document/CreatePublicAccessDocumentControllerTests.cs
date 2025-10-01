using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class
    CreatePublicAccessDocumentControllerTests : ManagementApiUserGroupTestBase<CreatePublicAccessDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private Guid _contentDefaultPageKey;
    private Guid _contentLoginPageKey;
    private Guid _contentErrorPageKey;

    [SetUp]
    public async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // Content Type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Default page
        var createDefaultPageModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var responseDefaultPage = await ContentEditingService.CreateAsync(createDefaultPageModel, Constants.Security.SuperUserKey);
        _contentDefaultPageKey = responseDefaultPage.Result.Content.Key;

        // Login page
        var createLoginPageModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var responseLoginPage = await ContentEditingService.CreateAsync(createLoginPageModel, Constants.Security.SuperUserKey);
        _contentLoginPageKey = responseLoginPage.Result.Content.Key;

        // Error page
        var createErrorPageModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var responseErrorPage = await ContentEditingService.CreateAsync(createErrorPageModel, Constants.Security.SuperUserKey);
        _contentErrorPageKey = responseErrorPage.Result.Content.Key;

        // Member
        var memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        var member = MemberService.CreateMember("test", "test@test.com", "T. Est", memberType.Alias);
        MemberService.Save(member);
    }

    protected override Expression<Func<CreatePublicAccessDocumentController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, _contentDefaultPageKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        PublicAccessRequestModel publicAccessRequestModel = new()
        {
            MemberUserNames = ["test@test.com"],
            MemberGroupNames = [],
            LoginDocument = new ReferenceByIdModel(_contentLoginPageKey),
            ErrorDocument = new ReferenceByIdModel(_contentErrorPageKey),
        };

        return await Client.PostAsync(Url, JsonContent.Create(publicAccessRequestModel));
    }
}
