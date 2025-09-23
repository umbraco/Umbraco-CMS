using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal class ContentTypeReferenceServiceTests : ContentTypeEditingServiceTestsBase
{
    private IContentTypeReferenceService ContentTypeReferenceService => GetRequiredService<IContentTypeReferenceService>();
    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    [Test]
    public async Task Get_Referenced_DocumentKeys_New()
    {
        // var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        var contentTypeCreateModel = ContentTypeEditingBuilder.CreateSimpleContentType();
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(contentTypeCreateModel, Constants.Security.SuperUserKey)).Result!;
        var contentCreateModel = ContentEditingBuilder.CreateSimpleContent(compositionContentType.Key);
        var content = await ContentEditingService.CreateAsync(contentCreateModel, Constants.Security.SuperUserKey);
        var keys = await ContentTypeReferenceService.GetReferencedDocumentKeysAsync(compositionContentType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(content.Result.Content.Key));
        });
    }

    [Test]
    public async Task Get_Referenced_DocumentTypeKeys()
    {
        var propertyType1 = ContentTypePropertyTypeModel("Test Property 1", "testProperty1");
        var propertyType2 = ContentTypePropertyTypeModel("Test Property 2", "testProperty2");

        var compositionCreateModel = ContentTypeCreateModel("Composition", "composition");
        compositionCreateModel.Properties = new[] { propertyType1 };
        var compositionContentType = (await ContentTypeEditingService.CreateAsync(compositionCreateModel, Constants.Security.SuperUserKey)).Result!;

        var createModel = ContentTypeCreateModel("Test", "test");
        createModel.Properties = new[] { propertyType2 };
        var contentType = (await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey)).Result!;

        var updateModel = ContentTypeUpdateModel("Test", "test");
        updateModel.Properties = new[] { propertyType2 };
        updateModel.Compositions = new[]
        {
            new Composition { Key = compositionContentType.Key, CompositionType = CompositionType.Composition }
        };

        await ContentTypeEditingService.UpdateAsync(contentType, updateModel, Constants.Security.SuperUserKey);

        var keys = await ContentTypeReferenceService.GetReferencedDocumentTypeKeysAsync(compositionContentType.Key, CancellationToken.None, 0, 100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(keys.Items.Count(), Is.EqualTo(1));
            Assert.That(keys.Items.First(), Is.EqualTo(contentType.Key));
        });

    }
}
