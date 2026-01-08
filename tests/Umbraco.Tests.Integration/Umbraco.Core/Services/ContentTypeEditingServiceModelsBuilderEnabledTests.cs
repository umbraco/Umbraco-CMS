using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public class ContentTypeEditingServiceModelsBuilderEnabledTests : ContentTypeEditingServiceModelsBuilderEnabledTestsBase
{
    // test some properties from IPublishedContent
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.Id) })]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.Name) })]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.SortOrder) })]
    // test some properties from IPublishedElement
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.Properties) })]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.ContentType) })]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.Key) })]
    // test some methods from IPublishedContent
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.IsDraft) })]
    [TestCaseSource(nameof(DifferentCapitalizedAlias), new object[] { nameof(IPublishedContent.IsPublished) })]
    public async Task Cannot_Use_Invalid_ModelsBuilder_PropertyType_Alias_When_ModelsBuilderIsEnabled(
        string propertyTypeAlias)
    {
        var propertyType = ContentTypePropertyTypeModel("Test Property", propertyTypeAlias);
        var createModel = ContentTypeCreateModel("Test", propertyTypes: new[] { propertyType });

        var result = await ContentTypeEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentTypeOperationStatus.InvalidPropertyTypeAlias, result.Status);
        });
    }
}
