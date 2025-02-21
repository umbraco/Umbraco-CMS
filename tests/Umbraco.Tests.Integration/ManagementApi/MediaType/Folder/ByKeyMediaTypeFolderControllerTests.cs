using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType.Folder;

[TestFixture]
public class ByKeyMediaTypeFolderControllerTests : ManagementApiUserGroupTestBase<ByKeyMediaTypeFolderController>
{
    private IMediaTypeContainerService _mediaTypeContainerService;
    private Guid _mediaTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _mediaTypeKey = Guid.NewGuid();
        _mediaTypeContainerService = GetRequiredService<IMediaTypeContainerService>();
        await _mediaTypeContainerService.CreateAsync(_mediaTypeKey, "TestFolder", null, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ByKeyMediaTypeFolderController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, _mediaTypeKey);

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
