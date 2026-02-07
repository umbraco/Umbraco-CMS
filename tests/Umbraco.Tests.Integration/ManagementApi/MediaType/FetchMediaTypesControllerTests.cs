using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MediaType;

public class FetchMediaTypesControllerTests : ManagementApiUserGroupTestBase<FetchMediaTypesController>
{
    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private Guid _key1;
    private Guid _key2;

    [SetUp]
    public async Task Setup()
    {
        _key1 = Guid.NewGuid();
        _key2 = Guid.NewGuid();
        await MediaTypeEditingService.CreateAsync(new MediaTypeCreateModel { Key = _key1, Name = "MediaType1", Alias = "mediaType1" }, Constants.Security.SuperUserKey);
        await MediaTypeEditingService.CreateAsync(new MediaTypeCreateModel { Key = _key2, Name = "MediaType2", Alias = "mediaType2" }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<FetchMediaTypesController, object>> MethodSelector =>
        x => x.Fetch(CancellationToken.None, Array.Empty<Guid>());

    protected override async Task<HttpResponseMessage> ClientRequest()
        => await Client.GetAsync($"{Url}?id={_key1}&id={_key2}");

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
