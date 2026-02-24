using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class EFCoreModelCustomizerIntegrationTests : UmbracoIntegrationTest
{
    [Test]
    public void Registered_Model_Customizer_Is_Applied_Via_Pooled_Factory()
    {
        var dbContext = Services.GetRequiredService<UmbracoDbContext>();

        Assert.IsNotNull(dbContext);

        // The customizer adds a comment annotation to the WebhookDto entity type.
        // If it was invoked during OnModelCreating, the annotation will be present.
        var entityType = dbContext.Model.FindEntityType(typeof(WebhookDto));
        Assert.IsNotNull(entityType);

        var annotation = entityType!.FindAnnotation(WebhookDtoModelCustomizer.AnnotationName);
        Assert.IsNotNull(annotation, "Model customizer was not invoked — the annotation is missing from the model.");
        Assert.That(annotation!.Value, Is.EqualTo("applied"));
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddEFCoreModelCustomizer<WebhookDtoModelCustomizer>();

    private class WebhookDtoModelCustomizer : IEFCoreModelCustomizer<WebhookDto>
    {
        public const string AnnotationName = "Test:ModelCustomizerApplied";

        public void Customize(EntityTypeBuilder<WebhookDto> builder)
            => builder.HasAnnotation(AnnotationName, "applied");
    }
}
