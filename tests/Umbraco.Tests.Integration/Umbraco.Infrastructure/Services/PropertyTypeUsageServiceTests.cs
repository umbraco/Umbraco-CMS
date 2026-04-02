using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PropertyTypeUsageServiceTests : UmbracoIntegrationTestWithContent
{
    private IPropertyTypeUsageService PropertyTypeUsageService => GetRequiredService<IPropertyTypeUsageService>();

    [TestCase(TextpageContentTypeKey, "title", true, true, PropertyTypeOperationStatus.Success)]
    [TestCase("1D3A8E6E-2EA9-4CC1-B229-1AEE19821523", "title", false, false, PropertyTypeOperationStatus.ContentTypeNotFound)]
    [TestCase(TextpageContentTypeKey, "missingProperty", true, false, PropertyTypeOperationStatus.Success)]
    public async Task Can_Check_For_Saved_Property_Values(Guid contentTypeKey, string propertyAlias, bool expectedSuccess, bool expectedResult, PropertyTypeOperationStatus expectedOperationStatus)
    {
        Attempt<bool, PropertyTypeOperationStatus> resultAttempt = await PropertyTypeUsageService.HasSavedPropertyValuesAsync(contentTypeKey, propertyAlias);
        Assert.AreEqual(expectedSuccess, resultAttempt.Success);
        Assert.AreEqual(expectedResult, resultAttempt.Result);
        Assert.AreEqual(expectedOperationStatus, resultAttempt.Status);
    }
}
