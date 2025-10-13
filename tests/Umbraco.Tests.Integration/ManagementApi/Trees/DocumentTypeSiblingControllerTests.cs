using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Trees;

public class DocumentTypeSiblingControllerTests : ManagementApiUserGroupTestBase<SiblingsDocumentTypeTreeController>
{
    private IContentTypeContainerService ContentTypeContainerService => GetRequiredService<IContentTypeContainerService>();

    private Guid _firstContentTypeKey;
    private Guid _secondContentTypeKey;

    [SetUp]
    public async Task Setup()
    {
        // First sibling folder
        _firstContentTypeKey = Guid.NewGuid();
        await ContentTypeContainerService.CreateAsync(_firstContentTypeKey, "Test", Constants.System.RootKey, Constants.Security.SuperUserKey);

        // Second sibling folder
        _secondContentTypeKey = Guid.NewGuid();
        await ContentTypeContainerService.CreateAsync(_secondContentTypeKey, "SecondTest", Constants.System.RootKey,
            Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<SiblingsDocumentTypeTreeController, object>> MethodSelector =>
        x => x.Siblings(CancellationToken.None, _firstContentTypeKey, 0, 0, false);

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
