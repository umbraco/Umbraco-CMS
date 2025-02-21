using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType;

public class ByKeyDocumentTypeControllerTests : ManagementApiUserGroupTestBase<ByKeyDocumentTypeController>
{
    private IContentTypeEditingService _contentTypeEditingService;
    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _contentTypeEditingService = GetRequiredService<IContentTypeEditingService>();
        _key = Guid.NewGuid();
        await _contentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key, Name = "Test", Alias = "test" }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ByKeyDocumentTypeController, object>> MethodSelector =>
        x => x.ByKey(CancellationToken.None, _key);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
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
