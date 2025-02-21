using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Media;

[TestFixture]
public class ByKeyMediaControllerTests : ManagementApiUserGroupTestBase<ByKeyMediaController>
{
    private Guid _mediaKey = Guid.Empty;

    // We need to setup a media item. The reason for this is that the permission MediaPermissionByResource requires a media item to be present otherwise you will get the forbidden response.
    [SetUp]
    public async Task Setup()
    {
        var mediaService = GetRequiredService<IMediaService>();
        var media = mediaService.CreateMedia("Test", -1, Constants.Conventions.MediaTypes.Folder);
        mediaService.Save(media);
        _mediaKey = media.Key;
    }

    protected override Expression<Func<ByKeyMediaController, object>> MethodSelector => x => x.ByKey(CancellationToken.None, _mediaKey);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
