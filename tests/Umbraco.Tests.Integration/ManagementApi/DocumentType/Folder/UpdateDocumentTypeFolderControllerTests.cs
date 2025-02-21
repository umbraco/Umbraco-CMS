using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Folder;

public class UpdateDocumentTypeFolderControllerTests : ManagementApiUserGroupTestBase<UpdateDocumentTypeFolderController>
{
    private IContentTypeContainerService _contentTypeContainerService;
    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _contentTypeContainerService = GetRequiredService<IContentTypeContainerService>();
        _key = Guid.NewGuid();
        await _contentTypeContainerService.CreateAsync(_key, "Test", null, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<UpdateDocumentTypeFolderController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _key, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
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
        UpdateFolderResponseModel updateDocumentRequestModel = new() { Name = "Test", };
        return await Client.PutAsync(Url, JsonContent.Create(updateDocumentRequestModel));
    }
}
