using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Item;

public class AncestorsDocumentTypeItemControllerTests : ManagementApiUserGroupTestBase<AncestorsDocumentTypeItemController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private Guid _documentTypeKey;

    [SetUp]
    public async Task Setup()
    {
        _documentTypeKey = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _documentTypeKey, Name = Guid.NewGuid().ToString(), Alias = Guid.NewGuid().ToString() }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<AncestorsDocumentTypeItemController, object>> MethodSelector =>
        x => x.Ancestors(CancellationToken.None, new HashSet<Guid> { _documentTypeKey });

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
}
