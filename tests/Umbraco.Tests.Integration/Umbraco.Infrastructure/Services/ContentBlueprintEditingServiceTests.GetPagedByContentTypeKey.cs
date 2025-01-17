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
        var contentType = CreateInvariantContentType();

        for (var i = 1; i < 6; i++)
        {
            var createModel = new ContentBlueprintCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = $"Blueprint {i}",
            };

            await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        }

        var result = await ContentBlueprintEditingService.GetPagedByContentTypeAsync(contentType.Key, 0, 2);

        var pagedResult = result.Result;
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            Assert.IsNotNull(pagedResult);
        });
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, pagedResult.Items.Count());
            Assert.AreEqual(5, pagedResult.Total);
        });
    }

    [Test]
    public async Task Cannot_Get_Paged_With_Non_Existing_Content_Type()
    {
        var result = await ContentBlueprintEditingService.GetPagedByContentTypeAsync(Guid.NewGuid(), 0, 10);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.ContentTypeNotFound, result.Status);
            Assert.IsNull(result.Result);
        });
    }
}
