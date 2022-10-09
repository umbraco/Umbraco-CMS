using AutoFixture;
using AutoFixture.AutoMoq;
using Umbraco.Cms.Tests.UnitTests.AutoFixture.Customizations;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture;

internal static class UmbracoAutoMoqFixtureFactory
{
    internal static IFixture CreateDefaultFixture() =>
        new Fixture()
            .Customize(new AutoMoqCustomization { ConfigureMembers = true })
            .Customize(new OmitRecursionCustomization())
            .Customize(new UmbracoCustomizations());
}
