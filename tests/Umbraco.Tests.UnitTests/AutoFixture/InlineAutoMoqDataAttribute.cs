// Copyright (c) Umbraco.
// See LICENSE for more details.

using AutoFixture.NUnit3;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture;

/// <summary>
/// An attribute that enables parameterized unit tests with inline data, automatically providing mock objects using Moq and AutoFixture.
/// Useful for simplifying test setup by injecting both custom and auto-generated data into test methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
/// <summary>
///     Uses AutoFixture to automatically mock (using Moq) the injected types. E.g when injecting interfaces.
///     AutoFixture is used to generate concrete types. If the concrete type required some types injected, the
///     [Frozen] can be used to ensure the same variable is injected and available as parameter for the test
/// </summary>
/// <param name="arguments">The arguments to be passed to the inline data.</param>
    public InlineAutoMoqDataAttribute(params object[] arguments)
        : base(UmbracoAutoMoqFixtureFactory.CreateDefaultFixture, arguments)
    {
    }
}
