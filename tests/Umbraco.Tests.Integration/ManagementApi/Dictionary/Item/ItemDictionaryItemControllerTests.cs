using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary.Item;

public class ItemDictionaryItemControllerTests : ManagementApiUserGroupTestBase<ItemDictionaryItemController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    [SetUp]
    public async Task Setup()
    {
        var dictionaryItem = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());
        _dictionaryKey = dictionaryItem.Key;
        await DictionaryItemService.CreateAsync(dictionaryItem, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ItemDictionaryItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _dictionaryKey });

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK,
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized,
    };
}
