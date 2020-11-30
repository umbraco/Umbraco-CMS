using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.Integration.Testing;

// this class has NO NAMESPACE
// it applies to the whole assembly

[SetUpFixture]
// ReSharper disable once CheckNamespace
public class TestsSetup
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
        LocalDbTestDatabase.KillLocalDb();
        SqlDeveloperTestDatabase.Instance?.Finish();
        Console.WriteLine("TOTAL TESTS DURATION: {0}", _stopwatch.Elapsed);
    }
}
