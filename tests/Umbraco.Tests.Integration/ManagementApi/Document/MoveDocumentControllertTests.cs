using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

[TestFixture]
public class MoveDocumentControllertTests : ManagementApiUserGroupTestBase<MoveDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    private Guid _targetId;

    [SetUp]
    public async Task Setup()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = "Test Move Document",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "bodyText", Value = "The body text" }
            }
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _targetId = response.Result.Key;
    }

    protected override Expression<Func<MoveDocumentController, object>> MethodSelector =>
        x => x.Move(Guid.NewGuid(), null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        ExpectedStatusCode = HttpStatusCode.NotFound
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveDocumentRequestModel voveDocumentRequestModel = new()
        {
            TargetId = _targetId,
        };

        return await Client.PutAsync(Url, JsonContent.Create(voveDocumentRequestModel));
    }
}
