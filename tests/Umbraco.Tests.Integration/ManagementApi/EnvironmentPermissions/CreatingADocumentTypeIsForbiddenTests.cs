using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.EnvironmentPermissions;

public class CreatingADocumentTypeIsForbiddenTests : EnvironmentPermissionsApiTest<CreateDocumentTypeController>
{
    protected override Expression<Func<CreateDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.Create(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.EnvironmentPermissionsConfigurators().Add<LockDocumentTypesConfigurator>();

    [Test]
    public async Task Create_Returns_Forbidden()
    {
        CreateDocumentTypeRequestModel createModel = new()
        {
            Alias = "test" + Guid.NewGuid().ToString("N"),
            Name = "Test",
            Id = Guid.NewGuid(),
            Icon = "icon-document",
        };

        var response = await Client.PostAsync(Url, JsonContent.Create(createModel));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
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
