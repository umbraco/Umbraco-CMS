using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Can_Get_Paged()
    {
        var contentType = await CreateInvariantContentType();

        for (var i = 1; i < 6; i++)
        {
            var createModel = new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                Variants = [new VariantModel { Name = $"Blueprint {i}" }],
            };

            await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        var result = await ContentBlueprintEditingService.GetPagedByContentTypeAsync(contentType.Key, 0, 2);

        var pagedResult = result.Result;
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(pagedResult, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(pagedResult.Items.Count(), Is.EqualTo(2));
            Assert.That(pagedResult.Total, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Cannot_Get_Paged_With_Non_Existing_Content_Type()
    {
        var result = await ContentBlueprintEditingService.GetPagedByContentTypeAsync(Guid.NewGuid(), 0, 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.ContentTypeNotFound));
            Assert.That(result.Result, Is.Null);
        });
    }
}
