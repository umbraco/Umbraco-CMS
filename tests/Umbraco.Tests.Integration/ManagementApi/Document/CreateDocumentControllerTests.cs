using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

public class CreateDocumentControllerTests : ManagementApiUserGroupTestBase<CreateDocumentController>
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _templateKey;
    private Guid _contentTypeKey;

    [SetUp]
    public new async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        _templateKey = template.Key;

        // Content Type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        _contentTypeKey = contentType.Key;
    }

    protected override Expression<Func<CreateDocumentController, object>> MethodSelector =>
        x => x.Create(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Created
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
        ExpectedStatusCode = HttpStatusCode.Created
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        CreateDocumentRequestModel createDocumentRequestModel = new()
        {
            Template = new ReferenceByIdModel(_templateKey),
            DocumentType = new ReferenceByIdModel(_contentTypeKey),
            Parent = null,
            Id = Guid.NewGuid(),
            Values = new DocumentValueModel[] { },
            Variants = new DocumentVariantRequestModel[]
            {
                new() { Culture = null, Segment = null, Name = "The en-US name", },
            },
        };

        return await Client.PostAsync(Url, JsonContent.Create(createDocumentRequestModel));
    }

    [Test]
    public async Task Cannot_Create_Culture_Variant_Document_Without_Variants()
    {
        await AuthenticateClientAsync(Client, "test@umbraco.com", UserPassword, isAdmin: true);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.Variations = ContentVariation.Culture;
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        CreateDocumentRequestModel createDocumentRequestModel = new()
        {
            Template = null,
            DocumentType = new ReferenceByIdModel(contentType.Key),
            Parent = null,
            Values = new DocumentValueModel[] { },
            Variants = new DocumentVariantRequestModel[] { },
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createDocumentRequestModel));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
