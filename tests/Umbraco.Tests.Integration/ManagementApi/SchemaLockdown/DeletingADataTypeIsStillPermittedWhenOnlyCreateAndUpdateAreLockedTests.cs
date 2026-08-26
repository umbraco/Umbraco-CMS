using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// A configurator that blocks only some operations on an entity type leaves the untouched operations governed by
/// the real request pipeline, not just by the rules in isolation.
/// </summary>
public class DeletingADataTypeIsStillPermittedWhenOnlyCreateAndUpdateAreLockedTests : ManagementApiTest<DeleteDataTypeController>
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private Guid _dataTypeKey;

    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypeCreateAndUpdateConfigurator>();

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
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

        await AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true);
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
    private sealed class LockDataTypeCreateAndUpdateConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaLockdownRules rules)
        {
            rules.Block(Constants.UdiEntityType.DataType, SchemaOperation.Create);
            rules.Block(Constants.UdiEntityType.DataType, SchemaOperation.Update);
        }
    }
}
