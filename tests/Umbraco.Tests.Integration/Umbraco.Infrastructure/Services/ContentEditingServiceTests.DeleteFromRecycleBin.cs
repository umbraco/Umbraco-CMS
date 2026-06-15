using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_DeleteFromRecycleBin_If_InsideRecycleBin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.DeleteFromRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));

        // re-get and verify deletion
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_FromRecycleBin_Non_Existing()
    {
        var result = await ContentEditingService.DeleteFromRecycleBinAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Cannot_Delete_FromRecycleBin_If_Not_In_Recycle_Bin(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());

        var result = await ContentEditingService.DeleteFromRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotInTrash));

        // re-get and verify that deletion did not happen
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
    }
}
