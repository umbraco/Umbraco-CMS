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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify deletion
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Null);
    }

    [Test]
    public async Task Cannot_DeleteRecycleBin_FromOutsideOfRecycleBin()
    {
        var element = await CreateInvariantElement();

        var result = await ElementEditingService.DeleteFromRecycleBinAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotInTrash));

        // re-get and verify that deletion failed
        element = await ElementEditingService.GetAsync(element.Key);
        Assert.That(element, Is.Not.Null);
    }

    [Test]
    public async Task Cannot_DeleteRecycleBin_Non_Existing()
    {
        var result = await ElementEditingService.DeleteFromRecycleBinAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }
}
