using System.Linq.Expressions;
using System.Net;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.SchemaLockdown;

public class ReadingDocumentTypesIsStillPermittedTests : SchemaLockdownApiTest<AllowedAtRootDocumentTypeController>
{
    protected override Expression<Func<AllowedAtRootDocumentTypeController, object>> MethodSelector { get; set; }
        = x => x.AllowedAtRoot(CancellationToken.None, 0, 100);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockDocumentTypesConfigurator>();

    [Test]
    public async Task AllowedAtRoot_Returns_Ok()
    {
        var response = await Client.GetAsync(Url);

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
