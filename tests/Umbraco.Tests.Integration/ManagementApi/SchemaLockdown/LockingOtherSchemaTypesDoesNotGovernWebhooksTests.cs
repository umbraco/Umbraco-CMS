using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locking a broad set of other entity types leaves webhooks creatable, so a lockdown cannot sweep up an entity
/// type no configurator named.
/// </summary>
public class LockingOtherSchemaTypesDoesNotGovernWebhooksTests : ManagementApiTest<CreateWebhookController>
{
    protected override Expression<Func<CreateWebhookController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockOtherSchemaTypesConfigurator>();

    [SetUp]
    public override void Setup()
    {
        InMemoryConfiguration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"] = "https://localhost";
        base.Setup();
        AuthenticateClientAsync(Client, "admin@test.com", "1234567890", true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Create_Returns_Created()
    {
        CreateWebhookRequestModel createModel = new()
        {
            Url = "https://example.com",
            Events = new[] { "umbracoContentPublish" },
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>
    /// Locks a broad set of schema entity types, none of which are webhooks.
    /// </summary>
    private sealed class LockOtherSchemaTypesConfigurator : ISchemaLockdownConfigurator
    {
        private static readonly string[] EntityTypes =
        [
            Constants.UdiEntityType.DocumentType,
            Constants.UdiEntityType.MediaType,
            Constants.UdiEntityType.MemberType,
            Constants.UdiEntityType.DataType,
            Constants.UdiEntityType.DictionaryItem,
            Constants.UdiEntityType.Language,
        ];

        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
        {
            foreach (var entityType in EntityTypes)
            {
                builder.BlockMutations(entityType);
            }
        }
    }
}
