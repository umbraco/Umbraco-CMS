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

public abstract class ComposingTestBase
{
    protected TypeLoader TypeLoader { get; private set; }

    protected virtual IEnumerable<Assembly> AssembliesToScan
        => new[]
        {
            GetType().Assembly, // this assembly only
        };

    [SetUp]
    public void Initialize()
    {
        var typeFinder = TestHelper.GetTypeFinder();
        TypeLoader = new TypeLoader(
            typeFinder,
            new VaryingRuntimeHash(),
            NoAppCache.Instance,
            new DirectoryInfo(TestHelper.GetHostingEnvironment().MapPathContentRoot(Constants.SystemDirectories.TempData)),
            Mock.Of<ILogger<TypeLoader>>(),
            Mock.Of<IProfiler>(),
            false,
            AssembliesToScan);
    }
}
