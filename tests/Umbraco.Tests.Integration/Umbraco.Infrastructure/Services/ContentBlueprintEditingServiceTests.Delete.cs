using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Delete(bool variant)
    {
        var blueprint = await (variant ? CreateVariantContentBlueprint() : CreateInvariantContentBlueprint());

        var result = await ContentBlueprintEditingService.DeleteAsync(blueprint.Key, Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        });

        // re-get and verify deletion
        blueprint = await ContentBlueprintEditingService.GetAsync(blueprint.Key);
        Assert.IsNull(blueprint);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ContentBlueprintEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.NotFound, result.Status);
        });
    }
}
