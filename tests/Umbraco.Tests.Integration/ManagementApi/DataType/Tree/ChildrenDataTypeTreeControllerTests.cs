using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.DataType.Tree;

public class ChildrenDataTypeTreeControllerTests : ManagementApiUserGroupTestBase<ChildrenDataTypeTreeController>
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    private Guid _dataTypeContainerKey;

    [SetUp]
    public async Task Setup()
    {
        _dataTypeContainerKey = Guid.NewGuid();
        await DataTypeContainerService.CreateAsync(_dataTypeContainerKey, "Folder", Constants.System.RootKey, Constants.Security.SuperUserKey);

        var savedContainer = await DataTypeContainerService.GetAsync(_dataTypeContainerKey);
        var containerId = savedContainer?.Id ?? Constants.System.Root;

        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.ListView)
                .Done()
            .WithParentId(containerId)
            .Build();

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
    }

    protected override Expression<Func<ChildrenDataTypeTreeController, object>> MethodSelector =>
        x => x.Children(CancellationToken.None, _dataTypeContainerKey, 0, 100, false);

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
        ExpectedStatusCode = HttpStatusCode.Forbidden,
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
