using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.MemberType.Tree;

public class ChildrenMemberTypeTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenMemberTypeTreeController>
{
    private IMemberTypeEditingService MemberTypeEditingService => GetRequiredService<IMemberTypeEditingService>();

    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private Guid _mediaTypeFolderKey;
    private Guid _mediaTypeKey;

    [SetUp]
    public async Task SetUp()
    {
        _mediaTypeFolderKey = Guid.NewGuid();
        await ContentTypeContainerService.CreateAsync(_mediaTypeFolderKey, "Folder", null, Constants.Security.SuperUserKey);

        _mediaTypeKey = Guid.NewGuid();
        MemberTypeCreateModel memberTypeCreateModel = new() { Name = Guid.NewGuid().ToString(), Alias = Guid.NewGuid().ToString(), Key = _mediaTypeKey, ContainerKey = _mediaTypeFolderKey };
        await MemberTypeEditingService.CreateAsync(memberTypeCreateModel, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenMemberTypeTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _mediaTypeFolderKey, 0, 100, false);

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
