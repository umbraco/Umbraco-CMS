using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetDocumentPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetDocumentPermissionsCurrentUserController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _documentKey;

    [SetUp]
    public async Task SetUp()
    {
        // Content Type
        var contentType = ContentTypeBuilder.CreateBasicContentType(
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var response = await ContentEditingService.CreateAsync(
            ContentEditingBuilder.CreateBasicContent(contentType.Key, null),
            Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success, $"Failed to create content with status {response.Status}.");
        _documentKey = response.Result.Content!.Key;
    }

    protected override Expression<Func<GetDocumentPermissionsCurrentUserController, object>> MethodSelector
        => x => x.GetPermissions(CancellationToken.None, null!);

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
        => await Client.GetAsync($"{Url}?id={_documentKey}");
}
