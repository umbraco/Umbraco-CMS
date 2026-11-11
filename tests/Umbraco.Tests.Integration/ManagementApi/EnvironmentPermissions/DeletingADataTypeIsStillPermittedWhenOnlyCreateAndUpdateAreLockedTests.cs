using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

/// <summary>
/// Operations a configurator didn't block on an entity type still go through.
/// </summary>
public class DeletingADataTypeIsStillPermittedWhenOnlyCreateAndUpdateAreLockedTests : EnvironmentPermissionsApiTest<DeleteDataTypeController>
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private Guid _dataTypeKey;

    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDataTypeCreateAndUpdateConfigurator>();

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        CreateDataType().GetAwaiter().GetResult();
    }

    private async Task CreateDataType()
    {
        var dataType = new DataTypeBuilder()
            .WithId(0)
            .WithName("Custom list view")
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .AddEditor()
                .WithAlias(Constants.PropertyEditors.Aliases.ListView)
                .Done()
            .Build();
        var result = await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        _dataTypeKey = result.Result.Key;

        MethodSelector = x => x.Delete(CancellationToken.None, _dataTypeKey);
    }

    [Test]
    public async Task Delete_Returns_Ok()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNull(DataTypeService.GetAsync(_dataTypeKey).GetAwaiter().GetResult());
    }

    /// <summary>
    /// Locks data type creation and update, leaving deletion permitted.
    /// </summary>
    private sealed class LockDataTypeCreateAndUpdateConfigurator : IEnvironmentPermissionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(IEnvironmentPermissionsBuilder builder)
        {
            builder.Deny(Constants.Permissions.DataType.Create);
            builder.Deny(Constants.Permissions.DataType.Update);
        }
    }
}
