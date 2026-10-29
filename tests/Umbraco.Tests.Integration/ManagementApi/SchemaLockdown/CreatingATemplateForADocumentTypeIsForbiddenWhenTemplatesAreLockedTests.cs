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

/// <summary>
/// Creating a template for a document type is denied when only templates are locked, as the endpoint declares both
/// entity types. The id doesn't exist, so reaching the action would return 404 instead.
/// </summary>
public class CreatingATemplateForADocumentTypeIsForbiddenWhenTemplatesAreLockedTests
    : SchemaLockdownApiTest<CreateDocumentTypeTemplateController>
{
    protected override Expression<Func<CreateDocumentTypeTemplateController, object>> MethodSelector { get; set; }
        = x => x.CreateTemplate(CancellationToken.None, Guid.Empty, null!);

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.SchemaLockdownConfigurators().Add<LockTemplatesOnlyConfigurator>();

    [Test]
    public async Task Create_Returns_Forbidden()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Url,
            new CreateDocumentTypeTemplateRequestModel { Alias = "whatever", Name = "Whatever" });

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.That(
            response.Headers.GetValues(Constants.Headers.SchemaLockdown),
            Is.EqualTo(new[] { $"{Constants.UdiEntityType.Template}:create" }));
    }

    /// <summary>
    /// Denies templates, and nothing else. Document types stay open.
    /// </summary>
    private sealed class LockTemplatesOnlyConfigurator : ISchemaLockdownConfigurator
    {
        /// <inheritdoc />
        public void Configure(ISchemaRestrictionsBuilder builder)
            => builder.BlockMutations(Constants.UdiEntityType.Template);
    }
}
