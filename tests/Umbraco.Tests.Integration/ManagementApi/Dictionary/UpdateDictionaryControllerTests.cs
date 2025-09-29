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

public class UpdateDictionaryControllerTests : ManagementApiUserGroupTestBase<UpdateDictionaryController>
{
    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    private Guid _dictionaryKey;

    [SetUp]
    public async Task Setup()
    {
        var response = new DictionaryItem(Constants.System.RootKey, Guid.NewGuid().ToString());

        _dictionaryKey = response.Key;

        await DictionaryItemService.CreateAsync(response, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<UpdateDictionaryController, object>> MethodSelector => x => x.Update(CancellationToken.None, _dictionaryKey, null);

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

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel =
            new() { Name = Guid.NewGuid().ToString(), Translations = { } };
        return await Client.PutAsync(Url, JsonContent.Create(updateDictionaryItemRequestModel));
    }
}
