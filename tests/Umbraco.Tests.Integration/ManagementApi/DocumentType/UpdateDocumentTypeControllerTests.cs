using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class UpdateDocumentTypeControllerTests : ManagementApiUserGroupTestBase<UpdateDocumentTypeController>
{
    private IContentTypeEditingService _contentTypeEditingService;
    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _contentTypeEditingService = GetRequiredService<IContentTypeEditingService>();
        _key = Guid.NewGuid();
        await _contentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key, Name = "Test", Alias = "test" }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<UpdateDocumentTypeController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _key, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateDocumentRequestModel updateDocumentRequestModel =
            new() { Template = new ReferenceByIdModel(Guid.NewGuid()), };

        return await Client.PutAsync(Url, JsonContent.Create(updateDocumentRequestModel));
    }
}
