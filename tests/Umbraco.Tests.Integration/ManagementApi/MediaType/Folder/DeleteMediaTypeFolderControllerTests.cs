using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Folder;

public class DeleteMediaTypeFolderControllerTests : ManagementApiUserGroupTestBase<DeleteMediaTypeFolderController>
{
    private IMediaTypeContainerService MediaTypeContainerService => GetRequiredService<IMediaTypeContainerService>();

    private Guid _mediaTypeContainerKey;

    [SetUp]
    public async Task SetUp()
    {
        _mediaTypeContainerKey = Guid.NewGuid();
        await MediaTypeContainerService.CreateAsync(_mediaTypeContainerKey, "TestFolder", Constants.System.RootKey , Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<DeleteMediaTypeFolderController, object>> MethodSelector =>
        x => x.Delete(CancellationToken.None, _mediaTypeContainerKey);

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
