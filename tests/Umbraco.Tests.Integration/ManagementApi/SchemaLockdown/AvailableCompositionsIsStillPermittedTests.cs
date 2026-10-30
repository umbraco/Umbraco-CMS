using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class AvailableCompositionsIsStillPermittedTests : SchemaLockdownApiTest<AvailableCompositionDocumentTypeController>
{
    protected override Expression<Func<AvailableCompositionDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AvailableCompositions(CancellationToken.None, null);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDocumentTypesConfigurator>();

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
    private sealed class LockDocumentTypesConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.DocumentType);
    }
}
