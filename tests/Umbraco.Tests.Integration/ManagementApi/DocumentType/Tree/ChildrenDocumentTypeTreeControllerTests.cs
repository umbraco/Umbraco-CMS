using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Tree;

public class ChildrenDocumentTypeTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDocumentTypeTreeController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private Guid _contentFolderKey;
    private Guid _contentTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _contentFolderKey = Guid.NewGuid();
        await ContentTypeContainerService.CreateAsync(_contentFolderKey,"Folder", null, Constants.Security.SuperUserKey);
        _contentTypeKey = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _contentTypeKey, Name = "Test", Alias = "test",  ContainerKey = _contentFolderKey}, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenDocumentTypeTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _contentFolderKey, 0, 100, false);

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
