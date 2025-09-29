using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.ContentTypeEditing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DocumentType.Root;

public class RootDocumentTypeTreeControllerTests : ManagementApiUserGroupTestBase<RootDocumentTypeTreeController>
{
    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private Guid _key;

    [SetUp]
    public async Task Setup()
    {
        _key = Guid.NewGuid();
        await ContentTypeEditingService.CreateAsync(new ContentTypeCreateModel { Key = _key, Name = "Test", Alias = "test" }, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<RootDocumentTypeTreeController, object>> MethodSelector =>
        x => x.Root(CancellationToken.None, 0, 100, false);

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
