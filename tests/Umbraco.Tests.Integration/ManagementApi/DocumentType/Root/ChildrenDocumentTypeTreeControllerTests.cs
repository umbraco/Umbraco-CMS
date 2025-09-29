using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Root;

public class ChildrenDocumentTypeTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDocumentTypeTreeController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private Guid _documentFolderKey;
    private Guid _documentTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _documentFolderKey = Guid.NewGuid();
        await ContentTypeContainerService.CreateAsync(_documentFolderKey,"Folder", null, Constants.Security.SuperUserKey);
        _documentTypeKey = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _documentTypeKey, Name = "Test", Alias = "test",  ContainerKey = _documentFolderKey}, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenDocumentTypeTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _documentFolderKey, 0, 100, false);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };
}
