using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class MoveDictionaryControllerTests : ManagementApiUserGroupTestBase<MoveDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _originalId;
    private Guid _targetId;

    [SetUp]
    public async Task Setup()
    {
        var originalDictionaryItem = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());
        var targetDictionaryItem = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());

        _originalId = originalDictionaryItem.Key;
        _targetId = targetDictionaryItem.Key;

        await DictionaryItemService.CreateAsync(originalDictionaryItem, Constants.Security.SuperUserKey);
        await DictionaryItemService.CreateAsync(targetDictionaryItem, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<MoveDictionaryController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, _originalId, null);

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
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Forbidden
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.Unauthorized
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        MoveDictionaryRequestModel moveDictionaryRequestModel =
            new() { Target = new ReferenceByIdModel(_targetId) };
        return await Client.PutAsync(Url, JsonContent.Create(moveDictionaryRequestModel));
    }
}
