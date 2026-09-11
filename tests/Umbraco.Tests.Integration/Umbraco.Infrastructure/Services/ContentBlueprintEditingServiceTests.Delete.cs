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
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        });

        // re-get and verify deletion
        blueprint = await ContentBlueprintEditingService.GetAsync(blueprint.Key);
        Assert.That(blueprint, Is.Null);
    }

    [Test]
    public async Task Cannot_Delete_Non_Existing()
    {
        var result = await ContentBlueprintEditingService.DeleteAsync(Guid.NewGuid(), Constants.Security.SuperUserKey);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.NotFound));
        });
    }
}
