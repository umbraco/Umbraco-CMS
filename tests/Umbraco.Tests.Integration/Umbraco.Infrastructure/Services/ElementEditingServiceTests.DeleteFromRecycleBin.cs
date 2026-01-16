using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_DeleteRecycleBin_FromRecycleBin(bool variant)
    {
        var element = await (variant ? CreateCultureVariantElement() : CreateInvariantElement());
        await ElementEditingService.MoveToRecycleBinAsync(element.Key,  Constants.Security.SuperUserKey);

        var result = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.IsNull(element);
    }

    [Test]
    public async Task Cannot_DeleteRecycleBin_FromOutsideOfRecycleBin()
    {
        var element = await CreateInvariantElement();

        var result = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotInTrash, result.Status);

        // re-get and verify that deletion failed
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.NotNull(element);
    }

    [Test]
    public async Task Cannot_DeleteRecycleBin_Non_Existing()
    {
        var result = await ElementEditingService.DeleteFromRecycleBinAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }
}
