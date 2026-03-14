// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

    /// <summary>
    /// Provides a base class for unit tests related to composition in Umbraco Core, offering common setup and utilities for composing tests.
    /// </summary>
public abstract class ComposingTestBase
{
    protected TypeLoader TypeLoader { get; private set; }

    protected virtual IEnumerable<Assembly> AssembliesToScan
        => new[]
        {
            GetType().Assembly, // this assembly only
        };

    /// <summary>
    /// Initializes the test setup before each test runs.
    /// </summary>
    [SetUp]
    public void Initialize()
    {
        var typeFinder = TestHelper.GetTypeFinder();
        TypeLoader = new TypeLoader(
            typeFinder,
            Mock.Of<ILogger<TypeLoader>>(),
            AssembliesToScan);
    }
}
