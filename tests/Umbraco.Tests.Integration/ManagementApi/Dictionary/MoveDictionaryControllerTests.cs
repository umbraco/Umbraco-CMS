using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Dictionary;

[TestFixture]
public class MoveDictionaryControllerTests : DictionaryBaseTest<MoveDictionaryController>
{
    private Guid _dictionaryId;
    private Guid _targetDictionaryId;

    protected override Expression<Func<MoveDictionaryController, object>> MethodSelector =>
        x => x.Move(_dictionaryId, null);

    [SetUp]
    public void Setup()
    {
        _dictionaryId = CreateDictionaryItem("TestDictionaryItem");
        _targetDictionaryId = CreateDictionaryItem("TestTargetDictionary");
    }

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
            new() { TargetId = _targetDictionaryId };
        return await Client.PostAsync(Url, JsonContent.Create(moveDictionaryRequestModel));
    }
}
