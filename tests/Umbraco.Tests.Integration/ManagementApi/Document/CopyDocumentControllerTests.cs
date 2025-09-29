using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document;

[TestFixture]
public class CopyDocumentControllerTests : ManagementApiUserGroupTestBase<CopyDocumentController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _targetKey;
    private Guid _originalId;

    [SetUp]
    public async Task Setup()
    {
        var template = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id, name: Guid.NewGuid().ToString(), alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Target
        var parentCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var responseParent = await ContentEditingService.CreateAsync(parentCreateModel, Constants.Security.SuperUserKey);
        _targetKey = responseParent.Result.Content.Key;

        // Original
        var childCreateModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            TemplateKey = template.Key,
            ParentKey = Constants.System.RootKey,
            InvariantName = Guid.NewGuid().ToString(),
        };
        var responseChild = await ContentEditingService.CreateAsync(childCreateModel, Constants.Security.SuperUserKey);
        _originalId = responseChild.Result.Content.Key;

        await ContentEditingService.MoveToRecycleBinAsync(_originalId, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<CopyDocumentController, object>> MethodSelector =>
        x => x.Copy(CancellationToken.None, _originalId, null);

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
        CopyDocumentRequestModel copyDocumentRequestModel = new()
        {
            Target = new ReferenceByIdModel(_targetKey), RelateToOriginal = true, IncludeDescendants = true,
        };

        return await Client.PostAsync(Url, JsonContent.Create(copyDocumentRequestModel));
    }
}
