using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

public class MoveDictionaryControllerTests : ManagementApiUserGroupTestBase<MoveDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _originalId = new Guid();
    private Guid _targetId = new Guid();

    [SetUp]
    public async Task Setup()
    {
        var original = new DictionaryItem(Constants.System.RootKey, _originalId.ToString());
        var target = new DictionaryItem(Constants.System.RootKey, _targetId.ToString());

        await DictionaryItemService.CreateAsync(original, Constants.Security.SuperUserKey);
        await DictionaryItemService.CreateAsync(target, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<MoveDictionaryController, object>> MethodSelector =>
        x => x.Move(CancellationToken.None, Guid.NewGuid(), null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.NotFound
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
        ExpectedStatusCode = HttpStatusCode.NotFound
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
            new() { Target = null };
        return await Client.PutAsync(Url, JsonContent.Create(moveDictionaryRequestModel));
    }
}
