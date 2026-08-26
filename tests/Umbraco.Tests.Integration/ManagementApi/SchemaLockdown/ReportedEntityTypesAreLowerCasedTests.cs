using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Server;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// The rules match entity types case-insensitively and report back whatever casing a configurator wrote, so the
/// endpoint settles the casing rather than leaving the backoffice to guess at it.
/// </summary>
public class ReportedEntityTypesAreLowerCasedTests : ManagementApiTest<SchemaLockdownServerController>
{
    protected override Expression<Func<SchemaLockdownServerController, object>> MethodSelector { get; set; }
        = x => x.SchemaLockdown(CancellationToken.None);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockAMixedCaseEntityTypeConfigurator>();

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Reported_Entity_Type_Is_Lower_Cased()
    {
        HttpResponseMessage response = await Client.GetAsync(Url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        ServerSchemaLockdownResponseModel? model =
            await response.Content.ReadFromJsonAsync<ServerSchemaLockdownResponseModel>();

        Assert.That(model, Is.Not.Null);
        Assert.That(
            model!.RestrictedEntityTypes.Select(x => x.EntityType),
            Is.EquivalentTo(new[] { Constants.UdiEntityType.DictionaryItem }));
    }

    /// <summary>
    /// Denies an operation naming the entity type in a casing the rules accept but the backoffice never uses.
    /// </summary>
    private sealed class LockAMixedCaseEntityTypeConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaLockdownRules rules)
            => rules.BlockMutations("Dictionary-Item");
    }
}
