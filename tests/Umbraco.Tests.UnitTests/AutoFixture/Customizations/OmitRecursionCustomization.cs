using AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture.Customizations;

internal sealed class OmitRecursionCustomization : ICustomization
{
    /// <summary>
    /// Customizes the specified fixture to omit recursion by adding an OmitOnRecursionBehavior.
    /// </summary>
    /// <param name="fixture">The fixture to customize.</param>
    public void Customize(IFixture fixture) =>
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
}
