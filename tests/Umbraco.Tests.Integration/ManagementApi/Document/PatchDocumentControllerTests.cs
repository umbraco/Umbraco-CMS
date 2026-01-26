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

public class PatchDocumentControllerTests : ManagementApiUserGroupTestBase<PatchDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _templateKey;
    private Guid _documentKey;

    [SetUp]
    public async Task Setup()
    {
        // Template
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        var templateResponse = await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);
        _templateKey = templateResponse.Result.Key;

        // Content Type
        var contentType = ContentTypeBuilder.CreateTextPageContentType(
            defaultTemplateId: template.Id,
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = _templateKey,
            ParentKey = Constants.System.RootKey,
            Variants = new List<VariantModel> { new() { Name = "Original Name" } },
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (response.Result.Content is null)
        {
            throw new ArgumentNullException(nameof(response.Result.Content), "Setup failed: No content returned from CreateAsync");
        }

        _documentKey = response.Result.Content.Key;
    }

    protected override Expression<Func<PatchDocumentController, object>> MethodSelector =>
        x => x.Patch(CancellationToken.None, _documentKey, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        PatchDocumentRequestModel patchModel = new()
        {
            Variants = new DocumentVariantPatchModel[]
            {
                new() { Culture = null, Segment = null, Name = "Updated Name" },
            },
        };

        var httpContent = JsonContent.Create(patchModel);
        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/merge-patch+json");
        return await Client.PatchAsync(Url, httpContent);
    }
}
