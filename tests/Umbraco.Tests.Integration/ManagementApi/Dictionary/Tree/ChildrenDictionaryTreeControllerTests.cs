using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary.Tree;

public class ChildrenDictionaryTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDictionaryTreeController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _parentDictionaryKey;

    [SetUp]
    public async Task Setup()
    {
        var responseParent = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());

        _parentDictionaryKey = responseParent.Key;

        await DictionaryItemService.CreateAsync(responseParent, Constants.Security.SuperUserKey);

        var responseChild = new DictionaryItem(_parentDictionaryKey, Guid.NewGuid().ToString());

        await DictionaryItemService.CreateAsync(responseChild, Constants.Security.SuperUserKey);

    }

    protected override Expression<Func<ChildrenDictionaryTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _parentDictionaryKey, 0, 100);

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
        ExpectedStatusCode = HttpStatusCode.OK,
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
