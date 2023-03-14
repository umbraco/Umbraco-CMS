// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using AutoFixture.NUnit3;

namespace Umbraco.Cms.Tests.UnitTests.AutoFixture;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class AutoMoqDataAttribute : AutoDataAttribute
{
    /// <summary>
    ///     Uses AutoFixture to automatically mock (using Moq) the injected types. E.g when injecting interfaces.
    ///     AutoFixture is used to generate concrete types. If the concrete type required some types injected, the
    ///     [Frozen] can be used to ensure the same variable is injected and available as parameter for the test
    /// </summary>
    public AutoMoqDataAttribute()
        : base(UmbracoAutoMoqFixtureFactory.CreateDefaultFixture)
    {
    }
}
