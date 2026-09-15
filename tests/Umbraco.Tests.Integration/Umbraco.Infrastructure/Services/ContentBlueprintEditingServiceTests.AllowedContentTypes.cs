using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [Test]
    public async Task Cannot_Create_For_Element_Type()
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("elementTest")
            .WithName("Element Test")
            .WithContentVariation(ContentVariation.Nothing)
            .WithIsElement(true)
            .Build();
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = elementType.Key,
            Variants = [new VariantModel { Name = "Blueprint For Element Type" }],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        });
    }

    [Test]
    public async Task Cannot_Create_For_Content_Type_Excluded_By_Content_Type_Filter()
    {
        var contentType = await CreateInvariantContentType();
        ExcludingContentTypeFilter.ExcludedContentTypeKey = contentType.Key;

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = [new VariantModel { Name = "Blueprint For Excluded Type" }],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotAllowed, result.Status);
        });
    }

    [Test]
    public async Task Can_Create_For_Content_Type_Not_Excluded_By_Content_Type_Filter()
    {
        var contentType = await CreateInvariantContentType();
        ExcludingContentTypeFilter.ExcludedContentTypeKey = Guid.NewGuid();

        var createModel = new ContentBlueprintCreateModel
        {
            ContentTypeKey = contentType.Key,
            Variants = [new VariantModel { Name = "Blueprint For Allowed Type" }],
        };

        var result = await ContentBlueprintEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        });
    }
}
