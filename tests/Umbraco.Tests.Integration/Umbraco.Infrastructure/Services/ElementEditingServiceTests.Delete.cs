using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromOutsideOfRecycleBin(bool variant)
    {
        var element = await (variant ? CreateCultureVariantElement() : CreateInvariantElement());

        var result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ElementEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }
}
