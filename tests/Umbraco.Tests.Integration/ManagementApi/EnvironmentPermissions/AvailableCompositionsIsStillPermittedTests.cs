using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

internal sealed class AvailableCompositionsIsStillPermittedTests : EnvironmentPermissionsApiTest<AvailableCompositionDocumentTypeController>
{
    protected override Expression<Func<AvailableCompositionDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AvailableCompositions(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDocumentTypesConfigurator>();

    [Test]
    public async Task AvailableCompositions_Returns_Ok()
    {
        DocumentTypeCompositionRequestModel requestModel = new();

        var response = await Client.PostAsync(Url, JsonContent.Create(requestModel));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Locks document types, and nothing else.
    /// </summary>
    private sealed class LockDocumentTypesConfigurator : IEnvironmentPermissionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(IEnvironmentPermissionsBuilder builder)
            => builder.Deny(Constants.Permissions.Entities.DocumentType.Create, Constants.Permissions.Entities.DocumentType.Update, Constants.Permissions.Entities.DocumentType.Delete);
    }
}
