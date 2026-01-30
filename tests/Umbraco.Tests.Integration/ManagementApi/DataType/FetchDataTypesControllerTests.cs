using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType;

public class FetchDataTypesControllerTests : ManagementApiUserGroupTestBase<FetchDataTypesController>
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private Guid _key1;
    private Guid _key2;

    [SetUp]
    public async Task Setup()
    {
        var dataType1 = new DataTypeBuilder()
            .WithId(0)
            .WithName("Test DataType 1")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.ListView)
                .Done()
            .Build();
        var response1 = await DataTypeService.CreateAsync(dataType1, Constants.Security.SuperUserKey);
        _key1 = response1.Result.Key;

        var dataType2 = new DataTypeBuilder()
            .WithId(0)
            .WithName("Test DataType 2")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.ListView)
                .Done()
            .Build();
        var response2 = await DataTypeService.CreateAsync(dataType2, Constants.Security.SuperUserKey);
        _key2 = response2.Result.Key;
    }

    protected override Expression<Func<FetchDataTypesController, object>> MethodSelector =>
        x => x.Fetch(CancellationToken.None, Array.Empty<Guid>());

    protected override async Task<HttpResponseMessage> ClientRequest()
        => await Client.GetAsync($"{Url}?id={_key1}&id={_key2}");

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
