using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

/// <summary>
/// Locking other entity types leaves webhooks creatable: only the entity types a configurator names are locked.
/// </summary>
public class LockingOtherSchemaTypesDoesNotGovernWebhooksTests : EnvironmentPermissionsApiTest<CreateWebhookController>
{
    protected override Expression<Func<CreateWebhookController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockOtherSchemaTypesConfigurator>();

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
    private sealed class LockOtherSchemaTypesConfigurator : IEnvironmentPermissionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(IEnvironmentPermissionsBuilder builder)
            => builder.Deny(
                Constants.Permissions.Entities.DocumentType.Create,
                Constants.Permissions.Entities.DocumentType.Update,
                Constants.Permissions.Entities.DocumentType.Delete,
                Constants.Permissions.Entities.MediaType.Create,
                Constants.Permissions.Entities.MediaType.Update,
                Constants.Permissions.Entities.MediaType.Delete,
                Constants.Permissions.Entities.MemberType.Create,
                Constants.Permissions.Entities.MemberType.Update,
                Constants.Permissions.Entities.MemberType.Delete,
                Constants.Permissions.Entities.DataType.Create,
                Constants.Permissions.Entities.DataType.Update,
                Constants.Permissions.Entities.DataType.Delete,
                Constants.Permissions.Entities.DictionaryItem.Create,
                Constants.Permissions.Entities.DictionaryItem.Update,
                Constants.Permissions.Entities.DictionaryItem.Delete,
                Constants.Permissions.Entities.Language.Create,
                Constants.Permissions.Entities.Language.Update,
                Constants.Permissions.Entities.Language.Delete);
    }
}
