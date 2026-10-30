using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Deleting a locked data type is denied. The id doesn't exist, so reaching the action would return 404 instead.
/// </summary>
public class DeletingADataTypeIsForbiddenTests : SchemaLockdownApiTest<DeleteDataTypeController>
{
    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }
        = x => x.Delete(CancellationToken.None, Guid.Empty);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypesConfigurator>();

    [Test]
    public async Task Delete_Returns_Forbidden()
    {
        var response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Locks data types, and nothing else.
    /// </summary>
    private sealed class LockDataTypesConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.DataType);
    }
}
