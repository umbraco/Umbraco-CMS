using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.User.Current;

public class GetElementPermissionsCurrentUserControllerTests : ManagementApiUserGroupTestBase<GetElementPermissionsCurrentUserController>
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private Guid _elementKey;

    [SetUp]
    public async Task SetUp()
    {
        // Content Type
        var contentType = ContentTypeBuilder.CreateBasicContentType(
            name: Guid.NewGuid().ToString(),
            alias: Guid.NewGuid().ToString());
        contentType.IsElement = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        var response = await ElementEditingService.CreateAsync(
            new ElementCreateModel
            {
                ContentTypeKey = contentType.Key,
                ParentKey = null,
                Variants = [new VariantModel { Name = "Test Element Instance" }],
            },
            Constants.Security.SuperUserKey);
        Assert.IsTrue(response.Success, $"Failed to create element with status {response.Status}.");
        _elementKey = response.Result.Content!.Key;
    }

    protected override Expression<Func<GetElementPermissionsCurrentUserController, object>> MethodSelector
        => x => x.GetPermissions(CancellationToken.None, new HashSet<Guid> { _elementKey });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.OK };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel
        => new() { ExpectedStatusCode = HttpStatusCode.Unauthorized };
}
