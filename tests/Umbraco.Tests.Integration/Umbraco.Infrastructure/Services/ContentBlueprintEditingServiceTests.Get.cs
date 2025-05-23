using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get(bool variant)
    {
        var blueprint = await (variant ? CreateVariantContentBlueprint() : CreateInvariantContentBlueprint());

        var result = await ContentBlueprintEditingService.GetAsync(blueprint.Key);
        Assert.IsNotNull(result);
        Assert.AreEqual(blueprint.Key, result.Key);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing()
    {
        var result = await ContentBlueprintEditingService.GetAsync(Guid.NewGuid());
        Assert.IsNull(result);
    }
}
