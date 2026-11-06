using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Document.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Document.Tree;

public class BatchDocumentTreeControllerTests : ManagementApiUserGroupTestBase<BatchDocumentTreeController>
{
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private Guid _documentKey;

    [SetUp]
    public new async Task Setup()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString() }],
        };
        var response = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        _documentKey = response.Result.Content!.Key;
    }

    protected override Expression<Func<BatchDocumentTreeController, object>> MethodSelector =>
        x => x.Batch(CancellationToken.None, new HashSet<Guid> { _documentKey }, null);

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };
}
