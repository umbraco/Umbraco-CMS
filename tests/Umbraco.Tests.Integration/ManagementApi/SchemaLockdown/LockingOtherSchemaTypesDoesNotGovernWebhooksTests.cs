using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

/// <summary>
/// Locking other entity types leaves webhooks creatable: only the entity types a configurator names are locked.
/// </summary>
public class LockingOtherSchemaTypesDoesNotGovernWebhooksTests : SchemaLockdownApiTest<CreateWebhookController>
{
    protected override Expression<Func<CreateWebhookController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockOtherSchemaTypesConfigurator>();

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
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.Deny(
                Constants.Permissions.DocumentType.Create,
                Constants.Permissions.DocumentType.Update,
                Constants.Permissions.DocumentType.Delete,
                Constants.Permissions.MediaType.Create,
                Constants.Permissions.MediaType.Update,
                Constants.Permissions.MediaType.Delete,
                Constants.Permissions.MemberType.Create,
                Constants.Permissions.MemberType.Update,
                Constants.Permissions.MemberType.Delete,
                Constants.Permissions.DataType.Create,
                Constants.Permissions.DataType.Update,
                Constants.Permissions.DataType.Delete,
                Constants.Permissions.DictionaryItem.Create,
                Constants.Permissions.DictionaryItem.Update,
                Constants.Permissions.DictionaryItem.Delete,
                Constants.Permissions.Language.Create,
                Constants.Permissions.Language.Update,
                Constants.Permissions.Language.Delete);
    }
}
