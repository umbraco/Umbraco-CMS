using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Item;

public class ItemDocumentTypeItemControllerTests : ManagementApiUserGroupTestBase<ItemDocumentTypeItemController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _key = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key, Name = Guid.NewGuid().ToString(), Alias = Guid.NewGuid().ToString() }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ItemDocumentTypeItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { Guid.NewGuid() });

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
