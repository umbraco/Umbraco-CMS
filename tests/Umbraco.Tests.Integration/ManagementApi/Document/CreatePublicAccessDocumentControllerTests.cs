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

public class CreatePublicAccessDocumentControllerTests : ManagementApiUserGroupTestBase<CreatePublicAccessDocumentController>
{

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();
    protected IMemberService MemberService => GetRequiredService<IMemberService>();
    protected IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();
    private Guid _key;

    [SetUp]
    public async Task Setup()
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
            InvariantName = Guid.NewGuid().ToString(),
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            }
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _key = response.Result.Content.Key;



        MemberCreateModel memberCreateModel = new()
        {
            Username = "member",
            Email = "",
            ContentTypeKey = Guid.NewGuid(),
            Password = "password",
            IsApproved = true,
            InvariantProperties = new List<PropertyValueModel>(),


        };

        await MemberEditingService.CreateAsync(memberCreateModel, Constants.Security.SuperUserKey);

    }

    protected override Expression<Func<CreatePublicAccessDocumentController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, Guid.NewGuid(), null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        PublicAccessRequestModel publicAccessRequestModel = new() { MemberUserNames = null, MemberGroupNames = null , LoginDocument = new ReferenceByIdModel(Guid.Empty), ErrorDocument = new ReferenceByIdModel(Guid.Empty) };

        return await Client.PostAsync(Url, JsonContent.Create(publicAccessRequestModel));
    }
}
