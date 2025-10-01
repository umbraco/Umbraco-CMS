using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Folder;

public class ByKeyMediaTypeFolderControllerTests : ManagementApiUserGroupTestBase<ByKeyMediaTypeFolderController>
{
    private IMediaTypeContainerService MediaTypeContainerService => GetRequiredService<IMediaTypeContainerService>();

    private Guid _mediaTypeFolderKey;

    [SetUp]
    public async Task SetUp()
    {
        _mediaTypeFolderKey = Guid.NewGuid();
        await MediaTypeContainerService.CreateAsync(_mediaTypeFolderKey, "TestFolder", Constants.System.RootKey, Constants.Security.SuperUserKey);
    }


    protected override Expression<Func<ByKeyMediaTypeFolderController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, _mediaTypeFolderKey);

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
}
