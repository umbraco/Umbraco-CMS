using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Exercises a locked mutation through the real request pipeline. The id names nothing, so the only response that
/// can precede the action running is the lockdown denial - reaching the action would answer with a not found.
/// </summary>
public class DeletingADataTypeIsForbiddenTests : ManagementApiTest<DeleteDataTypeController>
{
    protected override Expression<Func<DeleteDataTypeController, object>> MethodSelector { get; set; }
        = x => x.Delete(CancellationToken.None, Guid.Empty);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDataTypesConfigurator>();

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

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
        public void Configure(ISchemaLockdownRules rules)
            => rules.BlockMutations(Constants.UdiEntityType.DataType);
    }
}
