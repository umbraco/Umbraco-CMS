using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class FetchDocumentTypesControllerTests : ManagementApiUserGroupTestBase<FetchDocumentTypesController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private Guid _key1;
    private Guid _key2;

    [SetUp]
    public async Task Setup()
    {
        _key1 = Guid.NewGuid();
        _key2 = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key1, Name = "Type1", Alias = "type1" }, Constants.Security.SuperUserKey);
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key2, Name = "Type2", Alias = "type2" }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<FetchDocumentTypesController, object>> MethodSelector =>
        x => x.Fetch(CancellationToken.None, new FetchRequestModel());

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        var requestModel = new FetchRequestModel
        {
            Ids = [new(_key1), new(_key2)]
        };
        return await Client.PostAsync(Url, JsonContent.Create(requestModel));
    }

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
