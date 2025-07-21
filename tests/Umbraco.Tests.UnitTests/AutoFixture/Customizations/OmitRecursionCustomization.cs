using AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture.Customizations;

internal sealed class OmitRecursionCustomization : ICustomization
{
    public void Customize(IFixture fixture) =>
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
}
