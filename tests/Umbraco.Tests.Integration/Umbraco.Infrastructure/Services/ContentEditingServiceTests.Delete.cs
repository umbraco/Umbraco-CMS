using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromRecycleBin(bool variant)
    {
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());
        await ContentEditingService.MoveToRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);

        var result = await ContentEditingService.DeleteFromRecycleBinAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNull(content);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete_FromOutsideOfRecycleBin(bool variant)
    {
        var content = await (variant ? CreateVariantContent() : CreateInvariantContent());

        var result = await ContentEditingService.DeleteAsync(content.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);

        // re-get and verify deletion
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNull(content);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ContentEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
    }
}
