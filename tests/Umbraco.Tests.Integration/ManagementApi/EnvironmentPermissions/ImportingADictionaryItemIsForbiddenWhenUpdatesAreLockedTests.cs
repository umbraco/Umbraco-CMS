using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

/// <summary>
/// Importing dictionary items can update existing ones, so it's denied while updates are locked.
/// </summary>
public class ImportingADictionaryItemIsForbiddenWhenUpdatesAreLockedTests : EnvironmentPermissionsApiTest<ImportDictionaryController>
{
    protected override Expression<Func<ImportDictionaryController, object>> MethodSelector { get; set; }
        = x => x.Import(CancellationToken.None, null!);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDictionaryItemUpdatesConfigurator>();

    [Test]
    public async Task Import_Returns_Forbidden()
    {
        ImportDictionaryRequestModel importModel = new() { TemporaryFile = new ReferenceByIdModel(Guid.NewGuid()) };

        HttpResponseMessage response = await Client.PostAsync(Url, JsonContent.Create(importModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.That(
            response.Headers.GetValues(Constants.Headers.DeniedPermission),
            Is.EqualTo(new[] { Constants.Permissions.Entities.DictionaryItem.Update }));
    }

    /// <summary>
    /// Denies updating dictionary items, and nothing else.
    /// </summary>
    private sealed class LockDictionaryItemUpdatesConfigurator : IEnvironmentPermissionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(IEnvironmentPermissionsBuilder builder)
            => builder.Deny(Constants.Permissions.Entities.DictionaryItem.Update);
    }
}
