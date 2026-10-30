using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// A denial names the entity type and operation in a header, so it can be told apart from a permission 403.
/// </summary>
public class DenialNamesWhatWasDeniedInAHeaderTests : SchemaLockdownApiTest<DeleteDataTypeController>
{
    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }
        = x => x.Delete(CancellationToken.None, Guid.Empty);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypeDeletionConfigurator>();

    [Test]
    public async Task Forbidden_Response_Names_The_Entity_Type_And_Operation()
    {
        HttpResponseMessage response = await Client.DeleteAsync(Url);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.That(
            response.Headers.GetValues(Constants.Headers.SchemaLockdown),
            Is.EqualTo(new[] { $"{Constants.UdiEntityType.DataType}:delete" }));
    }

    /// <summary>
    /// Denies deleting data types, and nothing else.
    /// </summary>
    private sealed class LockDataTypeDeletionConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Block(Constants.UdiEntityType.DataType, SchemaOperation.Delete);
    }
}
