using NUnit.Framework;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentTypeSchemaServiceTests : UmbracoIntegrationTestWithContentEditing
{
    // Required to register IPublishedContentTypeCache (even though it uses in-memory cache, not HybridCache)
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddUmbracoHybridCache();

    private IContentTypeSchemaService ContentTypeSchemaService =>
        GetRequiredService<IContentTypeSchemaService>();

    private IContentTypeService ContentTypeService =>
        GetRequiredService<IContentTypeService>();

    private IMediaTypeService MediaTypeService =>
        GetRequiredService<IMediaTypeService>();

    [Test]
    public void Can_Get_DocumentTypes()
    {
        // Act
        var result = ContentTypeSchemaService.GetDocumentTypes();

        // Assert - count should match the content type service
        Assert.That(result, Has.Count.EqualTo(ContentTypeService.Count()));

        var contentTypeSchema = result.FirstOrDefault(ct => ct.Alias == ContentType.Alias);
        Assert.That(contentTypeSchema, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(contentTypeSchema!.Alias, Is.EqualTo("umbTextpage"));
            Assert.That(contentTypeSchema.SchemaId, Is.EqualTo("UmbTextpage"));
            Assert.That(contentTypeSchema.Properties, Has.Count.EqualTo(1));
        });

        // Verify property details - the base class creates a "title" property
        var titleProperty = contentTypeSchema!.Properties.First();
        Assert.Multiple(() =>
        {
            Assert.That(titleProperty.Alias, Is.EqualTo("title"));
            Assert.That(titleProperty.EditorAlias, Is.EqualTo("Umbraco.TextArea"));
            Assert.That(titleProperty.Inherited, Is.False);
            Assert.That(titleProperty.DeliveryApiClrType, Is.EqualTo(typeof(string)));
        });
    }

    [Test]
    public void Can_Get_MediaTypes()
    {
        // Act
        var result = ContentTypeSchemaService.GetMediaTypes();

        // Assert - count should match the media type service
        Assert.That(result, Has.Count.EqualTo(MediaTypeService.Count()));

        // Built-in Image media type should be present with its properties
        var imageSchema = result.FirstOrDefault(mt => mt.Alias == "Image");
        Assert.That(imageSchema, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(imageSchema!.SchemaId, Is.EqualTo("Image"));
            Assert.That(imageSchema.Properties, Is.Not.Empty);
        });

        // Verify the umbracoFile property exists on Image
        var umbracoFileProperty = imageSchema!.Properties.FirstOrDefault(p => p.Alias == "umbracoFile");
        Assert.That(umbracoFileProperty, Is.Not.Null);
        Assert.That(umbracoFileProperty!.EditorAlias, Is.EqualTo("Umbraco.ImageCropper"));
        Assert.That(umbracoFileProperty.DeliveryApiClrType, Is.EqualTo(typeof(ApiImageCropperValue)));
    }
}
