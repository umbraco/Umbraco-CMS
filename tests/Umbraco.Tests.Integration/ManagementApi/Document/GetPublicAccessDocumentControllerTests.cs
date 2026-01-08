using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class GetPublicAccessDocumentControllerTests : ManagementApiUserGroupTestBase<GetPublicAccessDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

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

        // Public Access
        PublicAccessEntrySlim publicAccessEntry = new()
        {
            ContentId = _contentDefaultPageKey,
            ErrorPageId = _contentErrorPageKey,
            LoginPageId = _contentLoginPageKey,
            MemberUserNames = [member.Email],
            MemberGroupNames = [],
        };
        await PublicAccessService.CreateAsync(publicAccessEntry);
    }

    protected override Expression<Func<GetPublicAccessDocumentController, object>> MethodSelector =>
        x => x.GetPublicAccess(CancellationToken.None, _contentDefaultPageKey);

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
}
