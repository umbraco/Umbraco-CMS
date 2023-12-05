using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Bogus.DataSets;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

[TestFixture]
public class UpdateDictionaryControllerTests : DictionaryBaseTest<UpdateDictionaryController>
{
    private Guid _dictionaryId;

    protected override Expression<Func<UpdateDictionaryController, object>> MethodSelector =>
            x => x.Update(_dictionaryId, null);

    [SetUp]
    public void Setup() => _dictionaryId = CreateDictionaryItem("TestDictionaryItem");

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
        UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel =
            new() { Name = "TestDictionary", Translations = { } };
        return await Client.PutAsync(Url, JsonContent.Create(updateDictionaryItemRequestModel));
    }
}
