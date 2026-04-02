using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType;

public class DeleteMediaTypeControllerTests : ManagementApiUserGroupTestBase<DeleteMediaTypeController>
{
    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _mediaTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _mediaTypeKey = Guid.NewGuid();
        MediaTypeCreateModel mediaTypeCreateModel = new() { Name = "Test", Alias = "test", Key = _mediaTypeKey };
        await MediaTypeEditingService.CreateAsync(mediaTypeCreateModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<DeleteMediaTypeController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _mediaTypeKey);

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

    protected override async Task<HttpResponseMessage> ClientRequest() => await Client.DeleteAsync(Url);
}
