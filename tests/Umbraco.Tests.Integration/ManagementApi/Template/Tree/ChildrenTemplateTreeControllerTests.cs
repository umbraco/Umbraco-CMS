using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Template.Tree;

public class ChildrenTemplateTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenTemplateTreeController>
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private Guid _parentTemplateKey;

    [SetUp]
    public async Task SetUp()
    {
        // Parent Template
        var parentTemplate = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        parentTemplate.IsMasterTemplate = true;
        var responseParent = await TemplateService.CreateAsync(parentTemplate, Constants.Security.SuperUserKey);
        _parentTemplateKey = responseParent.Result.Key;

        // Child Template
        var childTemplate = TemplateBuilder.CreateTextPageTemplate(Guid.NewGuid().ToString());
        childTemplate.MasterTemplateAlias = parentTemplate.Alias;
        await TemplateService.CreateAsync(childTemplate, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenTemplateTreeController, object>> MethodSelector => x => x.Children(CancellationToken.None, _parentTemplateKey, 0, 100);

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
