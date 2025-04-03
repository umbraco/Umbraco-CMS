using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType;

[TestFixture]
public class UpdateMediaTypeControllerTests : ManagementApiUserGroupTestBase<UpdateMediaTypeController>
{
    private IMediaTypeEditingService _mediaTypeEditingService;
    private Guid _mediaTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _mediaTypeKey = Guid.NewGuid();
        _mediaTypeEditingService = GetRequiredService<IMediaTypeEditingService>();
        MediaTypeCreateModel mediaTypeCreateModel = new() { Name = "Test", Alias = "test", Key = _mediaTypeKey };
        await _mediaTypeEditingService.CreateAsync(mediaTypeCreateModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<UpdateMediaTypeController, object>> MethodSelector =>
        x => x.Update(CancellationToken.None, _mediaTypeKey, null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateMediaTypeRequestModel updateMediaTypeRequest = new() { Name = "Tester", Alias = "test", Icon = "icon-picture", AllowedAsRoot = true };

        return await Client.PutAsync(Url, JsonContent.Create(updateMediaTypeRequest));
    }
}
