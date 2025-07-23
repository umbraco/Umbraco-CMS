using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Get(bool variant)
    {
        var content = await (variant ? CreateCultureVariantContent() : CreateInvariantContent());

        var result = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(result);
        Assert.AreEqual(content.Key, result.Key);
    }

    [Test]
    public async Task Cannot_Get_Non_Existing()
    {
        var result = await ContentEditingService.GetAsync(Guid.NewGuid());
        Assert.IsNull(result);
    }
}
