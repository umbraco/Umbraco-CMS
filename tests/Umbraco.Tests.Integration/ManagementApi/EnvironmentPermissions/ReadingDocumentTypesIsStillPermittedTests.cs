using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

internal sealed class ReadingDocumentTypesIsStillPermittedTests : EnvironmentPermissionsApiTest<AllowedAtRootDocumentTypeController>
{
    protected override Expression<Func<AllowedAtRootDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AllowedAtRoot(CancellationToken.None, 0, 100);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDocumentTypesConfigurator>();

    [Test]
    public async Task AllowedAtRoot_Returns_Ok()
    {
        var response = await Client.GetAsync(Url);

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
