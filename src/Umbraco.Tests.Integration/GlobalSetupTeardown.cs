// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.Integration.Testing;

// ReSharper disable once CheckNamespace

/// <summary>
/// Global setup and teardown.
/// </summary>
/// <remarks>
/// This class has NO NAMESPACE so it applies to the whole assembly.
/// </remarks>
[SetUpFixture]
public class GlobalSetupTeardown
{
    private Stopwatch _stopwatch;

    [OneTimeSetUp]
    public void SetUp()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        LocalDbTestDatabase.Instance?.Finish();
        SqlDeveloperTestDatabase.Instance?.Finish();
        Console.WriteLine("TOTAL TESTS DURATION: {0}", _stopwatch.Elapsed);
    }
}
